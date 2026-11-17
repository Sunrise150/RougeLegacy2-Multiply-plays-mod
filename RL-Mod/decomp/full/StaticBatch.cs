using UnityEngine;

public class StaticBatch : MonoBehaviour
{
	private void Awake()
	{
		StaticBatchingUtility.Combine(base.gameObject);
	}
}
