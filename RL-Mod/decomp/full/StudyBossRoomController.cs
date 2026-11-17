using System;
using System.Collections;
using RLAudio;
using UnityEngine;

public class StudyBossRoomController : BossRoomController
{
	[SerializeField]
	private EnemySpawnController m_mimicChestSpawner;

	private IHazardSpawnController[] m_hazardsToDisableOnModeShift;

	private SpecialPlatformSpawnController[] m_platformsToDisableOnModeShift;

	private bool m_mimicBossDefeated;

	public EnemyController MimicChestBoss
	{
		get
		{
			if ((bool)m_mimicChestSpawner)
			{
				return m_mimicChestSpawner.EnemyInstance;
			}
			return null;
		}
	}

	public override float BossHealthAsPercentage
	{
		get
		{
			if ((bool)MimicChestBoss && MimicChestBoss.isActiveAndEnabled)
			{
				return MimicChestBoss.CurrentHealth / (float)MimicChestBoss.ActualMaxHealth;
			}
			return base.BossHealthAsPercentage;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_hazardsToDisableOnModeShift = GetComponentsInChildren<IHazardSpawnController>(includeInactive: true);
		m_platformsToDisableOnModeShift = GetComponentsInChildren<SpecialPlatformSpawnController>(includeInactive: true);
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if ((bool)MimicChestBoss)
		{
			MimicChestBoss.GetComponent<Interactable>().TriggerOnEnterEvent.AddListener(ActivateMimicBoss);
			MimicChestBoss.gameObject.SetActive(value: false);
		}
		m_mimicBossDefeated = false;
		base.OnPlayerEnterRoom(sender, eventArgs);
	}

	protected override void RemoveListeners()
	{
		if (m_listenersAssigned && (bool)MimicChestBoss)
		{
			MimicChestBoss.GetComponent<Interactable>().TriggerOnEnterEvent.RemoveListener(ActivateMimicBoss);
		}
		base.RemoveListeners();
	}

	protected override void OnBossDeath(object sender, EventArgs args)
	{
		EnemyDeathEventArgs enemyDeathEventArgs = args as EnemyDeathEventArgs;
		if (enemyDeathEventArgs.Victim == MimicChestBoss)
		{
			m_mimicBossDefeated = true;
		}
		else if (enemyDeathEventArgs.Victim == base.Boss)
		{
			IHazardSpawnController[] hazardsToDisableOnModeShift = m_hazardsToDisableOnModeShift;
			foreach (IHazardSpawnController hazardSpawnController in hazardsToDisableOnModeShift)
			{
				if (hazardSpawnController.Type != HazardType.SnowMound && hazardSpawnController.Hazard != null)
				{
					hazardSpawnController.Hazard.gameObject.SetActive(value: false);
				}
			}
			SpecialPlatformSpawnController[] platformsToDisableOnModeShift = m_platformsToDisableOnModeShift;
			foreach (SpecialPlatformSpawnController specialPlatformSpawnController in platformsToDisableOnModeShift)
			{
				if ((bool)specialPlatformSpawnController.SpecialPlatformInstance)
				{
					specialPlatformSpawnController.SpecialPlatformInstance.gameObject.SetActive(value: false);
				}
			}
			if ((bool)MimicChestBoss)
			{
				StartCoroutine(SpawnMimicBossIntro());
			}
		}
		base.OnBossDeath(sender, args);
	}

	protected IEnumerator SpawnMimicBossIntro()
	{
		OutroStartRelay.Dispatch();
		m_waitYield.CreateNew(3f);
		yield return m_waitYield;
		EnemyController mimicBoss = MimicChestBoss;
		mimicBoss.gameObject.SetActive(value: true);
		mimicBoss.Animator.Play("BossIntro_Closed", 0, 1f);
		mimicBoss.transform.localPosition = new Vector3(-8000f, -8000f);
		while (!mimicBoss.IsInitialized)
		{
			yield return null;
		}
		if (mimicBoss.IsFacingRight)
		{
			bool lockFlip = mimicBoss.LockFlip;
			mimicBoss.LockFlip = false;
			mimicBoss.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: false, ignoreTurnParam: true);
			mimicBoss.LockFlip = lockFlip;
		}
		mimicBoss.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: false);
		mimicBoss.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
		mimicBoss.HitboxController.SetHitboxActiveState(HitboxType.Terrain, active: true);
		mimicBoss.SetVelocity(0f, 0f, additive: false);
		mimicBoss.ControllerCorgi.GravityActive(state: false);
		mimicBoss.ControllerCorgi.enabled = false;
		float chestOpacity = 0f;
		Renderer mimicRenderer = mimicBoss.GetComponentInChildren<Renderer>();
		mimicRenderer.material.SetFloat("_Opacity", chestOpacity);
		Vector3 position = base.Chest.transform.position;
		position.y += 1f;
		mimicBoss.transform.localPosition = position;
		mimicBoss.UpdateBounds();
		mimicBoss.ResetCollisionState();
		Interactable mimicInteract = mimicBoss.GetComponent<Interactable>();
		mimicInteract.SetIsInteractableActive(isActive: false);
		position.y -= 1f;
		TweenManager.TweenTo(mimicBoss.transform, 1f, Ease.Quad.EaseOut, "position.x", position.x, "position.y", position.y);
		m_chestAudioEventInstance = AudioUtility.GetEventInstance("event:/SFX/Interactables/sfx_env_prop_bossChest_spawn_loop", base.Chest.transform);
		AudioManager.PlayAttached(this, m_chestAudioEventInstance, base.Chest.gameObject);
		while (chestOpacity < 1f)
		{
			mimicRenderer.material.SetFloat("_Opacity", chestOpacity);
			chestOpacity += Time.deltaTime;
			yield return null;
		}
		AudioManager.Stop(m_chestAudioEventInstance);
		mimicInteract.SetIsInteractableActive(isActive: true);
	}

	private void ActivateMimicBoss(GameObject sender)
	{
		StartCoroutine(ActivateMimicBossCoroutine());
	}

	private IEnumerator ActivateMimicBossCoroutine()
	{
		Debug.Log("Activating Mimic boss");
		EnemyController mimicBoss = MimicChestBoss;
		yield return mimicBoss.LogicController.LogicScript.SpawnAnim();
		mimicBoss.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: true);
		mimicBoss.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
		mimicBoss.HitboxController.SetHitboxActiveState(HitboxType.Terrain, active: false);
		mimicBoss.ControllerCorgi.enabled = true;
		mimicBoss.UpdateBounds();
		mimicBoss.ResetCollisionState();
		mimicBoss.ControllerCorgi.GravityActive(state: true);
		mimicBoss.LogicController.TriggerAggro(null, null);
		mimicBoss.Animator.SetBool("IsActive", value: true);
	}
}
