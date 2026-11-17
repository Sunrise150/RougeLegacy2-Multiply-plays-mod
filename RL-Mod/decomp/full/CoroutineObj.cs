using System.Collections;
using UnityEngine;

public class CoroutineObj : MonoBehaviour
{
	protected Coroutine m_coroutine;

	public Object BaseObject { get; set; }

	public virtual void Initialize()
	{
	}

	public virtual void StartLogic()
	{
		m_coroutine = StartCoroutine(Execute());
	}

	protected virtual IEnumerator Execute()
	{
		yield break;
	}

	public virtual void StopLogic()
	{
		if (m_coroutine != null)
		{
			StopCoroutine(m_coroutine);
		}
	}
}
