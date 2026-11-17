using RL_Windows;

public class BackOptionItem : ExecuteImmediateOptionItem
{
	public override void ActivateOption()
	{
		base.ActivateOption();
		if (!WindowManager.GetIsWindowOpen(WindowID.Pause))
		{
			WindowManager.SetWindowIsOpen(WindowID.Options, isOpen: false);
			WindowManager.SetWindowIsOpen(WindowID.MainMenu, isOpen: true);
			OptionsWindowController.EnteredFromOtherSubmenu = false;
		}
		else
		{
			WindowManager.SetWindowIsOpen(WindowID.Options, isOpen: false);
		}
	}
}
