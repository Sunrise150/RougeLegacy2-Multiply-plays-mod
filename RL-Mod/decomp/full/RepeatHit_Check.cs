using UnityEngine;

public class RepeatHit_Check : MonoBehaviour
{
	protected class HitObjectContainer
	{
		public GameObject ObjectHit;

		public float HitDuration;

		public bool JustAdded;

		public void Update()
		{
			if (!(ObjectHit == null))
			{
				HitDuration -= Time.deltaTime;
				if (HitDuration <= 0f)
				{
					ObjectHit = null;
				}
			}
		}
	}

	private const int REPEAT_HIT_ARRAY_SIZE = 10;

	[SerializeField]
	private float m_repeatHitDuration = 0.5f;

	protected HitObjectContainer[] m_hitObjectArray;

	public float RepeatHitDuration
	{
		get
		{
			return m_repeatHitDuration;
		}
		set
		{
			m_repeatHitDuration = value;
		}
	}

	private void Awake()
	{
		m_hitObjectArray = new HitObjectContainer[10];
		for (int i = 0; i < 10; i++)
		{
			m_hitObjectArray[i] = new HitObjectContainer();
		}
	}

	protected void Update()
	{
		HitObjectContainer[] hitObjectArray = m_hitObjectArray;
		for (int i = 0; i < hitObjectArray.Length; i++)
		{
			hitObjectArray[i].Update();
		}
	}

	protected void LateUpdate()
	{
		HitObjectContainer[] hitObjectArray = m_hitObjectArray;
		for (int i = 0; i < hitObjectArray.Length; i++)
		{
			hitObjectArray[i].JustAdded = false;
		}
	}

	public bool AllowCollision(GameObject objCollided)
	{
		objCollided = objCollided.transform.root.gameObject;
		int num = 0;
		for (int i = 0; i < 10; i++)
		{
			HitObjectContainer hitObjectContainer = m_hitObjectArray[i];
			if (hitObjectContainer.ObjectHit == null)
			{
				num = i;
			}
			if (hitObjectContainer.ObjectHit == objCollided && !hitObjectContainer.JustAdded)
			{
				return false;
			}
		}
		HitObjectContainer obj = m_hitObjectArray[num];
		obj.ObjectHit = objCollided;
		obj.JustAdded = true;
		obj.HitDuration = RepeatHitDuration;
		return true;
	}

	public void ClearCollisionChecks()
	{
		if (m_hitObjectArray != null)
		{
			HitObjectContainer[] hitObjectArray = m_hitObjectArray;
			foreach (HitObjectContainer obj in hitObjectArray)
			{
				obj.JustAdded = false;
				obj.HitDuration = 0f;
				obj.ObjectHit = null;
			}
		}
	}
}
