using System;
using FMOD.Studio;
using UnityEngine;

namespace RLAudio;

public class ProjectileEventEmitter : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	protected bool m_onlyPlayCollisionWhenHitTerrain;

	[SerializeField]
	protected bool m_playSpawnAudioAutomatically = true;

	protected Projectile_RL m_projectile;

	protected string m_deathEvent;

	protected string m_lifetimeEvent;

	protected string m_hitSurfaceEvent;

	protected EventInstance m_lifeTimeEventInstance;

	protected string m_description;

	protected string m_trimmedName = string.Empty;

	protected string m_spawnEvent;

	protected GameObject m_lastGameObjectHit;

	private Action<Projectile_RL, GameObject> m_onCollision;

	private Action<Projectile_RL, GameObject> m_onDeath;

	private bool m_playSpawnAudio = true;

	private bool m_playLifetimeAudio = true;

	private bool m_playDeathAudio = true;

	protected const string WALL_SPELL_PROJECTILE_NAME_STARTS_WITH = "DamageWall";

	protected const string PROJECTILE_DEATH_DUE_TO_WALL_SPELL_AUDIO_PATH = "event:/SFX/Spells/sfx_spell_blueWall_hit";

	protected const string STATIC_WALL_PROJECTILE_NAME_STARTS_WITH = "StaticWall";

	protected const string PROJECTILE_DEATH_DUE_TO_STATIC_WALL_AUDIO_PATH = "event:/SFX/Spells/sfx_spell_shieldWall_damageAbsorb";

	public string DeathEvent
	{
		get
		{
			return m_deathEvent;
		}
		protected set
		{
			m_deathEvent = value;
		}
	}

	public string Description
	{
		get
		{
			if (string.IsNullOrEmpty(m_description))
			{
				m_description = $"{this}";
			}
			return m_description;
		}
	}

	private bool HasLifetimeEvent => !string.IsNullOrEmpty(LifetimeEvent);

	public string HitBreakableEvent { get; private set; }

	public string HitSurfaceEvent
	{
		get
		{
			return m_hitSurfaceEvent;
		}
		protected set
		{
			m_hitSurfaceEvent = value;
		}
	}

	public string LifetimeEvent
	{
		get
		{
			return m_lifetimeEvent;
		}
		protected set
		{
			m_lifetimeEvent = value;
		}
	}

	public string SpawnEvent
	{
		get
		{
			return m_spawnEvent;
		}
		protected set
		{
			m_spawnEvent = value;
		}
	}

	protected virtual void Awake()
	{
		m_projectile = GetComponent<Projectile_RL>();
		m_projectile.OnSpawnRelay.AddListener(OnSpawn);
		m_onCollision = OnCollision;
		m_onDeath = OnDeath;
		m_trimmedName = GetName();
		ProjectileAudioLibraryEntry entry = ProjectileAudioLibrary.GetEntry(m_trimmedName);
		if (entry != null)
		{
			SpawnEvent = entry.SpawnSingleEventPath;
			LifetimeEvent = entry.LifetimeEventPath;
			HitBreakableEvent = entry.HitItemEventPath;
			HitSurfaceEvent = entry.HitSurfaceEventPath;
			DeathEvent = entry.DeathEventPath;
		}
		if (!string.IsNullOrEmpty(LifetimeEvent))
		{
			try
			{
				m_lifeTimeEventInstance = AudioUtility.GetEventInstance(LifetimeEvent, base.transform);
			}
			catch (Exception)
			{
				Debug.LogFormat("<color=red>| {0} | The FMOD Event path you specified for the Lifetime Event <b>({1})</b> is invalid.</color>", this, LifetimeEvent);
			}
		}
	}

	protected string GetName()
	{
		int num = base.name.IndexOf("(Clone)");
		string result = base.name;
		if (num != -1)
		{
			result = base.name.Remove(num);
		}
		return result;
	}

	protected virtual void OnDisable()
	{
		if (GetIsEventPlaying(m_lifeTimeEventInstance))
		{
			AudioManager.Stop(m_lifeTimeEventInstance);
		}
	}

	protected virtual void OnDestroy()
	{
		m_projectile.OnSpawnRelay.RemoveListener(OnSpawn);
		m_projectile.OnCollisionRelay.RemoveListener(m_onCollision);
		m_projectile.OnDeathRelay.RemoveListener(m_onDeath);
		if (m_lifeTimeEventInstance.isValid())
		{
			m_lifeTimeEventInstance.release();
		}
	}

	protected bool GetIsEventPlaying(EventInstance eventInstance)
	{
		if (eventInstance.isValid())
		{
			eventInstance.getPlaybackState(out var state);
			return state != PLAYBACK_STATE.STOPPED;
		}
		return false;
	}

	protected virtual void OnCollision(Projectile_RL projectile, GameObject colliderObj)
	{
		m_lastGameObjectHit = colliderObj;
		if (!m_onlyPlayCollisionWhenHitTerrain && colliderObj.CompareTag("Breakable"))
		{
			AudioManager.PlayOneShot(this, HitBreakableEvent, base.transform.position);
		}
		else if (colliderObj.CompareTag("Platform"))
		{
			AudioManager.PlayOneShot(this, HitSurfaceEvent, base.transform.position);
		}
	}

	protected void OnDeath(Projectile_RL projectile, GameObject colliderObj)
	{
		if (GetIsEventPlaying(m_lifeTimeEventInstance))
		{
			AudioManager.Stop(m_lifeTimeEventInstance);
		}
		if (!m_playDeathAudio)
		{
			return;
		}
		bool flag = true;
		if ((bool)m_lastGameObjectHit)
		{
			string text = m_lastGameObjectHit.name;
			if (text.StartsWith("DamageWall"))
			{
				AudioManager.PlayOneShotAttached(this, "event:/SFX/Spells/sfx_spell_blueWall_hit", base.gameObject);
				m_lastGameObjectHit = null;
				flag = false;
			}
			else if (text.StartsWith("StaticWall"))
			{
				AudioManager.PlayOneShotAttached(this, "event:/SFX/Spells/sfx_spell_shieldWall_damageAbsorb", base.gameObject);
				m_lastGameObjectHit = null;
				flag = false;
			}
		}
		if (flag)
		{
			AudioManager.PlayOneShotAttached(this, DeathEvent, base.gameObject);
		}
	}

	protected virtual void OnSpawn(Projectile_RL projectile, GameObject colliderObj)
	{
		if (m_playSpawnAudioAutomatically && m_playSpawnAudio)
		{
			AudioManager.PlayOneShotAttached(this, SpawnEvent, base.gameObject);
		}
		m_projectile.OnCollisionRelay.AddListener(m_onCollision);
		m_projectile.OnDeathRelay.AddListener(m_onDeath);
		if (m_playLifetimeAudio && HasLifetimeEvent)
		{
			AudioManager.PlayAttached(this, m_lifeTimeEventInstance, base.gameObject);
		}
	}

	public void SetPlaySpawnAudio(bool playSpawnAudio)
	{
		m_playSpawnAudio = playSpawnAudio;
	}

	public void SetPlayLifetimeAudio(bool playLifetimeAudio)
	{
		m_playLifetimeAudio = playLifetimeAudio;
	}

	public void SetPlayDeathAudio(bool playDeathAudio)
	{
		m_playDeathAudio = playDeathAudio;
	}
}
