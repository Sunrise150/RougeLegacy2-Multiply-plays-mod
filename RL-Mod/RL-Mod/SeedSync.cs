using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace RL2Multiplayer
{
    /// <summary>
    /// 种子同步（v0.5.0 核心功能）：让所有玩家的地图生成一致。
    ///
    /// 游戏机制（反编译 RNGSeedManager 确认）：
    /// - GenerateNewSeed(sceneName) 是每次进新场景时 roll seed 的地方；
    ///   只有"激活场景是 World 且 IsOverrideSet"时才用 MasterSeedOverride，
    ///   否则用 Environment.TickCount 随机 —— 所以客户端光设 override 不保险。
    /// - RNGManager.InitializeRandomNumberGenerators(seed) 用 master seed 初始化所有 RNG。
    ///
    /// 实现分两侧：
    /// - HOST：GenerateNewSeed postfix —— 游戏 roll 完新 World seed 后立刻读出来广播。
    ///   开新局（全员死亡重开）时 seed 会变，这个 hook 保证每次变化都推给客户端。
    /// - CLIENT：收到 seed 后 (1) SetMasterSeedOverride + 强制重初始化 RNG；
    ///   (2) GenerateNewSeed postfix 强制把 seed 表里 World 的条目换成 host 的 seed，
    ///   绕过"激活场景"条件，保证下一张生成的地图与 host 一致。
    /// </summary>
    internal static class SeedSync
    {
        private static int _clientSeedOverride = -1;

        internal static bool ClientHasOverride { get { return _clientSeedOverride > 0; } }

        internal static void Apply(Harmony harmony)
        {
            try
            {
                var m = AccessTools.Method(typeof(RNGSeedManager), "GenerateNewSeed");
                if (m == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [SEED] RNGSeedManager.GenerateNewSeed not found!");
                    return;
                }

                var postfix = new HarmonyMethod(typeof(SeedSync).GetMethod("GenerateNewSeedPostfix",
                    BindingFlags.Static | BindingFlags.NonPublic));

                harmony.Patch(m, postfix: postfix);
                Plugin.Log.LogInfo("[RL2MP] [SEED] Patched RNGSeedManager.GenerateNewSeed postfix OK.");

                // 客机参数级强制：游戏有两条种子初始化路径——
                // 主菜单开新局用"存档里的 MasterSeed"，局内重建用"种子表 GetCurrentSeed"。
                // 光改种子表管不住主菜单路径，所以在 RNG 初始化入口直接改写参数。
                var initRng = AccessTools.Method(typeof(RNGManager), "InitializeRandomNumberGenerators", new[] { typeof(int) });
                if (initRng == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [SEED] RNGManager.InitializeRandomNumberGenerators(int) not found!");
                    return;
                }
                var rngPrefix = new HarmonyMethod(typeof(SeedSync).GetMethod("InitializeRngPrefix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(initRng, prefix: rngPrefix);
                Plugin.Log.LogInfo("[RL2MP] [SEED] Patched RNGManager.InitializeRandomNumberGenerators(int) prefix OK.");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [SEED] SeedSync patch failed: " + ex.Message);
            }
        }

        /// <summary>
        /// GenerateNewSeed 的 postfix。参数名 sceneName 与原方法一致，Harmony 按名绑定。
        /// </summary>
        private static void GenerateNewSeedPostfix(string sceneName)
        {
            try
            {
                var nm = NetworkManager.Instance;

                // HOST：游戏刚 roll 完新 seed。注意广播"真正驱动生成"的那一个：
                // 反编译 LevelManager_RL 确认——主菜单开新局走存档里的 MasterSeed
                // （此时种子表被填的是无用的 TickCount），所以优先读 MasterSeedOverride。
                if (nm != null && nm.IsHost)
                {
                    if (sceneName == "World")
                    {
                        int seed = RNGSeedManager.IsMasterSeedOverrideSet
                            ? RNGSeedManager.MasterSeedOverride
                            : RNGSeedManager.GetCurrentSeed("World");
                        if (seed > 0)
                        {
                            nm.OnWorldSeedGenerated(seed);
                        }
                    }
                    return;
                }

                // CLIENT：把 World 的 seed 强制替换成 host 的，绕过游戏里的激活场景条件
                if (nm != null && nm.Connected && !nm.IsHost && ClientHasOverride && sceneName == "World")
                {
                    var table = Traverse.Create(typeof(RNGSeedManager))
                        .Field("m_sceneToCurrentSeedTable")
                        .GetValue<Dictionary<string, int>>();
                    if (table != null)
                    {
                        table[sceneName] = _clientSeedOverride;
                        Plugin.Log.LogInfo("[RL2MP] [SEED] CLIENT: forced World seed -> "
                            + _clientSeedOverride + " (0x" + _clientSeedOverride.ToString("X") + ")");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [SEED] GenerateNewSeedPostfix failed: " + ex.Message);
            }
        }

        /// <summary>
        /// 客机参数级强制：RNG 初始化时把种子改写为主机的。
        /// 覆盖两条路径：主菜单开新局（存档 MasterSeed）和局内重建（种子表）。
        /// </summary>
        private static void InitializeRngPrefix(ref int masterSeed)
        {
            try
            {
                var nm = NetworkManager.Instance;
                if (nm == null || nm.IsHost || !nm.Connected) return;
                if (!ClientHasOverride) return;
                if (masterSeed == _clientSeedOverride) return;

                Plugin.Log.LogInfo("[RL2MP] [SEED] CLIENT: RNG init seed " + masterSeed
                    + " (0x" + masterSeed.ToString("X") + ") -> forced to " + _clientSeedOverride
                    + " (0x" + _clientSeedOverride.ToString("X") + ")");
                masterSeed = _clientSeedOverride;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [SEED] InitializeRngPrefix failed: " + ex.Message);
            }
        }

        /// <summary>
        /// 客机收到 host 的 seed：记下 override 并立即套用。
        /// 返回 true 表示客机需要重进世界才能和主机地图对齐。
        /// </summary>
        internal static bool OnClientReceivedSeed(int seed)
        {
            if (seed <= 0) return false;

            bool hadOverride = (_clientSeedOverride > 0);
            bool changed = (seed != _clientSeedOverride);

            // 关键：不能只看"种子变了没有"。客机很可能是**先自己开了一局再连进来**的，
            // 那时 _clientSeedOverride 还是 -1（首次收到），按旧逻辑不算重开 → 不重进，
            // 于是客机留着自己那套地图，两边地图永远不一样。
            // 所以要直接比对"客机当前世界实际在用的种子"。
            int currentWorldSeed = ReadCurrentEffectiveSeed();
            bool worldMismatch = (currentWorldSeed > 0 && currentWorldSeed != seed);

            bool needReEnter = (hadOverride && changed) || worldMismatch;

            _clientSeedOverride = seed;

            Plugin.Log.LogInfo("[RL2MP] [SEED] CLIENT received seed=" + seed + " (0x" + seed.ToString("X")
                + ")" + (changed ? " (NEW -> applying)" : " (unchanged)")
                + " | 客机当前世界种子=" + currentWorldSeed
                + (needReEnter ? " [> 不一致，需要重进世界对齐地图]" : ""));

            if (changed)
            {
                ApplySeedAsClient(seed);
            }
            return needReEnter;
        }

        /// <summary>读客机当前世界实际生效的种子。-1 = 读不到 / 不在 World 场景。</summary>
        private static int ReadCurrentEffectiveSeed()
        {
            try
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "World") return -1;
                if (RNGSeedManager.IsMasterSeedOverrideSet) return RNGSeedManager.MasterSeedOverride;
                return RNGSeedManager.GetCurrentSeed("World");
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 客户端套用 host 的 seed：走游戏自带的 SetMasterSeedOverride（保持
        /// IsMasterSeedOverrideSet 等内部状态一致），再强制重初始化 RNG。
        /// 注意：SetMasterSeedOverride 会把 seed 写进玩家存档（游戏自带机制，
        /// 与"带种子开局"行为一致）；客人存档隔离是后续待办。
        /// </summary>
        private static void ApplySeedAsClient(int seed)
        {
            try
            {
                RNGSeedManager.SetMasterSeedOverride(seed);
                Plugin.Log.LogInfo("[RL2MP] [SEED] SetMasterSeedOverride(" + seed + ") OK.");

                var initAll = AccessTools.Method(typeof(RNGManager), "InitializeRandomNumberGenerators", new[] { typeof(int) });
                if (initAll != null)
                {
                    initAll.Invoke(null, new object[] { seed });
                    Plugin.Log.LogInfo("[RL2MP] [SEED] RNGManager.InitializeRandomNumberGenerators(" + seed + ") OK.");
                }
                else
                {
                    Plugin.Log.LogWarning("[RL2MP] [SEED] InitializeRandomNumberGenerators(int) not found.");
                }

                var printSeeds = AccessTools.Method(typeof(RNGManager), "PrintSeedsToConsole");
                if (printSeeds != null)
                {
                    printSeeds.Invoke(null, null);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("[RL2MP] [SEED] ApplySeedAsClient FAILED: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
