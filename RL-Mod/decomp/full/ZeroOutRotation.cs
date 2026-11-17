using System.Collections;
using UnityEngine;

public class ZeroOutRotation : MonoBehaviour
{
	[SerializeField]
	private bool m_counterNonUniformScaling;

	private IEnumerator Start()
	{
		Prop prop = GetComponentInParent<Prop>();
		if ((bool)prop)
		{
			while (!prop.IsInitialized)
			{
				yield return null;
			}
		}
		ZeroOut();
		if (m_counterNonUniformScaling)
		{
			AverageNonUniformScaling();
		}
	}

	public void ZeroOut()
	{
		base.gameObject.transform.eulerAngles = Vector3.zero;
		if ((base.transform.lossyScale.x < 0f && base.transform.localScale.x > 0f) || (base.transform.lossyScale.x > 0f && base.transform.localScale.x < 0f))
		{
			Vector3 localScale = base.transform.localScale;
			localScale.x = 0f - localScale.x;
			base.transform.localScale = localScale;
		}
	}

	public void AverageNonUniformScaling()
	{
		Transform parent = base.transform.parent;
		base.transform.SetParent(null);
		float num = (base.transform.localScale.x + base.transform.localScale.y) / 2f;
		base.transform.localScale = new Vector3(num, num, base.transform.localScale.z);
		base.transform.SetParent(parent);
	}
}
