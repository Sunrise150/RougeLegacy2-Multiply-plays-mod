using Sigtrap.Relays;
using UnityEngine;

public class CaveLanternPostProcessingController : MonoBehaviour
{
	private MobilePostProcessingProfile m_profile;

	private static Relay m_onAwakeRelay = new Relay();

	public static IRelayLink OnAwakeRelay => m_onAwakeRelay;

	public static CaveLanternPostProcessingController Instance { get; private set; }

	private float DarknessAmountWhenFullyLit
	{
		get
		{
			float num = Mathf.Min((float)SaveManager.PlayerSaveData.GetBurden(BurdenType.EnemyProjectiles).CurrentLevel * 0.05f, 0.25f);
			return 0.51f / (1f + num);
		}
	}

	private float SoftnessWhenFullyLit
	{
		get
		{
			float num = Mathf.Min((float)SaveManager.PlayerSaveData.GetBurden(BurdenType.EnemyProjectiles).CurrentLevel * 0.05f, 0.25f);
			return 0.31f / (1f + num);
		}
	}

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
		m_profile = ScriptableObject.CreateInstance<MobilePostProcessingProfile>();
		m_profile.EnableCircularDarknessEffect = true;
		m_profile.OverrideCircDarknessColor = true;
		m_profile.OverrideCircDarknessAmount = true;
		m_profile.OverrideCircDarknessSoftness = true;
		m_onAwakeRelay.Dispatch();
		m_onAwakeRelay.RemoveAll();
	}

	private void OnEnable()
	{
		if (SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.CaveLantern) == 0)
		{
			m_profile.CircDarknessColor = Heirloom_EV.CAVE_LANTERN_DARKNESS_COLOR_DIM;
			m_profile.CircDarknessAmount = 0.83f;
			m_profile.CircDarknessSoftness = 0.13f;
		}
		else
		{
			m_profile.CircDarknessColor = Heirloom_EV.CAVE_LANTERN_DARKNESS_COLOR_LIT;
			m_profile.CircDarknessAmount = DarknessAmountWhenFullyLit;
			m_profile.CircDarknessSoftness = SoftnessWhenFullyLit;
		}
		CameraController.ForegroundPostProcessing.AddDimensionOverride(m_profile);
	}

	private void OnDisable()
	{
		if (CameraController.IsInstantiated && (bool)CameraController.ForegroundPostProcessing)
		{
			CameraController.ForegroundPostProcessing.RemoveDimensionOverride(m_profile);
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	public static void EnableCaveLanternEffect()
	{
		if (CameraController.IsInstantiated && (bool)CameraController.ForegroundPostProcessing)
		{
			CameraController.ForegroundPostProcessing.AddDimensionOverride(Instance.m_profile);
		}
	}

	public static void DisableCaveLanternEffect()
	{
		if (CameraController.IsInstantiated && (bool)CameraController.ForegroundPostProcessing)
		{
			CameraController.ForegroundPostProcessing.RemoveDimensionOverride(Instance.m_profile);
		}
	}

	public static void SetDimnessPercent(float percent)
	{
		if (SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.CaveLantern) == 0)
		{
			Instance.m_profile.CircDarknessColor = Heirloom_EV.CAVE_LANTERN_DARKNESS_COLOR_DIM * percent;
			Instance.m_profile.CircDarknessAmount = 0.83f * percent;
			Instance.m_profile.CircDarknessSoftness = 0.13f * percent;
		}
		else
		{
			Instance.m_profile.CircDarknessColor = Heirloom_EV.CAVE_LANTERN_DARKNESS_COLOR_LIT * percent;
			Instance.m_profile.CircDarknessAmount = Instance.DarknessAmountWhenFullyLit * percent;
			Instance.m_profile.CircDarknessSoftness = Instance.SoftnessWhenFullyLit * percent;
		}
		CameraController.ForegroundPostProcessing.ForceDirty();
	}
}
