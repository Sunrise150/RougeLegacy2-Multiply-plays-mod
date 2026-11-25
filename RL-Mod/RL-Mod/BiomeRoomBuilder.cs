using System;
using System.Collections;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v3.0.22：观战端在本地补建"权威端所在区域"的房间。
    ///
    /// 背景（真机日志实锤）：跨区域（biome 切换）在原生里走
    /// BiomeTransitionController.Run(door)：
    ///     yield return WorldBuilder.InstantiateBiome(destinationBiome);  // 新区域房间
    ///     DestroyBiomeRoomInstances(prevBiome);                          // 旧区域房间销毁
    /// 这一整套是【本地玩家实际过那扇门】的那一端才执行的。主机倒地观战时，
    /// 主机从没过那扇门 → 主机本地永远不会实例化新区域的房间。
    /// 后果：客机日志 "目标(96,-180)未匹配：场景内有效房间=46 个"，
    /// 选房永远落空 → 观战视角卡在初始地图，怎么走都过不去。
    ///
    /// 修法：权威端换房广播里带上目标房间所属 biome（RoomTravelSync.RemoteBiome），
    /// 观战端发现本地找不到目标房间时，用原生 WorldBuilder.InstantiateBiome
    /// 在本地把那个区域的房间补建出来，再由既有的选房逻辑接管。
    ///
    /// 注意：这里只【补建】不【销毁】旧区域 —— 观战端本地的旧区域房间是观战者
    /// 自己待的地方，销毁它会让本地玩家（尸体）所在的房间凭空消失。
    /// 跨区域后两端房间集合会短暂不一致，但对观战来说这是安全方向。
    /// </summary>
    internal static class BiomeRoomBuilder
    {
        /// <summary>正在补建的 biome（-1 = 没在补建）。</summary>
        private static int _building = -1;
        /// <summary>开始补建的时刻 —— 用于超时看门狗（协程被 Unity 强停时 finally 不保证跑）。</summary>
        private static float _buildingSince;
        /// <summary>补建最长允许时长（秒），超过就认为卡死并释放占位。</summary>
        private const float BuildTimeout = 30f;
        /// <summary>最近一次补建的结果日志时间。</summary>
        private static float _lastLog;
        /// <summary>已经尝试过补建的 biome，避免每帧重复触发。</summary>
        private static readonly System.Collections.Generic.HashSet<int> _attempted =
            new System.Collections.Generic.HashSet<int>();
        /// <summary>补建【失败】过的 biome —— 允许重试（原生协程可能因加载时序失败）。</summary>
        private static readonly System.Collections.Generic.HashSet<int> _failed =
            new System.Collections.Generic.HashSet<int>();
        /// <summary>各 biome 上次补建失败的时刻（用于重试冷却）。</summary>
        private static readonly System.Collections.Generic.Dictionary<int, float> _lastFailAt =
            new System.Collections.Generic.Dictionary<int, float>();

        /// <summary>
        /// 确保本端有某个 biome 的房间。若本地还没有该 biome 的房间对象，
        /// 启动原生协程补建。返回 true 表示"本地已经/即将有"该区域的房间。
        /// </summary>
        internal static bool EnsureBiome(int biomeValue)
        {
            if (biomeValue < 0) return false;
            if (!WorldBuilder.IsInstantiated) return false;

            BiomeType biome;
            try { biome = (BiomeType)biomeValue; }
            catch { return false; }
            // v3.0.23：原生 BiomeTransitionController.BiomeTransitionCoroutine 在调用
            // GetBiomeController / InstantiateBiome 之前【先】GetGroupedBiomeType。
            // 子 biome（CaveMiddle=32/CaveBottom=34、ForestTop=52/ForestBottom=54、
            // TowerExterior=145）在 WorldBuilder.BiomeControllers 里没有自己的 key，
            // 直接拿原值查会返回 null → 补建静默失败，观战永远卡在旧区。
            BiomeType grouped = BiomeType_RL.GetGroupedBiomeType(biome);
            biomeValue = (int)grouped;
            biome = grouped;
            if (biome == BiomeType.None || biome == BiomeType.Any) return false;

            try
            {
                var ctrl = WorldBuilder.GetBiomeController(biome);
                if (ctrl == null) return false;

                // 本地已经有这批房间对象就算数。
                // 注意：不能用 ctrl.IsInstantiated —— 它对 Castle 有特殊判断
                // （Rooms[0].RoomType == Transition），已建好的 Castle 反而返回 false。
                if (ctrl.Rooms != null && ctrl.Rooms.Count > 0) return true;
            }
            catch (Exception ex)
            {
                Warn("查询 biome 状态失败: " + ex.Message);
                return false;
            }

            // 还没实例化 —— 启动原生协程补建。
            if (_building == biomeValue)
            {
                // 看门狗：Unity 强制停协程（宿主销毁/切场景）时 finally 不保证执行，
                // 这里按超时释放占位，否则这个 biome 永远补不上。
                if (Time.unscaledTime - _buildingSince < BuildTimeout) return true;  // 正在建
                Warn("补建超时（" + BuildTimeout + "s），释放占位并重试");
                _building = -1;
                _failed.Add(biomeValue);
                _lastFailAt[biomeValue] = Time.unscaledTime;
            }
            // 已经成功发起过（协程在跑或已完成）就不再重复发起，避免每帧刷协程。
            if (_attempted.Contains(biomeValue) && !_failed.Contains(biomeValue)) return false;
            // 上次失败过 —— 冷却 5 秒后再试（原生协程可能因场景还在加载而失败）。
            if (_failed.Contains(biomeValue))
            {
                float last = -999f;
                _lastFailAt.TryGetValue(biomeValue, out last);
                if (Time.unscaledTime - last < 5f) return false;
            }

            var host = Host();
            if (host == null) return false;

            _attempted.Add(biomeValue);
            _failed.Remove(biomeValue);
            _building = biomeValue;
            _buildingSince = Time.unscaledTime;
            try
            {
                host.StartCoroutine(BuildRoutine(biome));
            }
            catch (Exception ex)
            {
                _building = -1;
                Warn("启动 biome 补建协程失败: " + ex.Message);
                return false;
            }
            return true;
        }

        private static IEnumerator BuildRoutine(BiomeType biome)
        {
            // 注意：C# 迭代器里【带 catch 的 try 块中不允许 yield return】（CS1626）。
            // 所以这里外层只用 try/finally（finally 允许 yield），
            // 异常捕获交给 Drive() —— 它手动驱动原生协程，
            // 因此异常会在我们自己的 try/catch 里被抓住，不会逃到 Unity 的协程调度器。
            try
            {
                Plugin.Log.LogInfo("[RL2MP] [BIOME] 观战端本地补建区域房间: " + biome);

                var probe = new BuildProbe();
                yield return Drive(biome, probe);

                if (probe.Error != null)
                {
                    // 记为失败：允许后续重试（原生协程可能因为场景还在加载而失败）。
                    MarkFailed(biome, "原生补建协程异常: " + probe.Error.Message);
                    yield break;
                }

                // 原生 BiomeTransitionCoroutine 还会做这两步：
                //   Messenger.Broadcast(GameEvent.UpdatePools)
                //   UpdateObjectPools(originBiome, destBiome)
                // 我们补建时同样需要刷新对象池，否则新区域的 Prop/Enemy 借不出来
                // （PropManager.GetProp 找不到就返回 null，设施/怪全空）。
                try
                {
                    SharedWorldObjectPoolManager.CreateBiomePools(biome);
                }
                catch (Exception ex)
                {
                    Warn("补建对象池失败: " + ex.Message);
                }

                // 补建后房间集合变了，如果一个房间都没建出来也算失败（允许重试）。
                int roomCount = 0;
                try
                {
                    var ctrl = WorldBuilder.GetBiomeController(biome);
                    if (ctrl != null && ctrl.Rooms != null) roomCount = ctrl.Rooms.Count;
                }
                catch { }
                if (roomCount <= 0)
                {
                    MarkFailed(biome, "补建后该区域房间数为 0");
                    yield break;
                }

                // 房间缓存要失效 —— 新建的房间对象不在旧缓存里，
                // 否则 FindRoomByCoords 仍然报"未匹配"。
                RoomTravelSync.InvalidateRoomCache();

                // 观战选房要重新解析 —— 之前选中的房间还在旧区域，
                // 权威端已经跨区了，必须重新按权威坐标选一次。
                DownedState.ClearSpectateRoomSelection();
                DownedState.ForceSpectateRoomRefresh = true;

                Plugin.Log.LogInfo("[RL2MP] [BIOME] 区域房间补建完成: " + biome
                    + "，房间数 " + roomCount);
            }
            finally
            {
                _building = -1;
            }
        }

        /// <summary>手动驱动协程时的异常载体。</summary>
        private sealed class BuildProbe
        {
            public Exception Error;
        }

        /// <summary>
        /// 手动驱动原生协程（含嵌套的子 IEnumerator）。
        ///
        /// 为什么不用 <c>yield return WorldBuilder.InstantiateBiome(biome);</c>：
        /// 迭代器中带 catch 的 try 块里不允许 yield return（CS1626），
        /// 而这里又必须能捕获原生协程抛出的异常（否则失败后无法重试、
        /// 且 finally 可能不执行导致 _building 永久卡死）。
        ///
        /// 原生 InstantiateBiome 内部会 yield return 多个 BuildStage 子协程，
        /// 这里递归驱动它们，异常在每一层都被我们自己的 try/catch 捕获，
        /// 同时把 WaitForFixedUpdate / 普通 yield 指令原样交回 Unity 驱动。
        /// </summary>
        private static IEnumerator Drive(BiomeType biome, BuildProbe probe)
        {
            // 注意：连"创建"这一步也放进 try —— InstantiateBiome 内部会访问
            // BiomeControllers[biome]，biome 不存在时直接抛 KeyNotFoundException。
            IEnumerator inner = null;
            try
            {
                inner = WorldBuilder.InstantiateBiome(biome);
            }
            catch (Exception ex)
            {
                probe.Error = ex;
            }

            if (inner == null)
            {
                if (probe.Error == null) probe.Error = new Exception("原生补建协程为 null");
                yield break;
            }

            yield return DriveOne(inner, probe);
        }

        /// <summary>驱动单个 IEnumerator（可递归到嵌套子协程）。</summary>
        private static IEnumerator DriveOne(IEnumerator inner, BuildProbe probe)
        {
            // 显式栈扁平驱动：原生协程里 `yield return 某个 IEnumerator` 的语义是
            // "下一帧才展开该子协程"，所以这里每帧只推进一步，与原生帧序完全一致。
            // 用栈而不是递归，是为了不额外套一层 Unity 协程。
            var stack = new System.Collections.Generic.Stack<IEnumerator>();
            stack.Push(inner);

            while (stack.Count > 0)
            {
                if (probe.Error != null) yield break;

                var cur = stack.Peek();
                bool hasNext = false;
                object current = null;
                Exception err = null;
                try
                {
                    hasNext = cur.MoveNext();
                    if (hasNext) current = cur.Current;
                }
                catch (Exception ex)
                {
                    err = ex;
                }

                if (err != null)
                {
                    probe.Error = err;
                    yield break;
                }

                if (!hasNext)
                {
                    // 当前协程结束 —— 回到上一层。
                    // 注意不插入帧延迟：Unity 原生处理 `yield return 某个 IEnumerator`
                    // 时是同帧压栈推进的，这里保持完全一致（多余的等待只会拖慢补建）。
                    stack.Pop();
                    continue;
                }

                var nested = current as IEnumerator;
                if (nested != null)
                {
                    // 子协程（各 BuildStage）压栈，下一帧才推进它。
                    stack.Push(nested);
                }
                else
                {
                    // WaitForFixedUpdate 等普通 yield 指令，交回 Unity 驱动。
                    yield return current;
                }
            }
        }

        /// <summary>记录补建失败，允许冷却后重试。</summary>
        private static void MarkFailed(BiomeType biome, string reason)
        {
            _failed.Add((int)biome);
            _lastFailAt[(int)biome] = Time.unscaledTime;
            Warn("补建区域房间失败: " + reason);
        }

        /// <summary>观战端补建完后要能立刻用新房间 —— 让选房逻辑重新解析一次。</summary>
        internal static void ResetState()
        {
            _building = -1;
            _buildingSince = 0f;
            _attempted.Clear();
            _failed.Clear();
            _lastFailAt.Clear();
        }

        private static MonoBehaviour Host()
        {
            var nm = NetworkManager.Instance;
            if (nm != null) return nm;
            return DownedState.HostBehaviour;
        }

        private static void Warn(string msg)
        {
            if (Time.unscaledTime - _lastLog > 3f)
            {
                _lastLog = Time.unscaledTime;
                Plugin.Log.LogWarning("[RL2MP] [BIOME] " + msg);
            }
        }
    }
}
