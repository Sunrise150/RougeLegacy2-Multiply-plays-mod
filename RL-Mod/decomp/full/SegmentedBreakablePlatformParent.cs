using System.Collections;
using UnityEngine;

public class SegmentedBreakablePlatformParent : SpecialPlatform, IRoomConsumer
{
	[SerializeField]
	private GameObject m_platformPrefab;

	private GameObject[] m_segments;

	public BaseRoom Room { get; private set; }

	protected override void Awake()
	{
	}

	protected override IEnumerator Start()
	{
		int num = (int)base.Width / 2;
		m_segments = new GameObject[num];
		float num2 = base.transform.position.x - base.Width / 2f;
		for (int i = 0; i < num; i++)
		{
			int num3 = ((i == num / 2 && base.Width % 2f != 0f) ? 3 : 2);
			float x = num2 + (float)num3 / 2f;
			Vector2 vector = new Vector2(x, base.transform.position.y);
			m_segments[i] = Object.Instantiate(m_platformPrefab, vector, Quaternion.identity, base.transform);
			SegmentedBreakablePlatform component = m_segments[i].GetComponent<SegmentedBreakablePlatform>();
			component.SetRoom(Room);
			component.Width = num3;
			num2 += (float)num3;
		}
		yield break;
	}

	public override void SetState(StateID state)
	{
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
	}
}
