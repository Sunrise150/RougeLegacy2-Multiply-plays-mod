using System;
using TMPro;
using UnityEngine;

public class PurchaseBoxDialogueController : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_purchaseBoxDialogueText;

	[SerializeField]
	private GameObject m_purchaseBoxGoldGO;

	[SerializeField]
	private GameObject m_purchaseBoxOreTextGO;

	[SerializeField]
	private bool m_usesLocIDs = true;

	[SerializeField]
	private string[] m_welcomeDialogue;

	[SerializeField]
	private string m_purchaseSuccessfulDialogue;

	[SerializeField]
	private string m_purchaseFailedNoMoneyDialogue;

	[SerializeField]
	private string m_purchaseFailedNoOreDialogue;

	[SerializeField]
	private string[] m_gearDescriptionDialogue;

	[SerializeField]
	private string m_gearNotFoundDialogue;

	private float m_flavourDescriptionTimer;

	private Vector3 m_storedGoldTextScale;

	private Vector3 m_storedOreTextScale;

	private string m_purchaseDialogueLocID;

	private Action<MonoBehaviour, EventArgs> m_onPurchaseDialogueUpdated;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private void Awake()
	{
		if ((bool)m_purchaseBoxGoldGO)
		{
			m_storedGoldTextScale = m_purchaseBoxGoldGO.transform.localScale;
		}
		if ((bool)m_purchaseBoxOreTextGO)
		{
			m_storedOreTextScale = m_purchaseBoxOreTextGO.transform.localScale;
		}
		m_onPurchaseDialogueUpdated = OnPurchaseDialogueUpdated;
		m_refreshText = RefreshText;
	}

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.UpdatePurchaseBoxDialogue, m_onPurchaseDialogueUpdated);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.UpdatePurchaseBoxDialogue, m_onPurchaseDialogueUpdated);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void OnPurchaseDialogueUpdated(object sender, EventArgs args)
	{
		PurchaseBoxDialogueEventArgs obj = args as PurchaseBoxDialogueEventArgs;
		string text = null;
		switch (obj.DialogueType)
		{
		case PurchaseBoxDialogueType.Welcome:
			if (m_welcomeDialogue != null && m_welcomeDialogue.Length != 0)
			{
				text = m_welcomeDialogue[UnityEngine.Random.Range(0, m_welcomeDialogue.Length)];
				m_flavourDescriptionTimer = Time.unscaledTime;
			}
			break;
		case PurchaseBoxDialogueType.PurchaseSuccessful:
			text = m_purchaseSuccessfulDialogue;
			m_flavourDescriptionTimer = 0f;
			break;
		case PurchaseBoxDialogueType.PurchaseFailed_NoMoney:
			if ((bool)m_purchaseBoxGoldGO)
			{
				m_purchaseBoxGoldGO.transform.localScale = m_storedGoldTextScale + new Vector3(0.25f, 0.25f, 0.25f);
				TweenManager.TweenTo_UnscaledTime(m_purchaseBoxGoldGO.transform, 0.25f, Ease.Back.EaseOut, "localScale.x", m_storedGoldTextScale.x, "localScale.y", m_storedGoldTextScale.y, "localScale.z", m_storedGoldTextScale.z);
			}
			text = m_purchaseFailedNoMoneyDialogue;
			m_flavourDescriptionTimer = 0f;
			break;
		case PurchaseBoxDialogueType.PurchaseFailed_NoOre:
			if ((bool)m_purchaseBoxOreTextGO)
			{
				m_purchaseBoxOreTextGO.transform.localScale = m_storedOreTextScale + new Vector3(0.25f, 0.25f, 0.25f);
				TweenManager.TweenTo_UnscaledTime(m_purchaseBoxOreTextGO.transform, 0.25f, Ease.Back.EaseOut, "localScale.x", m_storedOreTextScale.x, "localScale.y", m_storedOreTextScale.y, "localScale.z", m_storedOreTextScale.z);
			}
			text = m_purchaseFailedNoOreDialogue;
			m_flavourDescriptionTimer = 0f;
			break;
		case PurchaseBoxDialogueType.GearDescription:
			if (m_gearDescriptionDialogue != null && m_gearDescriptionDialogue.Length != 0 && Time.unscaledTime > m_flavourDescriptionTimer + 5f)
			{
				text = m_gearDescriptionDialogue[UnityEngine.Random.Range(0, m_gearDescriptionDialogue.Length)];
				m_flavourDescriptionTimer = Time.unscaledTime;
			}
			break;
		case PurchaseBoxDialogueType.GearNotFound:
			text = m_gearNotFoundDialogue;
			m_flavourDescriptionTimer = 0f;
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			if (m_usesLocIDs)
			{
				m_purchaseDialogueLocID = text;
				text = LocalizationManager.GetString(m_purchaseDialogueLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			}
			m_purchaseBoxDialogueText.text = text;
		}
	}

	private void RefreshText(object sender, EventArgs args)
	{
		if (m_usesLocIDs)
		{
			string @string = LocalizationManager.GetString(m_purchaseDialogueLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			m_purchaseBoxDialogueText.text = @string;
		}
	}
}
