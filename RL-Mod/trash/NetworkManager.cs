using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace RL2Multiplayer;

public enum MsgType : byte
{
    Welcome = 1,
    PlayerState = 2,
    LevelSeed = 3,
}

public class NetworkManager : MonoBehaviour, INetEventListener
{
    public static NetworkManager Instance { get; private set; }

    private const int Port = 9050;
    private const string Key = "RL2MP";
    private const float SendInterval = 0.05f;
    private const int HostId = 0;

    public bool IsHost { get; private set; }
    public bool Connected => _netManager != null && _netManager.IsRunning;
    public int LocalPlayerId { get; private set; } = -1;

    public event Action<int> OnPlayerJoin;
    public event Action<int> OnPlayerLeave;

    private NetManager _netManager;
    private NetPeer _serverPeer;
    private readonly Dictionary<int, NetPeer> _clients = new();
    private readonly HashSet<int> _knownPlayerIds = new();
    private float _sendTimer;

    private int _hostMasterSeed = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (_netManager == null || !_netManager.IsRunning) return;

        _netManager.PollEvents();

        if (IsHost && _hostMasterSeed == -1)
        {
            TryCaptureMasterSeed();
        }

        if (LocalPlayerId >= 0 && PlayerSync.Instance != null && PlayerSync.Instance.LocalPlayer != null)
        {
            _sendTimer += Time.unscaledDeltaTime;
            if (_sendTimer >= SendInterval)
            {
                _sendTimer = 0f;
                SendPlayerState();
            }
        }
    }

    public void StartHost()
    {
        if (Connected)
        {
            Plugin.Log.LogWarning("[RL2MP] Network already running.");
            return;
        }

        Plugin.Log.LogInfo("[RL2MP] Starting host on port " + Port);

        _netManager = new NetManager(this) { AutoRecycle = true };

        if (!_netManager.Start(Port))
        {
            Plugin.Log.LogError("[RL2MP] Failed to start host - port " + Port + " may be in use.");
            _netManager = null;
            return;
        }

        IsHost = true;
        LocalPlayerId = HostId;
        _knownPlayerIds.Clear();
        _knownPlayerIds.Add(HostId);

        Plugin.Log.LogInfo("[RL2MP] Host started. Local player ID = " + LocalPlayerId);
    }

    public void StartClient()
    {
        if (Connected)
        {
            Plugin.Log.LogWarning("[RL2MP] Network already running.");
            return;
        }

        Plugin.Log.LogInfo("[RL2MP] Starting client...");

        _netManager = new NetManager(this) { AutoRecycle = true };
        _netManager.Start();
        _netManager.Connect(Plugin.HostAddress, Port, Key);

        IsHost = false;
        _knownPlayerIds.Clear();

        Plugin.Log.LogInfo("[RL2MP] Client connecting to " + Plugin.HostAddress + ":" + Port);
    }

    public void Stop()
    {
        if (_netManager == null) return;

        foreach (int id in new List<int>(_knownPlayerIds))
        {
            if (id != LocalPlayerId) OnPlayerLeave?.Invoke(id);
        }
        _knownPlayerIds.Clear();

        _clients.Clear();
        _serverPeer = null;

        try { _netManager.Stop(); } catch { }
        _netManager = null;

        IsHost = false;
        LocalPlayerId = -1;

        Plugin.Log.LogInfo("[RL2MP] Network stopped.");
    }

    public void SendPlayerState()
    {
        if (PlayerSync.Instance == null || PlayerSync.Instance.LocalPlayer == null) return;

        var p = PlayerSync.Instance.GetLocalPlayerState();

        var w = new NetDataWriter();
        w.Put((byte)MsgType.PlayerState);
        w.Put(LocalPlayerId);
        w.Put(p.PositionX); w.Put(p.PositionY);
        w.Put(p.VelocityX); w.Put(p.VelocityY);
        w.Put(p.FacingRight);
        w.Put(p.Grounded);

        if (IsHost)
        {
            foreach (var c in _clients.Values)
            {
                c.Send(w, DeliveryMethod.Unreliable);
            }
        }
        else if (_serverPeer != null)
        {
            _serverPeer.Send(w, DeliveryMethod.Unreliable);
        }
    }

    private void TryCaptureMasterSeed()
    {
        try
        {
            var t = AccessTools.TypeByName("RNGSeedManager");
            if (t == null) return;

            var getCur = AccessTools.Method(t, "GetCurrentSeed", new[] { typeof(string) });
            if (getCur == null) return;

            int seed = (int)getCur.Invoke(null, new object[] { "World" });
            if (seed > 0)
            {
                _hostMasterSeed = seed;
                Plugin.Log.LogInfo("[RL2MP] Master Seed captured: " + seed + " (0x" + seed.ToString("X") + ")");
                BroadcastLevelSeed(seed);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning("[RL2MP] MasterSeed capture: " + ex.Message);
        }
    }

    private void BroadcastLevelSeed(int seed)
    {
        if (!IsHost || _clients.Count == 0) return;

        var w = new NetDataWriter();
        w.Put((byte)MsgType.LevelSeed);
        w.Put(seed);

        foreach (var c in _clients.Values)
        {
            c.Send(w, DeliveryMethod.ReliableOrdered);
        }

        Plugin.Log.LogInfo("[RL2MP] Broadcast LevelSeed=" + seed + " to " + _clients.Count + " clients.");
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        if (IsHost)
        {
            request.AcceptIfKey(Key);
        }
    }

    public void OnPeerConnected(NetPeer peer)
    {
        if (IsHost)
        {
            int assignedId = peer.Id;
            _clients[assignedId] = peer;

            Plugin.Log.LogInfo("[RL2MP] Client connected: peerId=" + assignedId);

            var w = new NetDataWriter();
            w.Put((byte)MsgType.Welcome);
            w.Put(assignedId);
            peer.Send(w, DeliveryMethod.ReliableOrdered);

            if (_hostMasterSeed > 0)
            {
                var w2 = new NetDataWriter();
                w2.Put((byte)MsgType.LevelSeed);
                w2.Put(_hostMasterSeed);
                peer.Send(w2, DeliveryMethod.ReliableOrdered);
            }

            if (_knownPlayerIds.Add(assignedId))
            {
                OnPlayerJoin?.Invoke(assignedId);
            }
        }
        else
        {
            _serverPeer = peer;
            Plugin.Log.LogInfo("[RL2MP] Connected to host.");
        }
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        if (IsHost)
        {
            int id = peer.Id;
            Plugin.Log.LogInfo("[RL2MP] Client disconnected: id=" + id + " reason=" + disconnectInfo.Reason);

            _clients.Remove(id);
            if (_knownPlayerIds.Remove(id))
            {
                OnPlayerLeave?.Invoke(id);
            }
        }
        else
        {
            Plugin.Log.LogInfo("[RL2MP] Disconnected from host. Reason: " + disconnectInfo.Reason);
            _serverPeer = null;
            Stop();
        }
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        try
        {
            byte type = reader.GetByte();

            if (type == (byte)MsgType.Welcome)
            {
                int assignedId = reader.GetInt();
                LocalPlayerId = assignedId;
                _knownPlayerIds.Add(assignedId);
                Plugin.Log.LogInfo("[RL2MP] Received Welcome. Local ID = " + LocalPlayerId);
            }
            else if (type == (byte)MsgType.LevelSeed)
            {
                int seed = reader.GetInt();
                Plugin.Log.LogInfo("[RL2MP] Received LevelSeed = " + seed + " (0x" + seed.ToString("X") + ")");
                ApplyMasterSeedOverride(seed);
            }
            else if (type == (byte)MsgType.PlayerState)
            {
                int id = reader.GetInt();
                float x = reader.GetFloat();
                float y = reader.GetFloat();
                float vx = reader.GetFloat();
                float vy = reader.GetFloat();
                bool facingRight = reader.GetBool();
                bool grounded = reader.GetBool();

                if (IsHost && id != HostId)
                {
                    var fw = new NetDataWriter();
                    fw.Put((byte)MsgType.PlayerState);
                    fw.Put(id);
                    fw.Put(x); fw.Put(y);
                    fw.Put(vx); fw.Put(vy);
                    fw.Put(facingRight);
                    fw.Put(grounded);

                    foreach (var c in _clients.Values)
                    {
                        if (c.Id != id && c.ConnectionState == ConnectionState.Connected)
                        {
                            c.Send(fw, DeliveryMethod.Unreliable);
                        }
                    }
                }

                if (id == LocalPlayerId) return;

                if (_knownPlayerIds.Add(id))
                {
                    OnPlayerJoin?.Invoke(id);
                }

                PlayerSync.Instance?.HandleRemotePlayerState(id, x, y, vx, vy, facingRight, grounded);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError("[RL2MP] OnReceive error: " + ex.Message);
        }
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        Plugin.Log.LogWarning("[RL2MP] Network error: endpoint=" + endPoint + " socket=" + socketError);
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    private static void ApplyMasterSeedOverride(int seed)
    {
        try
        {
            var seedMgr = AccessTools.TypeByName("RNGSeedManager");
            var rngMgr = AccessTools.TypeByName("RNGManager");
            if (seedMgr == null || rngMgr == null)
            {
                Plugin.Log.LogWarning("[RL2MP] RNGSeedManager / RNGManager not found.");
                return;
            }

            var setOverride = AccessTools.Method(seedMgr, "SetMasterSeedOverride", new[] { typeof(int) });
            if (setOverride != null)
            {
                setOverride.Invoke(null, new object[] { seed });
                Plugin.Log.LogInfo("[RL2MP] RNGSeedManager.MasterSeedOverride set.");
            }

            var initAll = AccessTools.Method(rngMgr, "InitializeRandomNumberGenerators", new[] { typeof(int) });
            if (initAll != null)
            {
                initAll.Invoke(null, new object[] { seed });
                Plugin.Log.LogInfo("[RL2MP] RNG controllers rebuilt with Master Seed.");
            }

            var printSeeds = AccessTools.Method(rngMgr, "PrintSeedsToConsole");
            if (printSeeds != null) printSeeds.Invoke(null, null);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError("[RL2MP] ApplyMasterSeedOverride error: " + ex.Message);
        }
    }
}
