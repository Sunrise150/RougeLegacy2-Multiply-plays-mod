using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class ItemDropEventEmitter : MonoBehaviour, IAudioEventEmitter, ITerrainOnEnterHitResponse, IHitResponse
{
	[EventRef]
	public string PickupEvent;

	public ParamRef[] PickupParams = new ParamRef[0];

	[EventRef]
	public string BounceEvent;

	public ParamRef[] BounceParams = new ParamRef[0];

	[SerializeField]
	private CorgiController_RL m_corgi;

	protected EventInstance m_pickupEventInstance;

	protected EventInstance m_bounceEventInstance;

	public string Description => ToString();

	private void Start()
	{
		if (!string.IsNullOrEmpty(PickupEvent))
		{
			m_pickupEventInstance = RuntimeManager.CreateInstance(PickupEvent);
		}
		if (!string.IsNullOrEmpty(BounceEvent))
		{
			m_bounceEventInstance = RuntimeManager.CreateInstance(BounceEvent);
		}
		m_corgi.OnCorgiLandedEnterRelay.AddListener(OnBounce);
	}

	private void OnBounce(CorgiController_RL corgiController)
	{
		AudioManager.PlayAttached(this, m_bounceEventInstance, base.gameObject);
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		AudioManager.PlayAttached(this, m_pickupEventInstance, base.gameObject);
	}
}
