using System.Collections.Generic;
using UnityEngine;

public class Multi_Hazard : Hazard
{
	[SerializeField]
	private HazardType m_hazardType = HazardType.None;

	[SerializeField]
	private float m_rotationOffset;

	[SerializeField]
	private float m_positionOffset = 0.5f;

	protected List<IHazard> m_hazards = new List<IHazard>();

	public List<IHazard> Hazards => m_hazards;

	public override void Initialize(HazardArgs hazardArgs)
	{
		Debug.LogFormat("<color=red>| {0} | This method should not have been called. Let Paul know</color>", this);
	}

	public virtual void Initialize(PivotPoint pivot, int width, HazardArgs hazardArgs)
	{
		m_hazards.Clear();
		Vector3 right = base.transform.right;
		Vector3 position = base.transform.position;
		switch (pivot)
		{
		case PivotPoint.Right:
			right *= -1f;
			break;
		case PivotPoint.Center:
			position -= 0.5f * (float)width * base.transform.right;
			break;
		}
		for (int i = 0; i < width; i++)
		{
			IHazard hazard = HazardManager.GetHazard(m_hazardType);
			hazard.gameObject.SetActive(value: true);
			m_hazards.Add(hazard);
			if (hazard is IMultiHazardConsumer multiHazardConsumer)
			{
				multiHazardConsumer.MultiHazard = this;
			}
			m_hazards[i].gameObject.transform.rotation = base.transform.rotation;
			if (m_rotationOffset != 0f)
			{
				m_hazards[i].gameObject.transform.Rotate(new Vector3(0f, 0f, m_rotationOffset));
			}
			m_hazards[i].gameObject.transform.position = position + i * right + m_positionOffset * right;
			m_hazards[i].Initialize(hazardArgs);
			m_hazards[i].ResetHazard();
		}
		foreach (Hazard hazard2 in m_hazards)
		{
			hazard2.SetRoom(base.Room);
		}
	}

	public override void ResetHazard()
	{
		if (!Application.isEditor)
		{
			ResetHazards();
		}
	}

	private void ResetHazards()
	{
		if (!(base.gameObject != null) || m_hazards == null)
		{
			return;
		}
		foreach (Hazard hazard in m_hazards)
		{
			if (hazard != null)
			{
				hazard.ResetHazard();
			}
		}
	}

	public void Reset()
	{
		if (m_hazards == null)
		{
			return;
		}
		foreach (Hazard hazard in m_hazards)
		{
			if ((bool)hazard)
			{
				hazard.gameObject.SetActive(value: false);
			}
		}
	}

	public override void SetIsCulled(bool culled)
	{
		if (!this || m_hazards == null)
		{
			return;
		}
		foreach (Hazard hazard in m_hazards)
		{
			hazard.SetIsCulled(culled);
		}
	}
}
