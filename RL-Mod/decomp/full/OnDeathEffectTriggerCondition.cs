using UnityEngine;

public class OnDeathEffectTriggerCondition : MonoBehaviour
{
	[SerializeField]
	public OnDeathEffectTrigger[] OnDeathCollidedEffectArray;

	[SerializeField]
	public OnDeathEffectTrigger[] OnDeathAttackedEffectArray;

	private IEffectTriggerEvent_OnDeath m_onDeathEffectObj;

	private void Awake()
	{
		m_onDeathEffectObj = this.GetRoot().GetComponentInChildren<IEffectTriggerEvent_OnDeath>();
	}

	private void Start()
	{
		m_onDeathEffectObj.OnDeathEffectTriggerRelay.RemoveAll();
		m_onDeathEffectObj.OnDeathEffectTriggerRelay.AddListener(TriggerConditionalEffect);
	}

	private void TriggerConditionalEffect(GameObject attacker)
	{
		if (attacker != null && attacker.CompareTag("PlayerProjectile"))
		{
			OnDeathEffectTrigger[] onDeathAttackedEffectArray = OnDeathAttackedEffectArray;
			for (int i = 0; i < onDeathAttackedEffectArray.Length; i++)
			{
				onDeathAttackedEffectArray[i].InvokeOnDeathTrigger(attacker);
			}
		}
		else
		{
			OnDeathEffectTrigger[] onDeathAttackedEffectArray = OnDeathCollidedEffectArray;
			for (int i = 0; i < onDeathAttackedEffectArray.Length; i++)
			{
				onDeathAttackedEffectArray[i].InvokeOnDeathTrigger(attacker);
			}
		}
	}

	private void OnDestroy()
	{
		if (m_onDeathEffectObj != null)
		{
			m_onDeathEffectObj.OnDeathEffectTriggerRelay.RemoveListener(TriggerConditionalEffect);
		}
	}
}
