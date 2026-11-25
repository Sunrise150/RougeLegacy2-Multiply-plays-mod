using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v2.0.2 协作模式：技能树加点同步。
    ///
    /// 用户新约定（取代 v2.0.1 的"收益各自存档"）：
    ///   通关后大家一起回起点、一起加技能树，客机的技能树等元进度也要跟着改变。
    ///
    /// 与 BossProgressSync 的区别：
    ///   - Boss 门 = 主机权威、只改内存不落盘（收益各自）。
    ///   - 技能树 = 双向并集(max)：任一方加点，双方都获得该等级；等级写进各自存档（元进度共享）。
    ///
    /// 实现：
    ///   1) patch SkillTreeSlot.PurchaseSkillUpgrade postfix：本地真实加点后广播增量 (senderId, type, newLevel)。
    ///      所有包带发送者 id；客机发给主机，主机应用后 ForwardToOtherClients 转给其他客机。
    ///   2) 接收端取 max(local, remote)，仅当远端更高才 SetSkillObjLevel(level, additive:false)；
    ///      应用期间 _applyingRemote=true 抑制 postfix 再次广播（防回声）。
    ///   3) 主机每 3s 广播全量等级表（约 90 项），兜底中途加入 / 漏包。
    ///   4) 只在协作 + Connected 生效；竞速/单人完全不碰。不还原（元进度永久共享，断线保留）。
    /// </summary>
    internal static class SkillTreeSync
    {
        private const byte MsgSkillTree = 43;   // 与 NetworkManager.MsgType.SkillTree 保持同步

        internal static bool Active
        {
            get { return Score.Style == PlayStyle.CoOp
                          && NetworkManager.Instance != null && NetworkManager.Instance.Connected; }
        }

        private static bool _applyingRemote;
        private static MethodInfo _purchaseMethod;
        private static float _hostTimer;
        private static float _lastLog;

        /// <summary>读取本地技能树全部等级（type=int, level=short）。</summary>
        private static List<KeyValuePair<int, short>> ReadAll()
        {
            var list = new List<KeyValuePair<int, short>>();
            try
            {
                foreach (SkillTreeType t in SkillTreeType_RL.TypeArray)
                {
                    if (t == SkillTreeType.None) continue;
                    int lv = SkillTreeManager.GetSkillObjLevel(t);
                    if (lv > 0) list.Add(new KeyValuePair<int, short>((int)t, (short)lv));
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[SKILLSYNC] ReadAll 异常: " + Plugin.PatchError(ex));
            }
            return list;
        }

        /// <summary>玩家真实加点后：广播这一项（带 senderId + type + 新等级）。</summary>
        private static void PurchasePostfix(SkillTreeSlot __instance)
        {
            if (_applyingRemote) return;
            if (!Active || NetworkManager.Instance == null) return;
            try
            {
                SkillTreeType t = __instance.SkillTreeType;
                if (t == SkillTreeType.None) return;
                int lv = SkillTreeManager.GetSkillObjLevel(t);

                var w = new LiteNetLib.Utils.NetDataWriter();
                w.Put(MsgSkillTree);
                w.Put(NetworkManager.Instance.LocalPlayerId);   // senderId
                w.Put((byte)0);                                  // 模式 0 = 增量单项
                w.Put((int)t);
                w.Put((short)lv);
                NetworkManager.Instance.SendToAllReliable(w, false);

                if (Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[SKILLSYNC] 本地加点广播 " + t + " -> Lv" + lv);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[SKILLSYNC] PurchasePostfix 异常: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>主机周期性广播全量等级表：让中途加入 / 漏包的客机也能补齐。</summary>
        internal static void HostTick()
        {
            if (!Active || NetworkManager.Instance == null || !NetworkManager.Instance.IsHost) return;
            _hostTimer += Time.unscaledDeltaTime;
            if (_hostTimer < 3f) return;
            _hostTimer = 0f;
            try
            {
                var list = ReadAll();
                var w = new LiteNetLib.Utils.NetDataWriter();
                w.Put(MsgSkillTree);
                w.Put(NetworkManager.Instance.LocalPlayerId);   // senderId = 主机
                w.Put((byte)1);                                  // 模式 1 = 全量
                w.Put((short)list.Count);
                foreach (var kv in list) { w.Put(kv.Key); w.Put(kv.Value); }
                NetworkManager.Instance.SendToAllReliable(w, false);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[SKILLSYNC] HostTick 异常: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>收到同步数据（增量或全量）：逐项取 max 应用；主机负责中转给其他客机。</summary>
        internal static void OnReceive(LiteNetLib.Utils.NetDataReader reader)
        {
            if (!Active || NetworkManager.Instance == null) return;
            int senderId = reader.GetInt();
            byte mode = reader.GetByte();

            // 一次读完 reader 到 pending（reader 不可回退，必须先读完再中转+应用）
            var pending = new List<KeyValuePair<int, short>>();
            try
            {
                if (mode == 0)
                {
                    pending.Add(new KeyValuePair<int, short>(reader.GetInt(), reader.GetShort()));
                }
                else if (mode == 1)
                {
                    short count = reader.GetShort();
                    if (count < 0 || count > 500) return;   // 防御畸形包
                    for (int i = 0; i < count; i++)
                        pending.Add(new KeyValuePair<int, short>(reader.GetInt(), reader.GetShort()));
                }
                else return;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[SKILLSYNC] 读取包异常: " + ex.Message);
                return;
            }

            // 主机收到客机包：用 pending 构造中转包，转给其他客机（他们也要取 max 对齐）
            if (NetworkManager.Instance.IsHost && senderId != NetworkManager.Instance.LocalPlayerId)
            {
                var fw = new LiteNetLib.Utils.NetDataWriter();
                fw.Put(MsgSkillTree);
                fw.Put(senderId);
                fw.Put(mode);
                if (mode == 0)
                {
                    fw.Put(pending[0].Key); fw.Put(pending[0].Value);
                }
                else
                {
                    fw.Put((short)pending.Count);
                    foreach (var kv in pending) { fw.Put(kv.Key); fw.Put(kv.Value); }
                }
                NetworkManager.Instance.ForwardToOtherClients(senderId, fw);
            }

            // 本地应用（取 max）
            try
            {
                _applyingRemote = true;
                int applied = 0;
                try
                {
                    foreach (var kv in pending)
                    {
                        SkillTreeType t = (SkillTreeType)kv.Key;
                        if (t == SkillTreeType.None) continue;
                        int cur = SkillTreeManager.GetSkillObjLevel(t);
                        if (kv.Value > cur && SkillTreeManager.SetSkillObjLevel(t, kv.Value, additive: false))
                            applied++;
                    }
                }
                finally { _applyingRemote = false; }

                if (applied > 0 && Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[SKILLSYNC] 对齐技能树 " + applied + " 项（取 max，写各自存档）");
                }
            }
            catch (Exception ex)
            {
                _applyingRemote = false;
                Plugin.Log.LogWarning("[SKILLSYNC] OnReceive 异常: " + Plugin.PatchError(ex));
            }
        }

        internal static void Apply(Harmony harmony)
        {
            try
            {
                _purchaseMethod = AccessTools.Method(typeof(SkillTreeSlot), "PurchaseSkillUpgrade",
                    new Type[] { typeof(int) });
                if (_purchaseMethod == null)
                {
                    Plugin.Log.LogWarning("[SKILLSYNC] SkillTreeSlot.PurchaseSkillUpgrade 未找到，技能树同步不可用。");
                    return;
                }
                var post = new HarmonyMethod(typeof(SkillTreeSync).GetMethod("PurchasePostfix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(_purchaseMethod, postfix: post);
                Plugin.Log.LogInfo("[SKILLSYNC] 补丁 OK: SkillTreeSlot.PurchaseSkillUpgrade postfix（协作技能树共享）。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[SKILLSYNC] 补丁失败: " + Plugin.PatchError(ex));
            }
        }

        internal static void Reset()
        {
            _applyingRemote = false;
            _hostTimer = 0f;
        }
    }
}
