using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using LiteNetLib.Utils;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 角色外观同步（v1.0.0）：让分身长得像**对方**，而不是像自己。
    ///
    /// 旧实现的问题：整树克隆的模板永远从**本地玩家**复制，
    /// 所以客机看主机是"自己的样子"，主机看客机是"自己的高周目装备"（材质对不上还会发白）。
    ///
    /// 解决：把对方的 CharacterData（[Serializable]，含职业/装备/性别/肤色/发型/五官/特质）
    /// 发过来，在克隆体上用游戏自己的 LookCreator 重建外观——
    ///   LookCreator.InitializeClassLook      换职业套装
    ///   LookCreator.InitializeCharacterLook  五官/肤色/发型/体型
    ///   LookCreator.InitializeHelmetLook / InitializeArmorLook / InitializeCapeLook / InitializeWeaponLook  装备外观
    /// 这些方法只操作 SkinnedMeshRenderer 的 mesh/材质，不依赖 PlayerController 逻辑，
    /// 所以在"剥了脚本的分身"上调用是安全的。
    /// </summary>
    internal static class CharacterSync
    {
        private static readonly Dictionary<int, CharacterData> _remoteChars = new Dictionary<int, CharacterData>();

        private static CharacterData _lastSent;
        private static string _lastSentSignature;
        private static float _hostResendTimer;

        internal static bool HasData(int playerId)
        {
            return _remoteChars.ContainsKey(playerId);
        }

        // ---------------- 发送 ----------------

        /// <summary>序列化本地角色外观并广播（主机/客机都会调）。</summary>
        internal static void SendLocalCharacter()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected || nm.LocalPlayerId < 0) return;

            try
            {
                var data = SaveManager.PlayerSaveData.CurrentCharacter;
                if (data == null) return;

                var bf = new BinaryFormatter();
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, data);
                    bytes = ms.ToArray();
                }

                var w = new NetDataWriter();
                w.Put((byte)MsgType.CharacterData);
                w.Put(nm.LocalPlayerId);
                w.PutBytesWithLength(bytes);
                nm.SendToAllReliable(w, false);

                _lastSent = data;
                _lastSentSignature = Signature(data);
                Plugin.Log.LogInfo("[RL2MP] [CHAR] 已广播本地角色外观: "
                    + data.ClassType + " / " + data.Name + " / " + bytes.Length + " 字节");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [CHAR] 发送角色外观失败: " + ex.Message);
            }
        }

        /// <summary>本地角色换了（换继承人/换装备）就自动补发。</summary>
        internal static void Tick()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected || nm.LocalPlayerId < 0) return;

            _hostResendTimer += Time.unscaledDeltaTime;
            if (_hostResendTimer < 3f) return;
            _hostResendTimer = 0f;

            try
            {
                var data = SaveManager.PlayerSaveData.CurrentCharacter;
                if (data == null) return;
                string sig = Signature(data);
                if (sig != _lastSentSignature)
                {
                    Plugin.Log.LogInfo("[RL2MP] [CHAR] 检测到本地角色变化 -> 重新广播外观。");
                    SendLocalCharacter();
                }
            }
            catch { }
        }

        private static string Signature(CharacterData d)
        {
            return ((int)d.ClassType) + "|" + (int)d.Weapon + "|" + (int)d.EdgeEquipmentType + "|"
                + (int)d.ChestEquipmentType + "|" + (int)d.HeadEquipmentType + "|" + (int)d.CapeEquipmentType
                + "|" + d.EyeType + "|" + d.MouthType + "|" + d.HairType + "|" + d.HairColorType
                + "|" + d.SkinColorType + "|" + d.FacialHairType + "|" + d.BodyType + "|" + d.IsFemale
                + "|" + (int)d.TraitOne + "|" + (int)d.TraitTwo;
        }

        // ---------------- 接收 ----------------

        internal static void OnReceive(int playerId, byte[] bytes)
        {
            try
            {
                var bf = new BinaryFormatter();
                CharacterData data;
                using (var ms = new MemoryStream(bytes))
                {
                    data = (CharacterData)bf.Deserialize(ms);
                }
                if (data == null) return;

                var nm = NetworkManager.Instance;
                if (nm != null && nm.LocalPlayerId == playerId)
                {
                    return; // 自己的回声
                }

                bool isNew = !_remoteChars.ContainsKey(playerId);
                _remoteChars[playerId] = data;

                Plugin.Log.LogInfo("[RL2MP] [CHAR] 收到玩家 " + playerId + " 的角色外观: "
                    + data.ClassType + " / " + data.Name
                    + " 装备(头/胸/披/武)=" + data.HeadEquipmentType + "/" + data.ChestEquipmentType
                    + "/" + data.CapeEquipmentType + "/" + data.EdgeEquipmentType
                    + (isNew ? " (首次)" : " (更新)"));

                // 已经存在的分身立刻换装
                if (PlayerSync.Instance != null)
                {
                    PlayerSync.Instance.RefreshRemoteAppearance(playerId);
                }

                // 主机：转发给其他客机（多客机场景）
                if (nm != null && nm.IsHost)
                {
                    var w = new NetDataWriter();
                    w.Put((byte)MsgType.CharacterData);
                    w.Put(playerId);
                    w.PutBytesWithLength(bytes);
                    nm.ForwardToOtherClients(playerId, w);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [CHAR] 解析角色外观失败: " + ex.Message);
            }
        }

        // ---------------- 应用到分身 ----------------

        /// <summary>
        /// 在分身上用对方的 CharacterData 重建外观。
        /// 注意：只保留 LookController 组件，但先 enabled=false 阻止它自己的 OnEnable/Start
        /// （PlayerLookController.Start 会等 m_playerController，分身没有那个脚本，会 NRE）。
        /// </summary>
        internal static bool ApplyToClone(int playerId, GameObject cloneRoot)
        {
            if (cloneRoot == null) return false;

            CharacterData data;
            if (!_remoteChars.TryGetValue(playerId, out data) || data == null)
            {
                return false; // 还没收到对方外观，保持默认克隆外观
            }

            try
            {
                var look = cloneRoot.GetComponentInChildren<LookController>(true);
                if (look == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [CHAR] 克隆体上没有 LookController，无法重建外观。");
                    return false;
                }

                look.enabled = false; // 阻止注册事件 / 执行 Start 协程

                LookCreator.InitializeClassLook(data.ClassType, look);
                LookCreator.InitializeCharacterLook(data, look, false);
                LookCreator.InitializeHelmetLook(data.HeadEquipmentType, look);
                LookCreator.InitializeArmorLook(data.ChestEquipmentType, look);

                bool cantAttack = data.TraitOne == TraitType.CantAttack || data.TraitTwo == TraitType.CantAttack;
                LookCreator.InitializeWeaponLook(data.EdgeEquipmentType, look, data.Weapon, cantAttack);
                LookCreator.InitializeCapeLook(data.CapeEquipmentType, look);
                // 注意：不能再调 DisableAllWeaponGeo —— InitializeWeaponLook 内部已经
                // 先关全部再启用对应武器，这里再关一次会把刚启用的武器藏掉（看不到对方武器）。

                Plugin.Log.LogInfo("[RL2MP] [CHAR] 分身已重建外观: " + data.ClassType
                    + " (玩家 " + playerId + ")");
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [CHAR] 重建分身外观失败: " + ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }

        internal static void Clear(string reason)
        {
            if (_remoteChars.Count > 0)
            {
                Plugin.Log.LogInfo("[RL2MP] [CHAR] 清空远程角色外观缓存（" + reason + "）。");
            }
            _remoteChars.Clear();
            _lastSentSignature = null;
        }

        internal static void Remove(int playerId)
        {
            _remoteChars.Remove(playerId);
        }
    }
}
