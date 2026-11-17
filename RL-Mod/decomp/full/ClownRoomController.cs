using System;
using System.Collections;
using RL_Windows;
using UnityEngine;

public class ClownRoomController : MonoBehaviour
{
	[SerializeField]
	private AbilityType m_challengeAbilityType;

	[SerializeField]
	private int m_bronzeGoal;

	[SerializeField]
	private int m_silverGoal;

	[SerializeField]
	private int m_goldGoal;

	[SerializeField]
	private int m_startingAmmo;

	private ClownRoomEnteredEventArgs m_clownRoomArgs;

	private BaseAbility_RL m_challengeAbility;

	private BaseAbility_RL m_storedWeaponAbility;

	private BaseAbility_RL m_storedTalentAbility;

	private BaseAbility_RL m_storedSpellAbility;

	private TunnelSpawnController m_tunnelSpawnController;

	private WaitUntil m_waitUntilAttackReleasedYield;

	private WaitUntil m_clownRoomEndedYield;

	private Action<MonoBehaviour, EventArgs> m_onEnemyDeath;

	private Action<MonoBehaviour, EventArgs> m_onCastAbility;

	public int BronzeGoal => m_bronzeGoal;

	public int SilverGoal => m_silverGoal;

	public int GoldGoal => m_goldGoal;

	public int CurrentGoalAmount { get; protected set; }

	public int StartingAmmo => m_startingAmmo;

	public int CurrentAmmo { get; protected set; }

	public ClownRoomEventHandler BronzeGoalReachedEvent { get; set; }

	public ClownRoomEventHandler SilverGoalReachedEvent { get; set; }

	public ClownRoomEventHandler GoldGoalReachedEvent { get; set; }

	public ClownRoomEventHandler CurrentAmmoChangedEvent { get; set; }

	public ClownRoomEventHandler CurrentGoalAmountChangedEvent { get; set; }

	public BaseRoom Room { get; private set; }

	public ClownGoalType GetClownRewardType()
	{
		if (CurrentGoalAmount >= GoldGoal)
		{
			return ClownGoalType.Gold;
		}
		if (CurrentGoalAmount >= SilverGoal)
		{
			return ClownGoalType.Silver;
		}
		if (CurrentGoalAmount >= BronzeGoal)
		{
			return ClownGoalType.Bronze;
		}
		return ClownGoalType.None;
	}

