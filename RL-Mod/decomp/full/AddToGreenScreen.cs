using UnityEngine;

public class AddToGreenScreen : MonoBehaviour
{
	private int m_storedLayer;

	private void Awake()
	{
		m_storedLayer = base.gameObject.layer;
	}

	public void ResetLayers()
	{
		base.gameObject.SetLayerRecursively(m_storedLayer);
	}
}
