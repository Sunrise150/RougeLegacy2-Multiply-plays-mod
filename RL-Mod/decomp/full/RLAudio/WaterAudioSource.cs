using System.Collections;
using UnityEngine;

namespace RLAudio;

public class WaterAudioSource : MonoBehaviour
{
	[SerializeField]
	private bool m_isChoppy;

	public bool IsChoppy => m_isChoppy;

	public float WaterLevel { get; private set; }

	private void OnEnable()
	{
		StartCoroutine(AddWaterSourceCoroutine(GetComponent<Prop>()));
	}

	private IEnumerator AddWaterSourceCoroutine(Prop prop)
	{
		if ((bool)prop)
		{
			while (!prop.IsInitialized)
			{
				yield return null;
			}
			WaterLevel = base.transform.position.y;
			WaterAudioSourceManager.AddWaterSource(this);
		}
	}

	private void OnDisable()
	{
		WaterAudioSourceManager.RemoveWaterSource(this);
	}
}