	private void Awake()
	{
		Room = GetComponent<Room>();
		m_onCastAbility = OnCastAbility;
		m_onEnemyDeath = OnEnemyDeath;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnter);
		Room.PlayerExitRelay.AddListener(OnPlayerExit);
		m_clownRoomArgs = new ClownRoomEnteredEventArgs(this);
		m_tunnelSpawnController = Room.gameObject.GetComponentInChildren<TunnelSpawnController>();
	}

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		InitializeChallengeAbility();
		m_waitUntilAttackReleasedYield = new WaitUntil(() => PlayerManager.GetPlayerController().CastAbility.GetAbility(CastAbilityType.Weapon).CurrentAbilityAnimState == AbilityAnimState.Attack);
		m_clownRoomEndedYield = new WaitUntil(() => PlayerManager.GetPlayerController().IsGrounded && ProjectileManager.ActiveProjectileCount == 0);
	}

	private void InitializeChallengeAbility()
	{
		m_challengeAbility = PlayerManager.GetPlayerController().CastAbility.CreateAbilityInstance(CastAbilityType.Weapon, m_challengeAbilityType);
	}

	protected virtual void OnPlayerEnter(object sender, RoomViaDoorEventArgs eventArgs)
	{
		ResetClownRoom();
		AssignRoomAbilities();
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerEnterClownRoom, this, m_clownRoomArgs);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyDeath, m_onEnemyDeath);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerWeaponAbilityCast, m_onCastAbility);
	}

	protected virtual void OnPlayerExit(object sender, RoomViaDoorEventArgs eventArgs)
	{
		RevertAbilities();
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerExitClownRoom, this, m_clownRoomArgs);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyDeath, m_onEnemyDeath);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerWeaponAbilityCast, m_onCastAbility);
	}

	protected virtual void OnEnemyDeath(object sender, EventArgs args)
	{
		EnemyRank enemyRank = (args as EnemyDeathEventArgs).Victim.EnemyRank;
		if (enemyRank == EnemyRank.Basic || enemyRank != EnemyRank.Advanced)
		{
			CurrentGoalAmount++;
		}
		else
		{
			CurrentGoalAmount--;
		}
		if (CurrentGoalAmount == BronzeGoal && BronzeGoalReachedEvent != null)
		{
			BronzeGoalReachedEvent();
		}
		if (CurrentGoalAmount == SilverGoal && SilverGoalReachedEvent != null)
		{
			SilverGoalReachedEvent();
		}
		if (CurrentGoalAmount == GoldGoal && GoldGoalReachedEvent != null)
		{
			GoldGoalReachedEvent();
		}
		if (CurrentGoalAmountChangedEvent != null)
		{
			CurrentAmmoChangedEvent();
		}
	}

	protected virtual void OnCastAbility(object sender, EventArgs args)
	{
		StartCoroutine(OnCastAbilityCoroutine());
	}

	private IEnumerator OnCastAbilityCoroutine()
	{
		yield return m_waitUntilAttackReleasedYield;
		CurrentAmmo--;
		if (CurrentAmmoChangedEvent != null)
		{
			CurrentAmmoChangedEvent();
		}
		if (CurrentAmmo <= 0)
		{
			EndClownRoom();
		}
	}

	protected virtual void EndClownRoom()
	{
		StartCoroutine(EndClownRoomCoroutine());
	}

	protected virtual IEnumerator EndClownRoomCoroutine()
	{
		RewiredMapController.SetMapEnabled(GameInputMode.Game, enabled: false);
		yield return m_clownRoomEndedYield;
		yield return new WaitForSeconds(1f);
		if (m_tunnelSpawnController.Tunnel != null)
		{
			m_tunnelSpawnController.Tunnel.Destination.Room.gameObject.GetComponentInChildren<ClownEntranceRoomController>().UnlockChestReward(GetClownRewardType());
		}
		SetEndingDialogue();
		if (m_tunnelSpawnController.Tunnel != null)
		{
			DialogueManager.AddDialogueCompleteEndHandler(ExitClownRoom);
		}
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
	}

	protected virtual void SetEndingDialogue()
	{
		DialogueManager.StartNewDialogue();
		DialogueManager.AddNonLocDialogue("Clown", "TIMES UP!!");
		DialogueManager.AddNonLocDialogue("Clown", "Let's see how you did.");
		DialogueManager.AddNonLocDialogue("Clown", $"You destroyed a total of {CurrentGoalAmount} out of {GoldGoal} targets");
		switch (GetClownRewardType())
		{
		case ClownGoalType.Gold:
			DialogueManager.AddNonLocDialogue("Clown", "You did FANTASTIC.");
			DialogueManager.AddNonLocDialogue("Clown", "Don't get cocky, kid.");
			break;
		case ClownGoalType.Silver:
			DialogueManager.AddNonLocDialogue("Clown", "Not bad, not bad.");
			break;
		case ClownGoalType.Bronze:
			DialogueManager.AddNonLocDialogue("Clown", "MEDIOCRE!!");
			break;
		default:
			DialogueManager.AddNonLocDialogue("Clown", "Y-you couldn't even get a bronze!?");
			break;
		}
		DialogueManager.AddNonLocDialogue("Clown", "Well, you might as well come back for your reward.");
	}

	private void ExitClownRoom()
	{
		if (m_tunnelSpawnController.Tunnel != null)
		{
			m_tunnelSpawnController.Tunnel.ForceEnterTunnel(animate: true);
		}
	}

	protected void AssignRoomAbilities()
	{
		if (m_challengeAbility == null)
		{
			InitializeChallengeAbility();
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		m_storedWeaponAbility = playerController.CastAbility.GetAbility(CastAbilityType.Weapon);
		m_storedSpellAbility = playerController.CastAbility.GetAbility(CastAbilityType.Spell);
		m_storedTalentAbility = playerController.CastAbility.GetAbility(CastAbilityType.Talent);
		playerController.CharacterClass.SetAbility(CastAbilityType.Weapon, m_challengeAbility);
		playerController.CharacterClass.SetAbility(CastAbilityType.Spell, null);
		playerController.CharacterClass.SetAbility(CastAbilityType.Talent, null);
		if (playerController.LookController != null)
		{
			CharacterData characterData = SaveManager.PlayerSaveData.CurrentCharacter.Clone();
			characterData.Weapon = m_challengeAbilityType;
			characterData.Spell = AbilityType.None;
			characterData.Talent = AbilityType.None;
			playerController.LookController.InitializeEquipmentLook(characterData);
		}
	}

	protected void RevertAbilities()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CharacterClass.SetAbility(CastAbilityType.Weapon, m_storedWeaponAbility);
		playerController.CharacterClass.SetAbility(CastAbilityType.Spell, m_storedSpellAbility);
		playerController.CharacterClass.SetAbility(CastAbilityType.Talent, m_storedTalentAbility);
		if (playerController.LookController != null)
		{
			playerController.LookController.InitializeEquipmentLook(SaveManager.PlayerSaveData.CurrentCharacter);
		}
	}

	public virtual void ResetClownRoom()
	{
		CurrentAmmo = StartingAmmo;
		CurrentGoalAmount = 0;
	}

	private void OnDestroy()
	{
		if (Room != null)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnter);
			Room.PlayerExitRelay.RemoveListener(OnPlayerExit);
		}
	}
}
