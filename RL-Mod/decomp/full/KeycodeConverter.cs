using Rewired;

public class KeycodeConverter
{
	public static string GetKeycodeStringName(KeyboardKeyCode key, bool shift, bool capsLock, bool numLock)
	{
		switch (key)
		{
		case KeyboardKeyCode.A:
			return TranslateAlphabetic('a', shift, capsLock);
		case KeyboardKeyCode.B:
			return TranslateAlphabetic('b', shift, capsLock);
		case KeyboardKeyCode.C:
			return TranslateAlphabetic('c', shift, capsLock);
		case KeyboardKeyCode.D:
			return TranslateAlphabetic('d', shift, capsLock);
		case KeyboardKeyCode.E:
			return TranslateAlphabetic('e', shift, capsLock);
		case KeyboardKeyCode.F:
			return TranslateAlphabetic('f', shift, capsLock);
		case KeyboardKeyCode.G:
			return TranslateAlphabetic('g', shift, capsLock);
		case KeyboardKeyCode.H:
			return TranslateAlphabetic('h', shift, capsLock);
		case KeyboardKeyCode.I:
			return TranslateAlphabetic('i', shift, capsLock);
		case KeyboardKeyCode.J:
			return TranslateAlphabetic('j', shift, capsLock);
		case KeyboardKeyCode.K:
			return TranslateAlphabetic('k', shift, capsLock);
		case KeyboardKeyCode.L:
			return TranslateAlphabetic('l', shift, capsLock);
		case KeyboardKeyCode.M:
			return TranslateAlphabetic('m', shift, capsLock);
		case KeyboardKeyCode.N:
			return TranslateAlphabetic('n', shift, capsLock);
		case KeyboardKeyCode.O:
			return TranslateAlphabetic('o', shift, capsLock);
		case KeyboardKeyCode.P:
			return TranslateAlphabetic('p', shift, capsLock);
		case KeyboardKeyCode.Q:
			return TranslateAlphabetic('q', shift, capsLock);
		case KeyboardKeyCode.R:
			return TranslateAlphabetic('r', shift, capsLock);
		case KeyboardKeyCode.S:
			return TranslateAlphabetic('s', shift, capsLock);
		case KeyboardKeyCode.T:
			return TranslateAlphabetic('t', shift, capsLock);
		case KeyboardKeyCode.U:
			return TranslateAlphabetic('u', shift, capsLock);
		case KeyboardKeyCode.V:
			return TranslateAlphabetic('v', shift, capsLock);
		case KeyboardKeyCode.W:
			return TranslateAlphabetic('w', shift, capsLock);
		case KeyboardKeyCode.X:
			return TranslateAlphabetic('x', shift, capsLock);
		case KeyboardKeyCode.Y:
			return TranslateAlphabetic('y', shift, capsLock);
		case KeyboardKeyCode.Z:
			return TranslateAlphabetic('z', shift, capsLock);
		case KeyboardKeyCode.F1:
			return "F1";
		case KeyboardKeyCode.F2:
			return "F2";
		case KeyboardKeyCode.F3:
			return "F3";
		case KeyboardKeyCode.F4:
			return "F4";
		case KeyboardKeyCode.F5:
			return "F5";
		case KeyboardKeyCode.F6:
			return "F6";
		case KeyboardKeyCode.F7:
			return "F7";
		case KeyboardKeyCode.F8:
			return "F8";
		case KeyboardKeyCode.F9:
			return "F9";
		case KeyboardKeyCode.F10:
			return "F10";
		case KeyboardKeyCode.F11:
			return "F11";
		case KeyboardKeyCode.F12:
			return "F12";
		case KeyboardKeyCode.Alpha0:
			if (!shift)
			{
				return "0";
			}
			return ")";
		case KeyboardKeyCode.Alpha1:
			if (!shift)
			{
				return "1";
			}
			return "!";
		case KeyboardKeyCode.Alpha2:
			if (!shift)
			{
				return "2";
			}
			return "@";
		case KeyboardKeyCode.Alpha3:
			if (!shift)
			{
				return "3";
			}
			return "#";
		case KeyboardKeyCode.Alpha4:
			if (!shift)
			{
				return "4";
			}
			return "$";
		case KeyboardKeyCode.Alpha5:
			if (!shift)
			{
				return "5";
			}
			return "%";
		case KeyboardKeyCode.Alpha6:
			if (!shift)
			{
				return "6";
			}
			return "^";
		case KeyboardKeyCode.Alpha7:
			if (!shift)
			{
				return "7";
			}
			return "&";
		case KeyboardKeyCode.Alpha8:
			if (!shift)
			{
				return "8";
			}
			return "*";
		case KeyboardKeyCode.Alpha9:
			if (!shift)
			{
				return "9";
			}
			return "(";
		case KeyboardKeyCode.KeypadPlus:
			return "+";
		case KeyboardKeyCode.KeypadDivide:
			return "/";
		case KeyboardKeyCode.KeypadMultiply:
			return "*";
		case KeyboardKeyCode.KeypadMinus:
			return "-";
		case KeyboardKeyCode.Space:
			return "Space";
		case KeyboardKeyCode.Tab:
			return "Tab";
		case KeyboardKeyCode.Return:
		case KeyboardKeyCode.KeypadEnter:
			return "Enter";
		case KeyboardKeyCode.Backspace:
			return "Back";
		case KeyboardKeyCode.LeftControl:
			return "L-Ctrl";
		case KeyboardKeyCode.RightControl:
			return "R-Ctrl";
		case KeyboardKeyCode.LeftAlt:
			return "L-Alt";
		case KeyboardKeyCode.RightAlt:
			return "R-Alt";
		case KeyboardKeyCode.LeftShift:
			return "LShift";
		case KeyboardKeyCode.RightShift:
			return "RShift";
		case KeyboardKeyCode.CapsLock:
			return "Caps";
		case KeyboardKeyCode.Menu:
			return "Menu";
		case KeyboardKeyCode.RightCommand:
			return "R-Cmd";
		case KeyboardKeyCode.LeftCommand:
			return "L-Cmd";
		case KeyboardKeyCode.DownArrow:
			return "";
		case KeyboardKeyCode.LeftArrow:
			return "";
		case KeyboardKeyCode.RightArrow:
			return "";
		case KeyboardKeyCode.UpArrow:
			return "";
		case KeyboardKeyCode.Escape:
			return "Esc";
		case KeyboardKeyCode.KeypadPeriod:
			if (numLock && !shift)
			{
				return ".";
			}
			break;
		case KeyboardKeyCode.Keypad0:
			return "NUM 0";
		case KeyboardKeyCode.Keypad1:
			return "NUM 1";
		case KeyboardKeyCode.Keypad2:
			return "NUM 2";
		case KeyboardKeyCode.Keypad3:
			return "NUM 3";
		case KeyboardKeyCode.Keypad4:
			return "NUM 4";
		case KeyboardKeyCode.Keypad5:
			return "NUM 5";
		case KeyboardKeyCode.Keypad6:
			return "NUM 6";
		case KeyboardKeyCode.Keypad7:
			return "NUM 7";
		case KeyboardKeyCode.Keypad8:
			return "NUM 8";
		case KeyboardKeyCode.Keypad9:
			return "NUM 9";
		case KeyboardKeyCode.Backslash:
			if (!shift)
			{
				return "\\";
			}
			return "|";
		case KeyboardKeyCode.RightBracket:
			if (!shift)
			{
				return "]";
			}
			return "}";
		case KeyboardKeyCode.Comma:
			if (!shift)
			{
				return ",";
			}
			return "<";
		case KeyboardKeyCode.Minus:
			if (!shift)
			{
				return "-";
			}
			return "_";
		case KeyboardKeyCode.LeftBracket:
			if (!shift)
			{
				return "[";
			}
			return "{";
		case KeyboardKeyCode.Period:
			if (!shift)
			{
				return ".";
			}
			return ">";
		case KeyboardKeyCode.Plus:
			if (!shift)
			{
				return "=";
			}
			return "+";
		case KeyboardKeyCode.Question:
			if (!shift)
			{
				return "/";
			}
			return "?";
		case KeyboardKeyCode.Quote:
			if (!shift)
			{
				return "\"";
			}
			return "'";
		case KeyboardKeyCode.Semicolon:
			if (!shift)
			{
				return ";";
			}
			return ":";
		case KeyboardKeyCode.BackQuote:
			if (!shift)
			{
				return "`";
			}
			return "~";
		case KeyboardKeyCode.Equals:
			if (!shift)
			{
				return "=";
			}
			return "+";
		}
		return "";
	}

	public static string TranslateAlphabetic(char baseChar, bool shift, bool capsLock)
	{
		return ((capsLock ^ shift) ? char.ToUpper(baseChar) : baseChar).ToString();
	}
}
