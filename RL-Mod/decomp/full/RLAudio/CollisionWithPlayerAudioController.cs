using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class CollisionWithPlayerAudioController : MonoBehaviour, IAudioEventEmitter, IWeaponOnEnterHitResponse, IHitResponse
{
	[SerializeField]
	[EventRef]
	private string m_collidedWithPlayerAudioPath = string.Empty;

	private string m_defaultPath = string.Empty;

	private string m_description = string.Empty;

	public string Description
	{
		get
		{
			if (m_description == string.Empty)
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		m_defaultPath = m_collidedWithPlayerAudioPath;
	}

	public void WeaponOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (otherHBController.RootGameObject.CompareTag("Player") && !PlayerManager.GetPlayerController().IsInvincible && m_collidedWithPlayerAudioPath != string.Empty)
		{
			AudioManager.PlayOneShotAttached(this, m_collidedWithPlayerAudioPath, base.gameObject);
		}
	}

	public void SetCollisionWithPlayerAudioOverride(string path)
	{
		if (path != string.Empty)
		{
			m_collidedWithPlayerAudioPath = path;
		}
		else
		{
			m_collidedWithPlayerAudioPath = m_defaultPath;
		}
	}
}
