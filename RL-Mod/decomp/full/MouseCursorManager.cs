using UnityEngine;

public class MouseCursorManager : MonoBehaviour
{
	private bool m_cursorVisible = true;

	private static MouseCursorManager m_cursorManager;

	private static MouseCursorManager Instance => m_cursorManager;

	public static bool IsInitialized { get; private set; }

	public static bool IsCursorVisible => Instance.m_cursorVisible;

	public static void SetCursorVisible(bool visible)
	{
		if (Instance.m_cursorVisible != visible)
		{
			Instance.m_cursorVisible = visible;
			Cursor.visible = visible;
		}
	}

	public static void SetCursorType(CursorIconType cursorType)
	{
		Cursor.SetCursor(IconLibrary.GetCursorIconTexture(cursorType), Vector2.zero, CursorMode.Auto);
	}

	private void Awake()
	{
		if (m_cursorManager == null)
		{
			m_cursorManager = this;
			Initialize();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Initialize()
	{
		SetCursorType(CursorIconType.Standard);
		SetCursorVisible(visible: false);
		IsInitialized = true;
	}
}
