using UnityEngine;

public class QuitGameOptionItem : ExecuteImmediateOptionItem
{
	public override void ActivateOption()
	{
		base.ActivateOption();
		Application.Quit(10);
	}
}
