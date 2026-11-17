using UnityEngine;

public class RoomBackgroundOverride : MonoBehaviour
{
	[SerializeField]
	private Background m_backgroundOverride;

	[SerializeField]
	private bool m_isTiled;

	public Background BackgroundOverride => m_backgroundOverride;

	public bool IsTiled => m_isTiled;
}
