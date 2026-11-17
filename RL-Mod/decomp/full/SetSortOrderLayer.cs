using UnityEngine;

public class SetSortOrderLayer : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer m_meshRenderer;

	[SerializeField]
	private string m_layerName = "";

	private void Start()
	{
		m_meshRenderer.sortingLayerName = m_layerName;
	}
}
