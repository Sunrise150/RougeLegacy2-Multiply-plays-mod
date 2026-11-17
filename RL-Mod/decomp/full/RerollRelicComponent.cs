using System.Globalization;
using RLAudio;
using SceneManagement_RL;
using TMPro;
using UnityEngine;

public class RerollRelicComponent : BaseSpecialPropController
{
	[SerializeField]
	private RelicRoomPropController m_relicRoomProp;

	[SerializeField]
	private GameObject m_rerollIcon;

	[SerializeField]
	private TMP_Text m_rerollText;

	private float m_startingY;

	protected override void Awake()
	{
		base.Awake();
		m_startingY = m_rerollIcon.transform.localPosition.y;
		m_rerollIcon.SetActive(value: false);
		m_relicRoomProp.DisableSpecialPropRelay.AddListener(DisableProp);
	}

	private void OnDestroy()
	{
		m_relicRoomProp.DisableSpecialPropRelay.RemoveListener(DisableProp);
	}

	protected override void InitializePooledPropOnEnter()
	{
		UpdateRolls();
	}

	public void RerollRelic()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		AmbientSoundController.Instance.StopOnTransition = false;
		SceneLoader_RL.RunTransitionWithLogic(RerollRelicTransitionOut, TransitionID.QuickSwipe, cleanup: false);
		AmbientSoundController.Instance.StopOnTransition = true;
	}

	private void RerollRelicTransitionOut()
	{
		m_relicRoomProp.RollRelics(1, addToTotalRoomRolls: true);
		SaveManager.PlayerSaveData.TimesRolledRelic++;
		UpdateRolls();
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	private void UpdateRolls()
	{
		int num = (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Reroll_Relic).CurrentStatGain - SaveManager.PlayerSaveData.TimesRolledRelic;
		int num2 = (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Reroll_Relic_Room_Cap).CurrentStatGain + 1;
		int num3 = 1;
		if (int.TryParse(GetRoomMiscData("TotalRelicRolls"), NumberStyles.Any, SaveManager.CultureInfo, out var result))
		{
			num3 = result;
		}
		m_rerollText.text = string.Format(LocalizationManager.GetString("LOC_ID_FORMATTER_REROLLS_REMAINING_1", isFemale: false), num, num2 - num3);
		if (num <= 0 || base.SpecialRoomController.IsRoomComplete || num3 > num2)
		{
			DisableProp(firstTimeDisabled: true);
			return;
		}
		m_rerollIcon.SetActive(value: true);
		m_interactable.SetIsInteractableActive(isActive: true);
	}

	private void Update()
	{
		float y = m_startingY + Mathf.Sin(Time.timeSinceLevelLoad * 2f) / 8f;
		Vector3 localPosition = m_rerollIcon.transform.localPosition;
		localPosition.y = y;
		m_rerollIcon.transform.localPosition = localPosition;
	}

	protected override void DisableProp(bool firstTimeDisabled)
	{
		base.DisableProp(firstTimeDisabled);
		m_rerollIcon.SetActive(value: false);
	}
}
