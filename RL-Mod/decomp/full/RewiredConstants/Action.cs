using Rewired.Dev;

namespace RewiredConstants;

public static class Action
{
	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "Attack")]
	public const int Attack = 1;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "Jump")]
	public const int Jump = 2;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "MoveHorizontal")]
	public const int MoveHorizontal = 3;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "MoveVertical")]
	public const int MoveVertical = 8;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "Dash")]
	public const int Dash = 4;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "Spell")]
	public const int Spell = 5;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "Talent")]
	public const int Talent = 16;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "DownStrike")]
	public const int DownStrike = 6;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "Interact")]
	public const int Interact = 10;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "MoveLeft")]
	public const int MoveLeft = 12;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "MoveRight")]
	public const int MoveRight = 13;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "MoveUp")]
	public const int MoveUp = 14;

	[ActionIdFieldInfo(categoryName = "PlayerActions", friendlyName = "MoveDown")]
	public const int MoveDown = 15;

	[ActionIdFieldInfo(categoryName = "Menu", friendlyName = "Confirm")]
	public const int Confirm = 23;

	[ActionIdFieldInfo(categoryName = "Menu", friendlyName = "Cancel")]
	public const int Cancel = 24;

	[ActionIdFieldInfo(categoryName = "Menu", friendlyName = "MenuLeft")]
	public const int MenuLeft = 25;

	[ActionIdFieldInfo(categoryName = "Menu", friendlyName = "MenuRight")]
	public const int MenuRight = 26;

	[ActionIdFieldInfo(categoryName = "Menu", friendlyName = "MenuUp")]
	public const int MenuUp = 27;

	[ActionIdFieldInfo(categoryName = "Menu", friendlyName = "MenuDown")]
	public const int MenuDown = 28;

	[ActionIdFieldInfo(categoryName = "Menu", friendlyName = "Start")]
	public const int Start = 29;

	[ActionIdFieldInfo(categoryName = "Menu", friendlyName = "Select")]
	public const int Select = 30;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "Action0")]
	public const int UIHorizontal = 33;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UIVertical")]
	public const int UIVertical = 34;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UICancel")]
	public const int UISubmit = 35;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UICancel")]
	public const int UICancel = 36;

	[ActionIdFieldInfo(categoryName = "UI", friendlyName = "UIUpgrade")]
	public const int UIUpgrade = 37;

	[ActionIdFieldInfo(categoryName = "Pause Window", friendlyName = "Pause Window Start")]
	public const int Pause_Window_Start = 38;

	[ActionIdFieldInfo(categoryName = "Pause Window", friendlyName = "Pause Window Select")]
	public const int Pause_Window_Select = 39;
}
