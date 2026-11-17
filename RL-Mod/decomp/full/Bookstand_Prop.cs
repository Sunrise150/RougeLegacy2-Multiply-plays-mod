using System.Collections.Generic;
using UnityEngine;

public class Bookstand_Prop : Prop
{
	[SerializeField]
	private SpriteRenderer m_legs;

	private const int MAX_RAYCAST_DISTANCE = 18;

	private List<RaycastHit2D> m_hitInfo = new List<RaycastHit2D>(2);

	public override void Initialize(BaseRoom room, PropSpawnController propSpawnController)
	{
		base.Initialize(room, propSpawnController);
		base.transform.rotation = Quaternion.identity;
		float y = m_legs.bounds.max.y;
		Vector2 origin = new Vector2(base.transform.position.x, y);
		int mask = LayerMask.GetMask("Platform_CollidesWithAll", "Platform_CollidesWithEnemy", "Platform_CollidesWithPlayer", "Platform_OneWay");
		ContactFilter2D contactFilter = default(ContactFilter2D);
		contactFilter.SetLayerMask(mask);
		int num = Physics2D.Raycast(origin, Vector2.down, contactFilter, m_hitInfo, 18f);
		if (num <= 0)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			if (!m_hitInfo[i].collider.gameObject.CompareTag("Untagged"))
			{
				float distance = m_hitInfo[i].distance;
				float num2 = 1f;
				if (base.transform.lossyScale.y != 0f)
				{
					num2 = 1f / base.transform.lossyScale.y;
				}
				m_legs.size = new Vector2(m_legs.size.x, num2 * distance);
				break;
			}
		}
	}
}
