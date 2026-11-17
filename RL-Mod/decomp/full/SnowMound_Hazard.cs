using System.Collections;
using UnityEngine;

public class SnowMound_Hazard : SingleLine_Multi_Hazard, ITerrainOnEnterHitResponse, IHitResponse, ITerrainOnExitHitResponse
{
	[SerializeField]
	private SpriteRenderer m_lineSprite;

	private IHitboxController m_hbController;

	private bool m_slowApplied;

	private Coroutine m_groundCheckCoroutine;

	private static int m_numSnowMoundsCollided_STATIC;

	private static GenericEffect m_snowMoundEffect_STATIC;

	private static Coroutine m_pauseSnowMoundEffectCoroutine_STATIC;

	private static SnowMound_Hazard m_activeSnowMound_STATIC;

	protected override void Awake()
	{
		base.Awake();
		m_hbController = GetComponentInChildren<IHitboxController>();
	}

	public override void Initialize(PivotPoint pivot, int width, HazardArgs hazardArgs)
	{
		base.Initialize(pivot, width, hazardArgs);
		if (base.transform.localEulerAngles.z > 45f && base.transform.localEulerAngles.z < 315f)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			StartCoroutine(SetSnowMoundWidth(width));
		}
	}

	private IEnumerator SetSnowMoundWidth(int width)
	{
		while (!m_hbController.IsInitialized)
		{
			yield return null;
		}
		m_hbController.RepeatHitDuration = 0f;
		BoxCollider2D obj = m_hbController.GetCollider(HitboxType.Terrain) as BoxCollider2D;
		Vector2 size = obj.size;
		size.x = width;
		obj.size = size;
		m_lineSprite.size = new Vector2(width, m_lineSprite.size.y);
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (otherHBController.RootGameObject.CompareTag("Player") && !m_slowApplied)
		{
			m_groundCheckCoroutine = StartCoroutine(IsGroundedCheck());
		}
	}

	private IEnumerator IsGroundedCheck()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		while (!playerController.IsGrounded)
		{
			yield return null;
		}
		m_slowApplied = true;
		m_numSnowMoundsCollided_STATIC++;
		if (m_numSnowMoundsCollided_STATIC != 1)
		{
			yield break;
		}
		playerController.MovementSpeedMod -= 0.35f;
		if (m_snowMoundEffect_STATIC == null || !m_snowMoundEffect_STATIC.IsPlaying)
		{
			if (m_pauseSnowMoundEffectCoroutine_STATIC != null && (bool)m_activeSnowMound_STATIC)
			{
				m_activeSnowMound_STATIC.StopCoroutine(m_pauseSnowMoundEffectCoroutine_STATIC);
				m_pauseSnowMoundEffectCoroutine_STATIC = null;
				m_activeSnowMound_STATIC = null;
			}
			m_snowMoundEffect_STATIC = EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "SnowDebris_Effect", Vector3.zero, 999999f, EffectStopType.Gracefully) as GenericEffect;
			m_snowMoundEffect_STATIC.transform.SetParent(playerController.Pivot.transform, worldPositionStays: false);
			if (playerController.transform.localScale.x > 1.4f)
			{
				float num = m_snowMoundEffect_STATIC.transform.localScale.x / 1.4f;
				Vector3 localScale = new Vector3(num, num, num);
				m_snowMoundEffect_STATIC.transform.localScale = localScale;
			}
			m_pauseSnowMoundEffectCoroutine_STATIC = StartCoroutine(PauseSnowMoundEffectCoroutine());
			m_activeSnowMound_STATIC = this;
		}
	}

	public void TerrainOnExitHitResponse(IHitboxController otherHBController)
	{
		if (!otherHBController.RootGameObject.CompareTag("Player"))
		{
			return;
		}
		if (m_groundCheckCoroutine != null)
		{
			StopCoroutine(m_groundCheckCoroutine);
		}
		if (!m_slowApplied)
		{
			return;
		}
		m_slowApplied = false;
		m_numSnowMoundsCollided_STATIC--;
		if (m_numSnowMoundsCollided_STATIC != 0)
		{
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.MovementSpeedMod += 0.35f;
		if (m_pauseSnowMoundEffectCoroutine_STATIC != null && (bool)m_activeSnowMound_STATIC)
		{
			m_activeSnowMound_STATIC.StopCoroutine(m_pauseSnowMoundEffectCoroutine_STATIC);
			m_pauseSnowMoundEffectCoroutine_STATIC = null;
			m_activeSnowMound_STATIC = null;
		}
		if ((bool)m_snowMoundEffect_STATIC)
		{
			if (m_snowMoundEffect_STATIC.IsPlaying)
			{
				if (m_snowMoundEffect_STATIC.isActiveAndEnabled)
				{
					m_snowMoundEffect_STATIC.Stop(EffectStopType.Gracefully);
				}
				else
				{
					m_snowMoundEffect_STATIC.Stop(EffectStopType.Immediate);
				}
			}
			m_snowMoundEffect_STATIC = null;
		}
		EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "SnowDebrisExit_Effect", playerController.transform.position, 0.1f, EffectStopType.Gracefully);
	}

	private IEnumerator PauseSnowMoundEffectCoroutine()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		while ((bool)m_snowMoundEffect_STATIC)
		{
			if (m_snowMoundEffect_STATIC.ParticleSystem.isPlaying && Mathf.Abs(playerController.Velocity.x) < 0.01f)
			{
				m_snowMoundEffect_STATIC.ParticleSystem.Stop();
			}
			else if (!m_snowMoundEffect_STATIC.ParticleSystem.isPlaying && Mathf.Abs(playerController.Velocity.x) >= 0.01f)
			{
				m_snowMoundEffect_STATIC.ParticleSystem.Play();
			}
			yield return null;
		}
	}

	public override void ResetHazard()
	{
		if (m_slowApplied)
		{
			PlayerManager.GetPlayerController().MovementSpeedMod += 0.35f;
			if (m_pauseSnowMoundEffectCoroutine_STATIC != null && (bool)m_activeSnowMound_STATIC)
			{
				m_activeSnowMound_STATIC.StopCoroutine(m_pauseSnowMoundEffectCoroutine_STATIC);
				m_pauseSnowMoundEffectCoroutine_STATIC = null;
				m_activeSnowMound_STATIC = null;
			}
			if (m_snowMoundEffect_STATIC != null)
			{
				if (m_snowMoundEffect_STATIC.isActiveAndEnabled)
				{
					m_snowMoundEffect_STATIC.Stop(EffectStopType.Gracefully);
				}
				else
				{
					m_snowMoundEffect_STATIC.Stop(EffectStopType.Immediate);
				}
				m_snowMoundEffect_STATIC = null;
			}
		}
		StopAllCoroutines();
		m_slowApplied = false;
	}
}
