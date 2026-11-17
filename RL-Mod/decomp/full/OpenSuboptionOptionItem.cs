using RL_Windows;
using UnityEngine;

public class OpenSuboptionOptionItem : ExecuteImmediateOptionItem
{
	[SerializeField]
	private SuboptionType m_suboptionType;

	public SuboptionType SuboptionType
	{
		get
		{
			return m_suboptionType;
		}
		set
		{
			m_suboptionType = value;
		}
	}

	public override void ActivateOption()
	{
		base.ActivateOption();
		WindowManager.SetWindowIsOpen(WindowID.Suboptions, isOpen: true);
		(WindowManager.GetWindowController(WindowID.Suboptions) as SuboptionsWindowController).ChangeSuboptionDisplayed(m_suboptionType);
	}
}
