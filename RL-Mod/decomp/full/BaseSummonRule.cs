using System.Collections;
using UnityEngine;

public abstract class BaseSummonRule
{
	public SummonRuleController SummonController { get; private set; }

	public abstract string RuleLabel { get; }

	public abstract SummonRuleType RuleType { get; }

	public virtual Color BoxColor { get; } = Color.white;


	public bool IsRuleComplete { get; protected set; }

	public Object SerializedObject { get; protected set; }

	public abstract IEnumerator RunSummonRule();

	public virtual void OnEnable()
	{
	}

	public virtual void OnDisable()
	{
	}

	public void SetSerializedObject(Object serializedObj)
	{
		SerializedObject = serializedObj;
	}

	public virtual void Initialize(SummonRuleController summonController)
	{
		SummonController = summonController;
	}

	public void ResetRule()
	{
		IsRuleComplete = false;
	}
}
