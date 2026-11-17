using System.Collections;
using FMOD.Studio;
using RLAudio;
using UnityEngine;

public class Buzzsaw_Hazard : SingleLine_Multi_Hazard, IBodyOnEnterHitResponse, IHitResponse
{
	private const string POSITION_PARAMETER_NAME = "sawPosition";

	private const string ACTIVE_PARAMETER_NAME = "sawActive";

	[SerializeField]
	private GameObject m_buzzSawSprite;

	[SerializeField]
	private SpriteRenderer m_lineSprite;

	[SerializeField]
	private GameObject m_resonantParticleEffect;

	private bool m_movingRight;

	private float m_buzzsawHalfWidth;

	private float m_buzzsawHeight;

	private IHitboxController m_hbController;

	private WaitRL_Yield m_retractWaitYield;

	private Coroutine m_retractBuzzsawCoroutine;

	private float m_movementStartTime;

	private EventInstance m_buzzSawEventInstance;

	public float SawPosition
	{
		get
		{
			float num = m_pivot.transform.localPosition.x - -1f * m_halfWidth;
			float num2 = 2f * m_halfWidth;
			return num / num2;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_hbController = GetComponentInChildren<IHitboxController>();
		m_retractWaitYield = new WaitRL_Yield(2f);
	}

	public override void Initialize(PivotPoint pivot, int width, HazardArgs hazardArgs)
	{
		base.Initialize(pivot, width, hazardArgs);
		m_lineSprite.gameObject.transform.localPosition = m_startingLocalPos;
		m_lineSprite.size = new Vector2(width, m_lineSprite.size.y);
		if (!m_buzzSawEventInstance.isValid())
		{
			m_buzzSawEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_hazard_sawTrap_on_loop", m_buzzSawSprite.transform);
		}
		StartCoroutine(SetHalfBoundsWidthCoroutine());
		if (m_buzzSawEventInstance.isValid())
		{
			AudioManager.Play(null, m_buzzSawEventInstance);
			m_buzzSawEventInstance.setParameterByName("sawActive", 1f);
		}
	}

	private IEnumerator SetHalfBoundsWidthCoroutine()
	{
		while (!m_hbController.IsInitialized)
		{
			yield return null;
		}
		m_buzzsawHalfWidth = m_hbController.GetCollider(HitboxType.Body).bounds.size.x / 2f;
		m_buzzsawHeight = m_hbController.GetCollider(HitboxType.Body).bounds.size.y;
	}

	private void FixedUpdate()
	{
		Vector3 localPosition = m_pivot.transform.localPosition;
		float num = 0f;
		float num2 = 0f;
		switch (m_pivotPoint)
		{
		case PivotPoint.Center:
			num = 0f - m_halfWidth + m_buzzsawHalfWidth;
			num2 = m_halfWidth - m_buzzsawHalfWidth;
			break;
		case PivotPoint.Left:
			num = 0f + m_buzzsawHalfWidth;
			num2 = 0f + m_halfWidth * 2f - m_buzzsawHalfWidth;
			break;
		case PivotPoint.Right:
			num = 0f - m_halfWidth * 2f + m_buzzsawHalfWidth;
			num2 = 0f - m_buzzsawHalfWidth;
			break;
		}
		_ = (Time.time - m_movementStartTime) / 10f;
		if (m_movingRight)
		{
			localPosition.x += 10f * Time.deltaTime;
			if (localPosition.x >= num2)
			{
				localPosition.x = num2;
				m_movingRight = !m_movingRight;
			}
		}
		else
		{
			localPosition.x -= 10f * Time.deltaTime;
			if (localPosition.x <= num)
			{
				localPosition.x = num;
				m_movingRight = !m_movingRight;
			}
		}
		m_pivot.transform.localPosition = localPosition;
		if (m_buzzSawEventInstance.isValid())
		{
			m_buzzSawEventInstance.setParameterByName("sawPosition", SawPosition);
		}
	}

	public override void ResetHazard()
	{
		m_movementStartTime = Time.time;
		if (this != null && PlayerManager.IsInstantiated && !PlayerManager.IsDisposed)
		{
			_ = m_pivot.transform.localPosition;
			if (PlayerManager.GetPlayer() == null)
			{
				return;
			}
			if (PlayerManager.GetPlayer().transform.position.x < base.transform.position.x)
			{
				m_movingRight = true;
			}
			else
			{
				m_movingRight = false;
			}
			m_pivot.transform.localPosition = m_startingLocalPos;
			if (m_hbController != null)
			{
				if (m_retractBuzzsawCoroutine != null)
				{
					StopCoroutine(m_retractBuzzsawCoroutine);
				}
				if (base.isActiveAndEnabled)
				{
					base.Animator.Play("Buzzsaw_Retracted_Animation");
					m_hbController.DisableAllCollisions = true;
					ExtractBuzzsaw();
				}
			}
		}
		base.ResetHazard();
	}

	public void BodyOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (otherHBController.IsNativeNull() || (bool)otherHBController.RootGameObject.GetComponent<DownstrikeProjectile_RL>())
		{
			if (m_retractBuzzsawCoroutine != null)
			{
				StopCoroutine(m_retractBuzzsawCoroutine);
			}
			m_retractBuzzsawCoroutine = StartCoroutine(RetractBuzzsawCoroutine());
		}
	}

	private IEnumerator RetractBuzzsawCoroutine()
	{
		RetractBuzzsaw();
		m_retractWaitYield.Reset();
		yield return m_retractWaitYield;
		ExtractBuzzsaw();
	}

	private void RetractBuzzsaw()
	{
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/sfx_hazard_sawTrap_destroy", m_buzzSawSprite.transform.position);
		if (m_buzzSawEventInstance.isValid())
		{
			m_buzzSawEventInstance.setParameterByName("sawActive", 0f);
		}
		m_hbController.DisableAllCollisions = true;
		base.Animator.SetBool("Retract", value: true);
		m_resonantParticleEffect.SetActive(value: false);
	}

	private void ExtractBuzzsaw()
	{
		if (m_buzzSawEventInstance.isValid())
		{
			m_buzzSawEventInstance.setParameterByName("sawActive", 1f);
		}
		m_hbController.DisableAllCollisions = false;
		base.Animator.SetBool("Retract", value: false);
		m_resonantParticleEffect.SetActive(value: true);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_buzzSawEventInstance.isValid())
		{
			AudioManager.Stop(m_buzzSawEventInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
	}

	private void OnDestroy()
	{
		if (m_buzzSawEventInstance.isValid())
		{
			m_buzzSawEventInstance.release();
		}
	}
}
