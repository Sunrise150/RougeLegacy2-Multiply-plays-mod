using System.Collections;
using UnityEngine;

public class LQATutorialTextBoxTestController : MonoBehaviour
{
	private TutorialTextBoxController[] m_textBoxArray;

	private void OnEnable()
	{
		if (m_textBoxArray == null)
		{
			Initialize();
		}
		StartCoroutine(DisplayAllTextBoxes());
	}

	private void Initialize()
	{
		Room component = GetComponent<Room>();
		m_textBoxArray = component.GetComponentsInChildren<TutorialTextBoxController>(includeInactive: true);
		TutorialTextBoxController[] textBoxArray = m_textBoxArray;
		foreach (TutorialTextBoxController obj in textBoxArray)
		{
			obj.StickToCamera = false;
			obj.MustHaveConditions = ConditionFlag.None;
			obj.MustNotHaveConditions = ConditionFlag.None;
			IRootObj componentInParent = obj.GetComponentInParent<IRootObj>();
			if (!componentInParent.IsNativeNull() && componentInParent.gameObject != component.gameObject)
			{
				Interactable component2 = componentInParent.gameObject.GetComponent<Interactable>();
				if ((bool)component2)
				{
					component2.SetIsInteractableActive(isActive: false);
				}
			}
		}
	}

	private IEnumerator DisplayAllTextBoxes()
	{
		yield return null;
		TutorialTextBoxController[] textBoxArray = m_textBoxArray;
		for (int i = 0; i < textBoxArray.Length; i++)
		{
			textBoxArray[i].DisplayText();
		}
	}
}
