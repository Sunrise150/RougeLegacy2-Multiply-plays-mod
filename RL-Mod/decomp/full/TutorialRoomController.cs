using UnityEngine;

public class TutorialRoomController : MonoBehaviour, IRoomConsumer
{
	public static bool InitializeTutorialPlayer;

	private bool m_playerIsInvincible;

	public BaseRoom Room { get; private set; }

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
		}
	}

	private void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs args)
	{
		if (InitializeTutorialPlayer)
		{
			CreateStartingCharacter();
		}
		InitializeTutorialPlayer = false;
		if (!m_playerIsInvincible)
		{
			PlayerManager.GetPlayerController().TakesNoDamage = true;
			m_playerIsInvincible = true;
		}
	}

	private void CreateStartingCharacter()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		CharacterData currentCharacter = SaveManager.PlayerSaveData.CurrentCharacter;
		currentCharacter.IsFemale = CharacterCreator.GetRandomGender(useUnityRandom: true);
		CharacterCreator.GenerateRandomLook(currentCharacter);
		string[] availableNames = CharacterCreator.GetAvailableNames(currentCharacter.IsFemale);
		string text = availableNames[Random.Range(0, availableNames.Length)];
		currentCharacter.Name = text;
		currentCharacter.Spell = AbilityType.FireballSpell;
		currentCharacter.Talent = AbilityType.ShieldBlockTalent;
		currentCharacter.ClassType = ClassType.SwordClass;
		playerController.LookController.InitializeLook(SaveManager.PlayerSaveData.CurrentCharacter);
		playerController.CharacterClass.ClassType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
		playerController.CharacterClass.SetAbility(CastAbilityType.Weapon, SaveManager.PlayerSaveData.CurrentCharacter.Weapon);
		playerController.CharacterClass.SetAbility(CastAbilityType.Spell, SaveManager.PlayerSaveData.CurrentCharacter.Spell);
		playerController.CharacterClass.SetAbility(CastAbilityType.Talent, SaveManager.PlayerSaveData.CurrentCharacter.Talent);
		playerController.ResetCharacter();
		playerController.SetMana(0f, additive: false, runEvents: true);
	}

	private void OnDisable()
	{
		if (!PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().TakesNoDamage = false;
			m_playerIsInvincible = false;
		}
	}
}
