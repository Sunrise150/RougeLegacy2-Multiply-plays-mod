using UnityEngine;

public class InvisibleCeiling : MonoBehaviour, IMirror
{
	[SerializeField]
	private MeshRenderer m_meshRenderer;

	private void Awake()
	{
		m_meshRenderer.enabled = false;
	}

	public void Mirror()
	{
		if (base.transform.right != Vector3.right)
		{
			base.transform.right = Vector3.Reflect(base.transform.right, Vector3.up);
		}
	}
}
