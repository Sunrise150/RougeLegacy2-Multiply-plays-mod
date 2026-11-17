using System.Collections.Generic;
using UnityEngine;

[SharedBetweenAnimators]
public class EffectTriggerAnimBehaviour : StateMachineBehaviour
{
	private static Dictionary<int, int> m_normalizedStartCounterDict = new Dictionary<int, int>();

	[SerializeField]
	private EffectTriggerEntry[] m_effectTriggerEntryList = new EffectTriggerEntry[0];

	[SerializeField]
	private AnimBehaviourCondition[] m_animBehaviourConditionArray = new AnimBehaviourCondition[0];

	public static bool DISABLE_GLOBALLY { get; set; } = false;


	public EffectTriggerEntry[] TriggerArray
	{
		get
		{
			return m_effectTriggerEntryList;
		}
		set
		{
			m_effectTriggerEntryList = value;
		}
	}

	public AnimBehaviourCondition[] ConditionArray
	{
		get
		{
			return m_animBehaviourConditionArray;
		}
		set
		{
			m_animBehaviourConditionArray = value;
		}
	}

	public static void ClearNormalizedStartCounters()
	{
		m_normalizedStartCounterDict.Clear();
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (DISABLE_GLOBALLY || EffectManager.AnimatorEffectsDisabled(animator))
		{
			return;
		}
		if (m_animBehaviourConditionArray == null)
		{
			Debug.Log("AnimBehaviourConditionArray null on animator: " + animator.name + " StateInfoHash: " + stateInfo.shortNameHash);
			string text = "";
			EffectTriggerEntry[] effectTriggerEntryList = m_effectTriggerEntryList;
			foreach (EffectTriggerEntry effectTriggerEntry in effectTriggerEntryList)
			{
				text = text + effectTriggerEntry.EffectTypeName + ",  ";
			}
			Debug.Log("Effects: " + text);
			return;
		}
		for (int j = 0; j < m_animBehaviourConditionArray.Length; j++)
		{
			if (!m_animBehaviourConditionArray[j].IsTrue(animator))
			{
				return;
			}
		}
		for (int k = 0; k < m_effectTriggerEntryList.Length; k++)
		{
			EffectTriggerEntry effectTriggerEntry2 = m_effectTriggerEntryList[k];
			if (effectTriggerEntry2.EffectStartType == EffectStartType.FirstLoopOnly && effectTriggerEntry2.NormalizedStartTime <= 0f)
			{
				effectTriggerEntry2.EffectStartType = EffectStartType.None;
			}
			if (effectTriggerEntry2.EffectStartType == EffectStartType.None)
			{
				InvokeTrigger(animator.gameObject, animator, effectTriggerEntry2, layerIndex);
				continue;
			}
			int instanceID = animator.GetInstanceID();
			if (!m_normalizedStartCounterDict.ContainsKey(instanceID))
			{
				m_normalizedStartCounterDict.Add(instanceID, 0);
			}
			else
			{
				m_normalizedStartCounterDict[instanceID] = 0;
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (DISABLE_GLOBALLY || EffectManager.AnimatorEffectsDisabled(animator))
		{
			return;
		}
		if (m_animBehaviourConditionArray == null)
		{
			Debug.Log("AnimBehaviourConditionArray null on animator: " + animator.name + " StateInfoHash: " + stateInfo.shortNameHash);
			string text = "";
			EffectTriggerEntry[] effectTriggerEntryList = m_effectTriggerEntryList;
			foreach (EffectTriggerEntry effectTriggerEntry in effectTriggerEntryList)
			{
				text = text + effectTriggerEntry.EffectTypeName + ",  ";
			}
			Debug.Log("Effects: " + text);
			return;
		}
		for (int j = 0; j < m_animBehaviourConditionArray.Length; j++)
		{
			if (!m_animBehaviourConditionArray[j].IsTrue(animator))
			{
				return;
			}
		}
		for (int k = 0; k < m_effectTriggerEntryList.Length; k++)
		{
			EffectTriggerEntry effectTriggerEntry2 = m_effectTriggerEntryList[k];
			switch (effectTriggerEntry2.EffectStartType)
			{
			case EffectStartType.FirstLoopOnly:
			{
				int instanceID2 = animator.GetInstanceID();
				if (!m_normalizedStartCounterDict.ContainsKey(instanceID2))
				{
					m_normalizedStartCounterDict.Add(instanceID2, 0);
				}
				if (stateInfo.normalizedTime >= effectTriggerEntry2.NormalizedStartTime && m_normalizedStartCounterDict[instanceID2] <= 0)
				{
					m_normalizedStartCounterDict[instanceID2]++;
					InvokeTrigger(animator.gameObject, animator, effectTriggerEntry2, layerIndex);
				}
				break;
			}
			case EffectStartType.OnceEveryLoop:
			{
				int instanceID = animator.GetInstanceID();
				if (!m_normalizedStartCounterDict.ContainsKey(instanceID))
				{
					m_normalizedStartCounterDict.Add(instanceID, 0);
				}
				if (stateInfo.normalizedTime >= (float)m_normalizedStartCounterDict[instanceID] + effectTriggerEntry2.NormalizedStartTime)
				{
					m_normalizedStartCounterDict[instanceID]++;
					InvokeTrigger(animator.gameObject, animator, effectTriggerEntry2, layerIndex);
				}
				break;
			}
			}
		}
	}

	private void InvokeTrigger(GameObject parentObj, Animator animator, EffectTriggerEntry entry, int layerIndex)
	{
		Vector3 vector = parentObj.transform.position;
		IMidpointObj component = parentObj.GetComponent<IMidpointObj>();
		if (component != null)
		{
			vector = component.Midpoint;
		}
		EffectTriggerDirection effectDirectionFromObject = EffectTrigger.GetEffectDirectionFromObject(parentObj, entry.TriggerDirection);
		EffectTrigger.InvokeTrigger(entry, parentObj, parentObj, vector, vector, effectDirectionFromObject, animator);
	}
}
