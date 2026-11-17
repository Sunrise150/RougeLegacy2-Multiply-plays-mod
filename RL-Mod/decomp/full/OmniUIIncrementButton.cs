using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class OmniUIIncrementButton<T> : OmniUIButton, IOmniUIIncrementButton
{
	[SerializeField]
	protected bool m_isDecrementButton;

	[SerializeField]
	protected TMP_Text m_levelText;

	protected List<T> m_incrementList;

	protected int m_selectedIndex;

	public abstract void InitializeIncrementList();

	public override void OnConfirmButtonPressed()
	{
		if (m_incrementList != null && m_incrementList.Count > 0)
		{
			base.OnConfirmButtonPressed();
			int selectedIndex = m_selectedIndex;
			if (m_isDecrementButton)
			{
				m_selectedIndex--;
			}
			else
			{
				m_selectedIndex++;
			}
			if (m_selectedIndex < 0)
			{
				m_selectedIndex = m_incrementList.Count - 1;
			}
			else if (m_selectedIndex >= m_incrementList.Count)
			{
				m_selectedIndex = 0;
			}
			if (m_selectedIndex != selectedIndex)
			{
				UpdateIncrementText();
			}
		}
	}

	protected abstract void UpdateIncrementText();
}
