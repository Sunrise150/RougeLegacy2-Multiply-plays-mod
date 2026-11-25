using System;
using System.Reflection;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v3.0.23：协作"共享世界状态"——原版一次性的【掉物易碎品】与【治疗树等特殊房】，
    /// 两端必须看到同一份状态（一端打碎/用完，另一端也立刻是打碎/用完的样子）。
    ///
    /// 背景：StageSaveData 里每个房间的 RoomSaveData 带 BreakableStates / DecoBreakableStates /
    /// IsRoomComplete 三组一次性标记。MapSync 只负责开局把主机地图拷给客机，且
    /// ResetRuntimeState 会把这些标记重置成"全新"（竞速各打各的需要），于是联机中途
    /// 一端打碎罐子/用完治疗树，另一端看到的还是完好的 —— 这就是用户反馈的现象。
    ///
    /// 方案（只做加法，不碰已验证的房间/门逻辑）：
    ///   1) 打碎 Breakable 时（postfix）广播 (biome, roomIndex, spawnIndex, subIndex, dir)；
    ///      收到端写 RoomSaveData + 对当前已实例化的道具立刻 ForceBrokenState(true)。
    ///   2) BaseSpecialRoomController.RoomCompleted() 时广播 (biome, roomIndex)；
    ///      收到端写 IsRoomComplete + 对当前已实例化的特殊设施立刻 DisableProp。
    ///   3) 状态是单调的（未用 -> 用过，不回退），天然幂等；收到端用 ForceBrokenState /
    ///      ForceRoomCompleted 而不是原生 Break / RoomCompleted，因此不会回声。
    ///
    /// key 用 spawn controller 在房间数组里的序号（种子同步后两端一致），不用 InstanceID。
    /// 协议 3011 -> 3012（新增 MsgType.WorldState），两端必须同版本。
    /// </summary>
    internal static class WorldStateSync
    {
        private const byte KindBreakable = 0;
        private const byte KindDecoBreakable = 1;
        private const byte KindSpecialComplete = 2;

        internal static bool Active
        {
            get { return Score.Style == PlayStyle.CoOp && DownedState.InSession
                          && NetworkManager.Instance != null && NetworkManager.Instance.Connected; }
        }

        // 收到远端状态时置位，避免 ForceBrokenState / ForceRoomCompleted 再次触发 postfix 回声。
        private static bool _applyingRemote;
        private static float _lastWarn;
        private static float _lastLog;

        private static readonly MethodInfo _forceRoomCompleted =
            AccessTools.Method(typeof(BaseSpecialRoomController), "ForceRoomCompleted");
        private static readonly MethodInfo _disableProp =
            AccessTools.Method(typeof(BaseSpecialPropController), "DisableProp", new[] { typeof(bool) });

        internal static void Apply(Harmony harmony)
        {
            try
            {
                var breakMethod = AccessTools.Method(typeof(Breakable), "Break", new[] { typeof(IDamageObj) });
                if (breakMethod != null)
                {
                    var post = new HarmonyMethod(typeof(WorldStateSync).GetMethod("BreakableBreakPostfix",
                        BindingFlags.Static | BindingFlags.NonPublic));
                    harmony.Patch(breakMethod, postfix: post);
                    Plugin.Log.LogInfo("[RL2MP] [WSTATE] 补丁 OK: Breakable.Break postfix（易碎品共享）。");
                }
                else Plugin.Log.LogWarning("[RL2MP] [WSTATE] Breakable.Break 未找到，易碎品共享不可用。");

                var completeMethod = AccessTools.Method(typeof(BaseSpecialRoomController), "RoomCompleted");
                if (completeMethod != null)
                {
                    var post2 = new HarmonyMethod(typeof(WorldStateSync).GetMethod("RoomCompletedPostfix",
                        BindingFlags.Static | BindingFlags.NonPublic));
                    harmony.Patch(completeMethod, postfix: post2);
                    Plugin.Log.LogInfo("[RL2MP] [WSTATE] 补丁 OK: RoomCompleted postfix（特殊房共享）。");
                }
                else Plugin.Log.LogWarning("[RL2MP] [WSTATE] RoomCompleted 未找到，特殊房共享不可用。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [WSTATE] 补丁失败: " + Plugin.PatchError(ex));
            }
        }

        // ---------------- 发送端 ----------------

        private static void BreakableBreakPostfix(Breakable __instance)
        {
            if (_applyingRemote || !Active) return;
            try
            {
                var prop = __instance.GetComponent<Prop>();
                if (prop == null) return;
                BaseRoom room = prop.Room;
                var psc = prop.PropSpawnController;
                if (room == null || psc == null) return;

                var all = room.SpawnControllerManager.PropSpawnControllers;
                if (all == null) return;
                int spawnIndex = -1, subIndex = 0;
                for (int i = 0; i < all.Length; i++)
                {
                    if (!ReferenceEquals(all[i], psc)) continue;
                    spawnIndex = i;
                    break;
                }
                if (spawnIndex < 0) return;

                byte kind = KindBreakable;
                // 判断是主道具本身还是它的易碎装饰物。
                if (psc.PropInstance == null || !ReferenceEquals(psc.PropInstance.Breakable, __instance))
                {
                    kind = KindDecoBreakable;
                    var decos = psc.BreakableDecoInstances;
                    subIndex = -1;
                    if (decos != null)
                    {
                        for (int k = 0; k < decos.Length; k++)
                            if (decos[k] != null && ReferenceEquals(decos[k].Breakable, __instance)) { subIndex = k + 1; break; }
                    }
                    if (subIndex < 0) return;
                }

                int biome = (int)BiomeType_RL.GetGroupedBiomeType(room.BiomeType);
                int roomIndex = room.BiomeControllerIndex;
                bool right = __instance.AttackerIsOnRight;
                Broadcast(kind, biome, roomIndex, spawnIndex, subIndex, right);

                if (Time.unscaledTime - _lastLog > 2f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [WSTATE] 广播易碎品破碎: biome=" + biome
                        + " room=" + roomIndex + " spawn=" + spawnIndex + " sub=" + subIndex);
                }
            }
            catch (Exception ex) { Warn("Breakable 广播失败: " + ex.Message); }
        }

        private static void RoomCompletedPostfix(BaseSpecialRoomController __instance)
        {
            if (_applyingRemote || !Active) return;
            try
            {
                BaseRoom room = __instance.Room;
                if (room == null) return;
                int biome = (int)BiomeType_RL.GetGroupedBiomeType(room.BiomeType);
                Broadcast(KindSpecialComplete, biome, room.BiomeControllerIndex, 0, 0, false);
                if (Time.unscaledTime - _lastLog > 2f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [WSTATE] 广播特殊房完成: biome=" + biome
                        + " room=" + room.BiomeControllerIndex + " type=" + __instance.SpecialRoomType);
                }
            }
            catch (Exception ex) { Warn("特殊房广播失败: " + ex.Message); }
        }

        private static void Broadcast(byte kind, int biome, int roomIndex, int spawnIndex, int subIndex, bool flag)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;
            var w = new NetDataWriter();
            w.Put((byte)MsgType.WorldState);
            w.Put(nm.LocalPlayerId);
            w.Put(kind);
            w.Put(biome);
            w.Put(roomIndex);
            w.Put(spawnIndex);
            w.Put(subIndex);
            w.Put(flag);
            nm.SendWorldState(w);
        }

        // ---------------- 接收端 ----------------

        /// <summary>由 NetworkManager 收到 WorldState 包后调用（已做 sender 校验/中转）。</summary>
        internal static void ApplyRemote(int senderId, byte kind, int biome, int roomIndex,
            int spawnIndex, int subIndex, bool flag)
        {
            if (!Active || senderId == NetworkManager.Instance.LocalPlayerId) return;
            if (biome <= 0 || biome >= 1000 || roomIndex < 0 || roomIndex > 4096) return;
            if (spawnIndex < 0 || spawnIndex > 4096 || subIndex < 0 || subIndex > 255) return;
            if (kind > KindSpecialComplete) return;

            _applyingRemote = true;
            try
            {
                if (kind == KindSpecialComplete)
                    ApplySpecial(biome, roomIndex);
                else
                    ApplyBreakable(kind, biome, roomIndex, spawnIndex, subIndex, flag);
            }
            finally { _applyingRemote = false; }
        }

        private static void ApplyBreakable(byte kind, int biome, int roomIndex, int spawnIndex, int subIndex, bool right)
        {
            // 1) 写存档：下次进这间房时原生 LoadRoomState 会据此恢复"已破"外观。
            var rd = SaveManager.StageSaveData != null
                ? SaveManager.StageSaveData.GetRoomSaveData((BiomeType)biome, roomIndex) : null;
            if (rd != null)
            {
                BaseRoom saveRoom = FindRoom(biome, roomIndex);
                var all = saveRoom != null && saveRoom.SpawnControllerManager != null
                    ? saveRoom.SpawnControllerManager.PropSpawnControllers : null;
                if (all != null && spawnIndex < all.Length)
                {
                    var psc = all[spawnIndex];
                    if (kind == KindBreakable)
                    {
                        int idx = SaveBreakableIndex(saveRoom, psc);
                        if (idx >= 0 && rd.BreakableStates != null && idx < rd.BreakableStates.Length)
                        {
                            rd.BreakableStates[idx].IsStateActive = false;
                            rd.BreakableStates[idx].AttackerIsOnRight = right;
                        }
                    }
                    else
                    {
                        int idx = SaveDecoIndex(saveRoom, psc, subIndex - 1);
                        if (idx >= 0 && rd.DecoBreakableStates != null && idx < rd.DecoBreakableStates.Length)
                        {
                            rd.DecoBreakableStates[idx].IsStateActive = false;
                            rd.DecoBreakableStates[idx].AttackerIsOnRight = right;
                        }
                    }
                }
            }

            // 2) 立刻作用于当前已实例化的道具（观战端正看着这间房时不能等下次进房）。
            BaseRoom room = FindRoom(biome, roomIndex);
            if (room == null || room.SpawnControllerManager == null) return;
            var spawners = room.SpawnControllerManager.PropSpawnControllers;
            if (spawners == null || spawnIndex >= spawners.Length) return;
            var sp = spawners[spawnIndex];
            if (sp == null) return;

            Breakable target = null;
            if (kind == KindBreakable)
            {
                if (sp.PropInstance != null) target = sp.PropInstance.Breakable;
            }
            else
            {
                var decos = sp.BreakableDecoInstances;
                int k = subIndex - 1;
                if (decos != null && k >= 0 && k < decos.Length && decos[k] != null)
                    target = decos[k].Breakable;
            }
            if (target == null || target.IsBroken) return;
            try
            {
                target.AttackerIsOnRight = right;
                target.ForceBrokenState(true);
            }
            catch (Exception ex) { Warn("应用易碎品状态失败: " + ex.Message); }
        }

        private static void ApplySpecial(int biome, int roomIndex)
        {
            var rd = SaveManager.StageSaveData != null
                ? SaveManager.StageSaveData.GetRoomSaveData((BiomeType)biome, roomIndex) : null;
            if (rd != null) rd.IsRoomComplete = true;

            BaseRoom room = FindRoom(biome, roomIndex);
            if (room == null) return;
            try
            {
                var controllers = room.GetComponentsInChildren<BaseSpecialRoomController>(true);
                for (int i = 0; i < controllers.Length; i++)
                {
                    if (_forceRoomCompleted != null)
                        _forceRoomCompleted.Invoke(controllers[i], null);   // 只置标记，不广播、不派发 relay
                }
                // 设施 Prop 是房间下的兄弟对象（不是控制器的子节点），必须从 room 起找。
                var props = room.GetComponentsInChildren<BaseSpecialPropController>(true);
                for (int k = 0; k < props.Length; k++)
                {
                    if (_disableProp != null)
                        _disableProp.Invoke(props[k], new object[] { false }); // InstantlyUsed，无首次动画
                }
            }
            catch (Exception ex) { Warn("应用特殊房状态失败: " + ex.Message); }
        }

        private static BaseRoom FindRoom(int biome, int roomIndex)
        {
            try
            {
                var ctrl = WorldBuilder.GetBiomeController((BiomeType)biome);
                if (ctrl == null || ctrl.Rooms == null) return null;
                for (int i = 0; i < ctrl.Rooms.Count; i++)
                {
                    var r = ctrl.Rooms[i];
                    if (r != null && r.BiomeControllerIndex == roomIndex) return r;
                }
            }
            catch { }
            return null;
        }

        // 与 RoomSaveController.InitializeBreakablePropsList / InitializeRoomSaveData 的过滤顺序一致。
        private static int SaveBreakableIndex(BaseRoom room, PropSpawnController target)
        {
            if (room == null || room.SpawnControllerManager == null) return -1;
            var all = room.SpawnControllerManager.PropSpawnControllers;
            if (all == null) return -1;
            int idx = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var p = all[i];
                if (p == null || p.PropInstance == null || p.CameraLayer != CameraLayer.Game || !p.IsBreakable) continue;
                if (ReferenceEquals(p, target)) return idx;
                idx++;
            }
            return -1;
        }

        private static int SaveDecoIndex(BaseRoom room, PropSpawnController target, int decoIdx)
        {
            if (room == null || room.SpawnControllerManager == null) return -1;
            var all = room.SpawnControllerManager.PropSpawnControllers;
            if (all == null) return -1;
            int n = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var p = all[i];
                if (p == null || p.CameraLayer != CameraLayer.Game || p.BreakableDecoInstances == null) continue;
                var decos = p.BreakableDecoInstances;
                for (int k = 0; k < decos.Length; k++)
                {
                    if (ReferenceEquals(p, target) && k == decoIdx) return n;
                    n++;
                }
            }
            return -1;
        }

        internal static void Reset()
        {
            _applyingRemote = false;
        }

        private static void Warn(string msg)
        {
            if (Time.unscaledTime - _lastWarn < 3f) return;
            _lastWarn = Time.unscaledTime;
            Plugin.Log.LogWarning("[RL2MP] [WSTATE] " + msg);
        }
    }
}
