using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class PlayerDamageAudioController : DamageAudioController
{
	[SerializeField]
	[EventRef]
	private string m_playerHitAudioPath;

	[SerializeField]
	[EventRef]
	private string m_bardHitAudioPath;

	[SerializeField]
	[EventRef]
	private string m_playerDeathAudioPath;

	[SerializeField]
	[EventRef]
	private string m_bardDeathAudioPath;

	[SerializeField]
	[EventRef]
	private string m_killingBlowAudioPath;

	[SerializeField]
	[EventRef]
	private string m_armourHitAudioPath;

	[SerializeField]
	[EventRef]
	private string m_armourBreakAudioPath;

	private IHealth m_playerHealth;

	private EventInstance m_hitEventInstance;

	private EventInstance m_deathEventInstance;

	private EventInstance m_killingBlowEventInstance;

	private EventInstance m_armourHitEventInstance;

	private EventInstance m_armourBreakEventInstance;

	private EventInstance m_bardHitEventInstance;

	private EventInstance m_bardDeathEventInstance;

	private int m_previousArmourValue = -1;

	protected override void Awake()
	{
		m_hitEventInstance = AudioUtility.GetEventInstance(m_playerHitAudioPath, base.transform);
		m_bardHitEventInstance = AudioUtility.GetEventInstance(m_bardHitAudioPath, base.transform);
		m_deathEventInstance = AudioUtility.GetEventInstance(m_playerDeathAudioPath, base.transform);
		m_bardDeathEventInstance = AudioUtility.GetEventInstance(m_bardDeathAudioPath, base.transform);
		m_killingBlowEventInstance = AudioUtility.GetEventInstance(m_killingBlowAudioPath, base.transform);
		m_armourHitEventInstance = AudioUtility.GetEventInstance(m_armourHitAudioPath, base.transform);
		m_armourBreakEventInstance = AudioUtility.GetEventInstance(m_armourBreakAudioPath, base.transform);
		m_playerHealth = GetComponent<IHealth>();
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_hitEventInstance.isValid())
		{
			m_hitEventInstance.release();
		}
		if (m_bardHitEventInstance.isValid())
		{
			m_bardHitEventInstance.release();
		}
		if (m_deathEventInstance.isValid())
		{
			m_deathEventInstance.release();
		}
		if (m_bardDeathEventInstance.isValid())
		{
			m_bardDeathEventInstance.release();
		}
		if (m_killingBlowEventInstance.isValid())
		{
			m_killingBlowEventInstance.release();
		}
		if (m_armourHitEventInstance.isValid())
		{
			m_armourHitEventInstance.release();
		}
		if (m_armourBreakEventInstance.isValid())
		{
			m_armourBreakEventInstance.release();
		}
	}

	private void SetAudioEventsGenderAndSizeParameters()
	{
		float playerGenderAudioParameterValue = AudioUtility.GetPlayerGenderAudioParameterValue();
		float playerSizeAudioParameterValue = AudioUtility.GetPlayerSizeAudioParameterValue();
		GetCurrentClassEventInstances(out var hitEvent, out var deathEvent);
		if (hitEvent.isValid())
		{
			hitEvent.setParameterByName("gender", playerGenderAudioParameterValue);
			hitEvent.setParameterByName("Player_Size", playerSizeAudioParameterValue);
		}
		if (deathEvent.isValid())
		{
			deathEvent.setParameterByName("gender", playerGenderAudioParameterValue);
			deathEvent.setParameterByName("Player_Size", playerSizeAudioParameterValue);
		}
	}

	private void GetCurrentClassEventInstances(out EventInstance hitEvent, out EventInstance deathEvent)
	{
		if (PlayerManager.GetPlayerController().CharacterClass.ClassType == ClassType.LuteClass)
		{
			hitEvent = m_bardHitEventInstance;
			deathEvent = m_bardDeathEventInstance;
		}
		else
		{
			hitEvent = m_hitEventInstance;
			deathEvent = m_deathEventInstance;
		}
	}

	protected override void OnTakeDamage(GameObject attacker, float damageTaken, bool isCrit)
	{
		SetAudioEventsGenderAndSizeParameters();
		base.OnTakeDamage(attacker, damageTaken, isCrit);
		EventInstance damageAudio = GetDamageAudio();
		if (damageAudio.isValid())
		{
			AudioManager.PlayAttached(this, damageAudio, base.gameObject);
		}
	}

	private EventInstance GetDamageAudio()
	{
		bool num = m_playerHealth.CurrentHealth <= 0f;
		int currentArmor = PlayerManager.GetPlayerController().CurrentArmor;
		bool flag = currentArmor > 0;
		GetCurrentClassEventInstances(out var hitEvent, out var _);
		if (num)
		{
			hitEvent = m_killingBlowEventInstance;
		}
		else if (flag)
		{
			hitEvent = m_armourHitEventInstance;
		}
		else if (m_previousArmourValue > 0)
		{
			hitEvent = m_armourBreakEventInstance;
		}
		m_previousArmourValue = currentArmor;
		return hitEvent;
	}

	protected override void OnDeath(GameObject otherObj)
	{
		base.OnDeath(otherObj);
		GetCurrentClassEventInstances(out var _, out var deathEvent);
		if (deathEvent.isValid())
		{
			AudioManager.PlayAttached(this, deathEvent, base.gameObject);
		}
	}
}
