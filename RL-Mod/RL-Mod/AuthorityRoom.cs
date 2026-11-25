using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 权威"当前房间坐标"的唯一来源。
    ///
    /// 背景：EnemyAliveSync / ObstacleSync 原来各自用
    /// PlayerManager.GetCurrentPlayerRoom()(=本地玩家 CurrentlyInRoom) 当房间坐标。
    /// 本地倒地观战时，尸体被 SetActive(false) 且停在死亡房间，CurrentlyInRoom
    /// 不再更新 —— 于是存活方已经换到新房间，倒地方的权威坐标却永远锁在死亡房间。
    /// 后果：ALIVE 广播的 room 不变、障碍物的 BuildKey 房间段对不上（客机收不到），
    /// 观战房间定位也滞后。这里在倒地观战时改用换房包带来的对端权威坐标。
    ///
    /// 非观战路径完全等价于旧的 GetCurrentPlayerRoom()，不改变单机/正常联机行为。
    /// </summary>
    internal static class AuthorityRoom
    {
        private static float _lastLog;

        /// <summary>权威房间坐标（网格坐标，整数）。取不到返回 Vector2.zero。</summary>
        internal static Vector2 GetCoords()
        {
            // 倒地观战：尸体所在房间已无意义，用对端换房包里的权威房间坐标。
            if (DownedState.LocalDown && RoomTravelSync.HasRemoteRoom)
            {
                Vector2 spectate = RoomTravelSync.RemoteRoomCoords;
                if (Time.unscaledTime - _lastLog > 5f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[AUTHROOM] 倒地观战权威坐标=(" + spectate.x + "," + spectate.y + ")");
                }
                return spectate;
            }

            try
            {
                var br = PlayerManager.GetCurrentPlayerRoom();
                if (br == null) return Vector2.zero;
                var r = br as Room;
                return r != null ? r.Coordinates : (Vector2)br.transform.position;
            }
            catch { return Vector2.zero; }
        }
    }
}
