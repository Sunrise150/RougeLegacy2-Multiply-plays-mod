using RLAudio;
using TMPro;
using UnityEngine;

public class LineagePortrait : MonoBehaviour
{
	[SerializeField]
	private PlayerLookController m_model;

	[SerializeField]
	private TMP_Text m_nameText;

	[SerializeField]
	private bool m_isLineageRoomPortrait;

	public int Index { get; set; }

	public PlayerLookController Model => m_model;

	public CharacterData PortraitCharData { get; private set; }

	private void Awake()
	{
		if (m_isLineageRoomPortrait && Model.VisualsGameObject != null)
		{
			Model.VisualsGameObject.SetLayerRecursively(24, includeInactive: true);
		}
	}

	public void SetPortraitLook(CharacterData charData)
	{
		AnimBehaviourEventEmitter.DisableEmitter_STATIC = true;
		m_nameText.text = LocalizationManager.GetLocalizedPlayerName(charData);
		PortraitCharData = charData;
		m_model.InitializeLook(charData);
		m_model.Animator.SetBool("PortraitPose", value: false);
		m_model.Animator.SetBool("RetirePose", value: false);
		m_model.Animator.SetBool("Victory", value: false);
		m_model.transform.SetLocalPositionX(0f);
		if (charData.IsVictory)
		{
			m_model.Animator.SetBool("Victory", value: true);
			m_model.transform.SetLocalPositionX(0.25f);
			m_model.Animator.Play("Victory", 0, 1f);
		}
		else if (charData.IsRetired)
		{
			m_model.Animator.SetBool("RetirePose", value: true);
			m_model.Animator.Play("RetirePose", 0, 1f);
			if (charData.Weapon != AbilityType.BoxingGloveWeapon && charData.Weapon != AbilityType.ExplosiveHandsWeapon)
			{
				if ((bool)m_model.CurrentWeaponGeo)
				{
					m_model.CurrentWeaponGeo.gameObject.SetActive(value: false);
				}
				if ((bool)m_model.SecondaryWeaponGeo)
				{
					m_model.SecondaryWeaponGeo.gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			m_model.Animator.SetBool("PortraitPose", value: true);
			m_model.Animator.Play("PortraitPose", 0, 1f);
		}
		m_model.Animator.speed = 0f;
		m_model.Animator.Update(1f);
		AnimBehaviourEventEmitter.DisableEmitter_STATIC = false;
	}
}
