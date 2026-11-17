using System;
using UnityEngine;

public class RoomLetterboxController : MonoBehaviour
{
	private enum LetterboxSide
	{
		Left,
		Right,
		Top,
		Bottom
	}

	[SerializeField]
	private GameObject m_letterboxPrefab;

	private GameObject[] m_letterBoxArray;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private void Awake()
	{
		m_letterBoxArray = new GameObject[4];
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		Initialize();
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnDisable()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void Initialize()
	{
		for (int i = 0; i < m_letterBoxArray.Length; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_letterboxPrefab);
			gameObject.transform.SetParent(base.transform);
			m_letterBoxArray[i] = gameObject;
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		RoomViaDoorEventArgs roomViaDoorEventArgs = args as RoomViaDoorEventArgs;
		if (m_letterBoxArray[0] == null)
		{
			Initialize();
		}
		if (roomViaDoorEventArgs == null)
		{
			return;
		}
		if (!roomViaDoorEventArgs.Room.DisableRoomLetterBoxing)
		{
			InitializeLetterbox(LetterboxSide.Left, roomViaDoorEventArgs.Room);
			InitializeLetterbox(LetterboxSide.Right, roomViaDoorEventArgs.Room);
			InitializeLetterbox(LetterboxSide.Top, roomViaDoorEventArgs.Room);
			InitializeLetterbox(LetterboxSide.Bottom, roomViaDoorEventArgs.Room);
		}
		else
		{
			GameObject[] letterBoxArray = m_letterBoxArray;
			for (int i = 0; i < letterBoxArray.Length; i++)
			{
				letterBoxArray[i].SetActive(value: false);
			}
		}
	}

	private void InitializeLetterbox(LetterboxSide side, BaseRoom room)
	{
		GameObject gameObject = m_letterBoxArray[(int)side];
		gameObject.transform.SetParent(room.gameObject.transform);
		Bounds bounds = room.Collider2D.bounds;
		Vector3 localScale = default(Vector3);
		Vector3 position = default(Vector3);
		float num = 10f;
		switch (side)
		{
		case LetterboxSide.Left:
		case LetterboxSide.Right:
			localScale.x = num;
			localScale.y = (bounds.size.y + 2f) / 2f;
			position.x = ((side == LetterboxSide.Left) ? (bounds.min.x - num) : (bounds.max.x + num));
			position.y = bounds.center.y;
			break;
		case LetterboxSide.Top:
		case LetterboxSide.Bottom:
			localScale.x = (bounds.size.x + 2f) / 2f;
			localScale.y = num;
			position.x = bounds.center.x;
			position.y = ((side == LetterboxSide.Top) ? (bounds.max.y + num) : (bounds.min.y - num));
			break;
		}
		localScale.z = 1f;
		position.z = 1f;
		gameObject.transform.localScale = localScale;
		gameObject.transform.position = position;
		if (bounds.size.x == 32f)
		{
			gameObject.layer = 5;
		}
		else
		{
			gameObject.layer = 23;
		}
		gameObject.SetActive(value: true);
	}
}
