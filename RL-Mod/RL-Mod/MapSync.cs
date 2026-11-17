using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LiteNetLib;
using LiteNetLib.Utils;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL2Multiplayer
{
    /// <summary>
    /// 地图同步（v1.0.0）：让客机看到的关卡布局与主机完全一致。
    ///
    /// 为什么单靠种子同步不够（反编译 LevelManager_RL + SaveManager 确认）：
    /// - RL2 的地图**不是**每次用种子重新生成的，而是存在存档里（StageSaveData）。
    /// - 进 World 时 LevelManager_RL 会调 SaveManager.LoadStageSaveData(...)，
    ///   优先读存档里已生成的房间数据（RoomSaveDataDict）；只有"新建世界"分支
    ///   才会用 RNGManager 从 MasterSeed 派生 BiomeCreationSeed 等重新生成。
    /// - 所以客机只要存档里已经有自己的地图（几乎必然有），种子对上也不会换地图。
    ///
    /// 解决办法：主机把 StageSaveData 用 BinaryFormatter 序列化后分块发给客机，
    /// 客机在内存里整体替换 SaveManager.StageSaveData（public static 字段），
    /// 再重进 World —— 加载的就是主机的房间数据，地图 100% 一致。
    ///
    /// 存档保护：客机的磁盘存档不能被主机地图污染。CreateSaveFile 落盘那一刻，
    /// 如果正在序列化的是 StageSaveData，就临时换回客机自己的原始对象，
    /// 序列化完再换回主机的。会话结束时彻底恢复。
    /// </summary>
    internal static class MapSync
    {
        private const int ChunkSize = 16 * 1024; // 单包数据块大小，避免 UDP 碎片过大

        internal static bool Applied;            // 客机是否已套用主机地图
        internal static bool PendingWorldReload; // 客机需要重进世界才能生效

        /// <summary>沙盒模式用：地图还没抓到手时先挂起推送，别把玩家自己的存档地图发出去。</summary>

        private static StageSaveData _original;  // 客机自己的原始 StageSaveData（存档保护 + 恢复）

        // 接收缓冲
        private static byte[] _buffer;
        private static bool[] _got;
        private static int _totalChunks;
        private static int _totalBytes;
        private static int _received;

        private static float _hostResendCooldown;
        private static readonly HashSet<int> _sentTo = new HashSet<int>();

        // ---------------- HOST ----------------

        /// <summary>主机定期检查：有新客机连上且自己已经进了世界，就把地图推过去（每个客机只推一次）。</summary>
        internal static void HostTick()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost || !nm.Connected) return;
            if (!nm.HasClients) return;

            _hostResendCooldown -= Time.unscaledDeltaTime;
            if (_hostResendCooldown > 0f) return;
            _hostResendCooldown = 2f;

            // 只发给还没收到过地图的客机（4 人局里中途有人加入时，不能重推给已经在跑的人）
            var targets = new List<int>();
            foreach (int cid in nm.ClientIds)
            {
                if (!_sentTo.Contains(cid)) targets.Add(cid);
            }
            if (targets.Count == 0) return;

            // 世界没建完就发会发到半成品数据，等构建完成
            try
            {
                if (SceneManager.GetActiveScene().name != "World") return;
                if (WorldBuilder.State != BiomeBuildStateID.Complete) return;
            }
            catch
            {
                return;
            }

            if (HostSendMap(targets))
            {
                foreach (int cid in targets) _sentTo.Add(cid);
            }
        }

        internal static void ClearSentTracking()
        {
            _sentTo.Clear();
        }

        /// <summary>把地图推给指定客机（targets 为空 = 所有客机）。返回是否发送成功。</summary>
        internal static bool HostSendMap(List<int> targets)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost) return false;

            byte[] bytes;
            try
            {
                var stage = SaveManager.StageSaveData;
                if (stage == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [MAP] SaveManager.StageSaveData is null, skip.");
                    return false;
                }

                var bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, stage);
                    bytes = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [MAP] 序列化 StageSaveData 失败: " + ex.Message);
                return false;
            }

            if (bytes == null || bytes.Length == 0) return false;

            if (targets == null || targets.Count == 0)
            {
                targets = new List<int>();
                foreach (int cid in nm.ClientIds) targets.Add(cid);
            }

            int totalChunks = (bytes.Length + ChunkSize - 1) / ChunkSize;
            Plugin.Log.LogInfo("[RL2MP] [MAP] HOST 推送地图数据: " + bytes.Length
                + " 字节 / " + totalChunks + " 块 -> " + targets.Count + " 个客机"
                + "（" + string.Join(",", targets) + "）。");

            for (int i = 0; i < totalChunks; i++)
            {
                int off = i * ChunkSize;
                int len = Math.Min(ChunkSize, bytes.Length - off);
                byte[] chunk = new byte[len];
                Array.Copy(bytes, off, chunk, 0, len);

                var w = new NetDataWriter();
                w.Put((byte)MsgType.MapData);
                w.Put(totalChunks);
                w.Put(i);
                w.Put(bytes.Length);
                w.PutBytesWithLength(chunk);
                for (int t = 0; t < targets.Count; t++) nm.SendToPlayer(targets[t], w);
            }

            return true;
        }

        // ---------------- CLIENT ----------------

        internal static void OnChunk(int totalChunks, int index, int totalBytes, byte[] chunk)
        {
            if (totalChunks <= 0 || totalBytes <= 0 || chunk == null) return;

            if (_buffer == null || _totalChunks != totalChunks || _totalBytes != totalBytes)
            {
                _buffer = new byte[totalBytes];
                _got = new bool[totalChunks];
                _totalChunks = totalChunks;
                _totalBytes = totalBytes;
                _received = 0;
                Plugin.Log.LogInfo("[RL2MP] [MAP] CLIENT 开始接收地图: " + totalBytes
                    + " 字节 / " + totalChunks + " 块。");
            }

            if (index < 0 || index >= totalChunks || _got[index]) return;

            int off = index * ChunkSize;
            if (off + chunk.Length > _buffer.Length)
            {
                Plugin.Log.LogWarning("[RL2MP] [MAP] 块 " + index + " 越界，丢弃整次接收。");
                ResetBuffer();
                return;
            }

            Array.Copy(chunk, 0, _buffer, off, chunk.Length);
            _got[index] = true;
            _received++;

            if (_received >= totalChunks)
            {
                ApplyMap();
            }
        }

        private static void ApplyMap()
        {
            byte[] data = _buffer;
            ResetBuffer();

            try
            {
                StageSaveData stage;
                var bf = new BinaryFormatter();
                using (var ms = new MemoryStream(data))
                {
                    stage = (StageSaveData)bf.Deserialize(ms);
                }
                if (stage == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [MAP] 反序列化结果为 null，放弃。");
                    return;
                }

                if (!Applied || _original == null)
                {
                    _original = SaveManager.StageSaveData;
                    Plugin.Log.LogInfo("[RL2MP] [MAP] 已备份客机原始 StageSaveData（存档保护用）。");
                }

                SaveManager.StageSaveData = stage;
                _hostStage = stage;
                _hostBiomeSeed = stage.BiomeCreationSeed;
                Applied = true;
                PendingWorldReload = true;

                int biomes = 0, rooms = 0;
                try
                {
                    foreach (var kv in stage.RoomSaveDataDict)
                    {
                        biomes++;
                        if (kv.Value != null) rooms += kv.Value.Count;
                    }
                }
                catch { }

                Plugin.Log.LogInfo("[RL2MP] [MAP] CLIENT 已套用主机地图: " + data.Length
                    + " 字节, " + biomes + " 个区域 / " + rooms + " 个房间数据。需要重进世界生效。");

                // 在 World 里就自动重进一次，让新地图立刻生效
                if (SceneManager.GetActiveScene().name == "World")
                {
                    ReloadWorldSoon();
                }
                else
                {
                    // 客机还在主菜单/城里：直接进图（进图时 LoadStageSaveData 会被拦成主机地图）
                    Race.AutoEnterWorld("收到主机地图");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [MAP] 应用地图失败: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private static void ResetBuffer()
        {
            _buffer = null;
            _got = null;
            _totalChunks = 0;
            _totalBytes = 0;
            _received = 0;
        }

        private static float _reloadAt = -1f;

        private static void ReloadWorldSoon()
        {
            if (_reloadAt > 0f) return;
            _reloadAt = Time.realtimeSinceStartup + 2f;
            Plugin.Log.LogInfo("[RL2MP] [MAP] 2 秒后重进 World 让主机地图生效...");
        }

        /// <summary>客机每帧：处理延迟重进世界。</summary>
        internal static void ClientTick()
        {
            if (_reloadAt <= 0f) return;
            if (Time.realtimeSinceStartup < _reloadAt) return;
            _reloadAt = -1f;

            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected || nm.IsHost) return;

            try
            {
                if (SceneManager.GetActiveScene().name != "World")
                {
                    Plugin.Log.LogInfo("[RL2MP] [MAP] 已不在 World，跳过重进（进图时会直接用主机地图）。");
                    return;
                }
                if (SceneLoader_RL.IsLoading)
                {
                    _reloadAt = Time.realtimeSinceStartup + 2f; // 正在转场，等一会
                    return;
                }

                Plugin.Log.LogInfo("[RL2MP] [MAP] 重进 World（用主机地图数据）...");
                SceneLoader_RL.LoadScene(SceneID.World, TransitionID.FadeToBlackWithLoading);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [MAP] 重进世界失败: " + ex.Message);
            }
        }

        /// <summary>客机进完世界后调用：标记已生效。</summary>
        internal static void OnWorldLoaded()
        {
            if (!Applied) return;
            PendingWorldReload = false;

            // 验证：进完世界后 StageSaveData 里的地图种子是否还是主机那份。
            // 如果变了，说明游戏走了"新建世界"分支、把我们传的地图丢掉了。
            try
            {
                if (_hostBiomeSeed > 0 && SaveManager.StageSaveData != null)
                {
                    int cur = SaveManager.StageSaveData.BiomeCreationSeed;
                    if (cur != _hostBiomeSeed)
                    {
                        Plugin.Log.LogWarning("[RL2MP] [MAP] ⚠ 地图验证失败：主机 BiomeCreationSeed="
                            + _hostBiomeSeed + "，当前=" + cur
                            + " —— 游戏重建了世界，地图可能仍不一致。");
                    }
                    else
                    {
                        Plugin.Log.LogInfo("[RL2MP] [MAP] ✓ 地图验证通过：BiomeCreationSeed=" + cur
                            + " 与主机一致。");
                    }
                }
                else
                {
                    Plugin.Log.LogInfo("[RL2MP] [MAP] 客机已进入使用主机地图的世界。");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [MAP] 验证地图种子失败: " + ex.Message);
            }
        }

        private static int _hostBiomeSeed;

        // ---------------- 存档保护 ----------------

        internal static void RestoreForSave()
        {
            if (!Applied || _original == null) return;
            SaveManager.StageSaveData = _original;
        }

        internal static void ReapplyAfterSave()
        {
            if (!Applied || _hostStage == null) return;
            SaveManager.StageSaveData = _hostStage;
        }

        private static StageSaveData _hostStage; // 主机地图（应用时保存引用，供写盘后还原）

        // ---------------- 补丁：拦截磁盘读档 ----------------

        /// <summary>
        /// 关键补丁：LevelManager_RL 进 World 时会调
        ///   SaveManager.LoadGameData(profile, SaveDataType.Stage, out data)
        ///   → WorldBuilder.CreateWorld(data)
        /// 直接从磁盘读客机自己的地图，把我们内存里替换的主机地图冲掉。
        /// 这里在它读完的瞬间把输出换成主机地图，世界就是用主机数据建的。
        /// </summary>
        internal static void ApplyPatches(HarmonyLib.Harmony harmony)
        {
            try
            {
                var m = HarmonyLib.AccessTools.Method(typeof(SaveManager), "LoadGameData",
                    new[] { typeof(int), typeof(SaveDataType), typeof(object).MakeByRefType() });
                if (m == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [MAP] SaveManager.LoadGameData not found; "
                        + "地图同步可能被磁盘读档覆盖。");
                    return;
                }

                var postfix = new HarmonyLib.HarmonyMethod(typeof(MapSync).GetMethod("LoadGameDataPostfix",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic));
                harmony.Patch(m, postfix: postfix);
                Plugin.Log.LogInfo("[RL2MP] [MAP] Patched SaveManager.LoadGameData (地图读档拦截 OK).");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [MAP] LoadGameData patch failed: " + ex.Message);
            }
        }

        private static void LoadGameDataPostfix(SaveDataType saveType, ref object loadDataOutput)
        {
            try
            {
                if (saveType != SaveDataType.Stage) return;
                if (!Applied || _hostStage == null) return;

                loadDataOutput = _hostStage;
                Plugin.Log.LogInfo("[RL2MP] [MAP] 拦截磁盘读档 -> 换成主机地图数据（BiomeCreationSeed="
                    + _hostStage.BiomeCreationSeed + "）。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [MAP] LoadGameDataPostfix failed: " + ex.Message);
            }
        }

        // ---------------- 会话结束恢复 ----------------

        internal static void Restore(string reason)
        {
            _sentTo.Clear();

            if (!Applied)
            {
                ResetBuffer();
                return;
            }

            try
            {
                if (_original != null)
                {
                    SaveManager.StageSaveData = _original;
                    Plugin.Log.LogInfo("[RL2MP] [MAP] 已恢复客机自己的地图数据（" + reason + "）。");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [MAP] 恢复失败: " + ex.Message);
            }

            Applied = false;
            PendingWorldReload = false;
            _original = null;
            _hostStage = null;
            _hostBiomeSeed = 0;
            _reloadAt = -1f;
            _sentTo.Clear();
            ResetBuffer();
        }
    }
}
