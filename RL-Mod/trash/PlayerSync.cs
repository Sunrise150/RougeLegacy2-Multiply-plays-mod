using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL2Multiplayer;

public struct PlayerStateData
{
    public float PositionX;
    public float PositionY;
    public float VelocityX;
    public float VelocityY;
    public bool FacingRight;
    public bool Grounded;
}

public class PlayerSync : MonoBehaviour
{
    public static PlayerSync Instance { get; private set; }

    public MonoBehaviour LocalPlayer { get; private set; }
    public GameObject CachedLocalPlayerObject { get; private set; }

    public readonly Dictionary<int, GameObject> RemotePlayers = new();
    public readonly Dictionary<int, RemotePlayerInterpolator> RemoteInterpolators = new();

    private Type _playerControllerType;
    private SpriteRenderer _localSpriteRenderer;
    private Transform _localVisualRoot;

    private const float LerpSpeed = 20f;

    private void Awake()
    {
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnPlayerJoin += OnRemotePlayerJoin;
            NetworkManager.Instance.OnPlayerLeave += OnRemotePlayerLeave;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnPlayerJoin -= OnRemotePlayerJoin;
            NetworkManager.Instance.OnPlayerLeave -= OnRemotePlayerLeave;
        }
        ClearAllRemote();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Plugin.Log.LogInfo("[RL2MP] Scene loaded: " + scene.name + " - clearing remote players.");
        ClearAllRemote();
        LocalPlayer = null;
        CachedLocalPlayerObject = null;
        _localSpriteRenderer = null;
        _localVisualRoot = null;
    }

    private void ClearAllRemote()
    {
        foreach (var go in RemotePlayers.Values)
        {
            if (go != null) Destroy(go);
        }
        RemotePlayers.Clear();
        RemoteInterpolators.Clear();
    }

    private void Update()
    {
        EnsureLocalPlayerCaptured();
    }

    private void EnsureLocalPlayerCaptured()
    {
        if (CachedLocalPlayerObject != null && CachedLocalPlayerObject.activeInHierarchy)
        {
            if (LocalPlayer == null || LocalPlayer.gameObject != CachedLocalPlayerObject)
            {
                LocalPlayer = CachedLocalPlayerObject.GetComponent(_playerControllerType) as MonoBehaviour;
            }
            return;
        }

        if (_playerControllerType == null)
        {
            _playerControllerType = AccessTools.TypeByName("PlayerController");
            if (_playerControllerType == null)
            {
                _playerControllerType = AccessTools.TypeByName("Player");
            }
        }

        if (_playerControllerType == null) return;

        var found = UnityEngine.Object.FindObjectsOfType(_playerControllerType);
        if (found != null && found.Length > 0)
        {
            var mb = found[0] as MonoBehaviour;
            if (mb != null)
            {
                LocalPlayer = mb;
                CachedLocalPlayerObject = mb.gameObject;
                Plugin.Log.LogInfo("[RL2MP] Captured local player: " + mb.gameObject.name);
            }
        }
    }

    public PlayerStateData GetLocalPlayerState()
    {
        var state = new PlayerStateData();

        if (LocalPlayer == null) return state;

        state.PositionX = LocalPlayer.transform.position.x;
        state.PositionY = LocalPlayer.transform.position.y;

        try
        {
            var t = LocalPlayer.GetType();
            state.VelocityX = GetVec2Prop(t, LocalPlayer, "Velocity").x;
            state.VelocityY = GetVec2Prop(t, LocalPlayer, "Velocity").y;
            state.FacingRight = GetBoolProp(t, LocalPlayer, "IsFacingRight");
            state.Grounded = GetBoolProp(t, LocalPlayer, "IsGrounded");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning("[RL2MP] GetLocalPlayerState prop read: " + ex.Message);
        }

        return state;
    }

    private static Vector2 GetVec2Prop(Type t, object obj, string name)
    {
        var cur = t;
        while (cur != null)
        {
            var p = AccessTools.Property(cur, name);
            if (p != null)
            {
                object v = p.GetValue(obj);
                if (v is Vector2 vec) return vec;
                return Vector2.zero;
            }
            cur = cur.BaseType;
        }
        return Vector2.zero;
    }

    private static bool GetBoolProp(Type t, object obj, string name)
    {
        var cur = t;
        while (cur != null)
        {
            var p = AccessTools.Property(cur, name);
            if (p != null)
            {
                object v = p.GetValue(obj);
                if (v is bool b) return b;
                return false;
            }
            cur = cur.BaseType;
        }
        return false;
    }

    public void HandleRemotePlayerState(int id, float x, float y, float vx, float vy, bool facingRight, bool grounded)
    {
        if (!RemotePlayers.TryGetValue(id, out var go) || go == null)
        {
            Plugin.Log.LogInfo("[RL2MP] Creating remote player visual for ID=" + id);
            go = CreateRemotePlayerObject(id);
            RemotePlayers[id] = go;
        }

        var interpolator = go.GetComponent<RemotePlayerInterpolator>();
        if (interpolator == null) interpolator = go.AddComponent<RemotePlayerInterpolator>();

        interpolator.SetTarget(new Vector3(x, y, go.transform.position.z), facingRight);
    }

    private GameObject CreateRemotePlayerObject(int id)
    {
        GameObject go = new GameObject("RemotePlayer_" + id);
        go.transform.position = new Vector3(0, 0, 0);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        sr.color = new Color(1f, 1f, 1f, 0.85f);

        Sprite sprite = CreateRemoteSprite();
        if (sprite != null)
        {
            sr.sprite = sprite;
        }
        else
        {
            Plugin.Log.LogWarning("[RL2MP] No sprite available for remote player " + id);
        }

        return go;
    }

    private Sprite CreateRemoteSprite()
    {
        if (CachedLocalPlayerObject != null)
        {
            try
            {
                var srs = CachedLocalPlayerObject.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var sr in srs)
                {
                    if (sr.sprite != null && sr.gameObject.activeInHierarchy)
                    {
                        return sr.sprite;
                    }
                }
            }
            catch { }
        }

        return CreateFallbackSprite();
    }

    private Sprite CreateFallbackSprite()
    {
        var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                tex.SetPixel(i, j, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
    }

    private void OnRemotePlayerJoin(int id)
    {
        Plugin.Log.LogInfo("[RL2MP] Remote player joined: ID=" + id);
        if (RemotePlayers.ContainsKey(id) && RemotePlayers[id] != null) return;
        HandleRemotePlayerState(id, 0, 0, 0, 0, true, false);
    }

    private void OnRemotePlayerLeave(int id)
    {
        Plugin.Log.LogInfo("[RL2MP] Remote player left: ID=" + id);
        if (RemotePlayers.TryGetValue(id, out var go) && go != null)
        {
            Destroy(go);
        }
        RemotePlayers.Remove(id);
        RemoteInterpolators.Remove(id);
    }
}

public class RemotePlayerInterpolator : MonoBehaviour
{
    private Vector3 _targetPos;
    private bool _targetFacingRight;
    private float _visualFacingScaleX;

    private const float LerpSpeed = 18f;

    private void Start()
    {
        _targetPos = transform.position;
        _targetFacingRight = true;
        _visualFacingScaleX = transform.localScale.x;
    }

    public void SetTarget(Vector3 pos, bool facingRight)
    {
        _targetPos = pos;
        _targetFacingRight = facingRight;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * LerpSpeed);

        var scale = transform.localScale;
        float absScale = Mathf.Abs(scale.x);
        float targetX = _targetFacingRight ? absScale : -absScale;
        scale.x = Mathf.Lerp(scale.x, targetX, Time.deltaTime * LerpSpeed * 2f);
        transform.localScale = scale;
    }
}
