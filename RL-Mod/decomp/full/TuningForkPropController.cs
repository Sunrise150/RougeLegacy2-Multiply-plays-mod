using System.Collections;
using FMODUnity;
using RLAudio;
using UnityEngine;

public class TuningForkPropController : BaseSpecialPropController, ITerrainOnEnterHitResponse, IHitResponse
{
	private const float HITBOX_DISABLE_DURATION = 2f;

	[SerializeField]
	private string m_projectileName;

	[SerializeField]
	private GameObject m_spawnPos;

	[SerializeField]
	private StudioEventEmitter m_onHitEventEmitter;

	private Animator m_animator;

	private IHitboxController m_hbController;

	private bool m_projectileAdded;

	private bool m_textPopupPlayed;

	private TextPopupObj m_textPopup;

	protected override void Awake()
	{
		base.Awake();
		m_animator = GetComponent<Animator>();
		m_hbController = GetComponentInChildren<IHitboxController>();
		m_hbController.DisableAllCollisions = false;
	}

	protected override void InitializePooledPropOnEnter()
	{
		if (!m_projectileAdded)
		{
			ProjectileManager.Instance.AddProjectileToPool(m_projectileName);
			m_projectileAdded = true;
		}
		m_hbController.DisableAllCollisions = false;
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveTuningForkTriggered))
		{
			m_hbController.SetHitboxActiveState(HitboxType.Terrain, active: false);
		}
		else
		{
			m_hbController.SetHitboxActiveState(HitboxType.Terrain, active: true);
		}
	}

	public void TuningForkHit()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.CaveTuningForkTriggered, value: true);
		MapController.SetCaveWhitePipVisibility(visible: true);
		m_animator.SetTrigger("Vibrate");
		m_animator.SetTrigger("StopVibrateEase");
		m_hbController.SetHitboxActiveState(HitboxType.Terrain, active: false);
		AudioManager.Play(null, m_onHitEventEmitter);
		if (!m_textPopupPlayed)
		{
			m_textPopupPlayed = true;
			StartCoroutine(MapUpdatedTextPopupCoroutine());
		}
		if (!string.IsNullOrEmpty(m_projectileName))
		{
			ProjectileManager.FireProjectile(base.gameObject, m_projectileName, m_spawnPos.transform.position, matchFacing: false, 0f, 1f, useAbsPos: true);
		}
	}

	private IEnumerator MapUpdatedTextPopupCoroutine()
	{
		Vector2 position = base.transform.position;
		position.y += 5f;
		TextPopupObj popup = TextPopupManager.DisplayLocIDText(TextPopupType.OutOfAmmo, "LOC_ID_STATUS_EFFECT_CAVE_MAP_UPDATED_1", StringGenderType.Male, position);
		popup.StopAllCoroutines();
		m_textPopup = popup;
		float speed = 1.05f;
		yield return TweenManager.TweenBy(popup.transform, speed, Ease.Quad.EaseOut, "localPosition.y", 2).TweenCoroutine;
		TweenManager.TweenBy(popup.transform, speed, Ease.Quad.EaseIn, "localPosition.y", -1);
		yield return TweenManager.TweenTo(popup.TMPText, speed / 2f, Ease.None, "delay", speed / 2f, "alpha", 0).TweenCoroutine;
		popup.gameObject.SetActive(value: false);
		m_textPopup = null;
	}

	private IEnumerator DisableHitbox(float secs)
	{
		m_hbController.DisableAllCollisions = true;
		float delay = Time.time + secs;
		while (Time.time < delay)
		{
			yield return null;
		}
		m_hbController.DisableAllCollisions = false;
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveTuningForkTriggered))
		{
			TuningForkHit();
		}
		m_hbController.SetHitboxActiveState(HitboxType.Terrain, active: false);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)m_textPopup)
		{
			m_textPopup.gameObject.SetActive(value: false);
			m_textPopup = null;
		}
	}
}
