using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer;

[BepInPlugin("com.rl2.mp", "Rogue Legacy 2 Multiplayer", "0.2.0")]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log;
    internal static Plugin Instance;

    public static string HostAddress = "127.0.0.1";

    private Harmony _harmony;

    private void Awake()
    {
        Log = Logger;
        Instance = this;

        Logger.LogInfo("[RL2MP] Plugin loaded.");

        _harmony = new Harmony("com.rl2.mp");
        _harmony.PatchAll();

        gameObject.name = "RL2MP_Root";
        DontDestroyOnLoad(gameObject);

        gameObject.AddComponent<NetworkManager>();
        gameObject.AddComponent<PlayerSync>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            Logger.LogInfo("[RL2MP] F8 pressed - starting host.");
            NetworkManager.Instance.StartHost();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            Logger.LogInfo("[RL2MP] F9 pressed - starting client.");
            NetworkManager.Instance.StartClient();
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            Logger.LogInfo("[RL2MP] F10 pressed - stopping network.");
            NetworkManager.Instance.Stop();
        }
    }

    private void OnDestroy()
    {
        NetworkManager.Instance?.Stop();
        _harmony?.UnpatchSelf();
    }
}
