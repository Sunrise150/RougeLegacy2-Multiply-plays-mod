using System.Runtime.CompilerServices;
using UnityEngine;

public class Background : MonoBehaviour, IGenericPoolObj
{
	[SerializeField]
	private Vector2Int m_size = Vector2Int.one;

	[SerializeField]
	private bool m_isTileable = true;

	public bool IsFreePoolObj { get; set; }

	public bool IsAwakeCalled => true;

	public Vector2Int Size
	{
		get
		{
			return m_size;
		}
		private set
		{
			m_size = value;
		}
	}

	public bool IsTileable => m_isTileable;

	private void OnValidate()
	{
		if (Size.x < 1 || Size.y < 1)
		{
			Size = Vector2Int.one;
		}
	}

	private void Start()
	{
		CameraLayerController component = GetComponent<CameraLayerController>();
		if ((bool)component)
		{
			component.SetLayerAndSubLayer();
		}
	}

	public void ResetValues()
	{
	}

	private void OnDisable()
	{
		DisablePooledObjectManager.DisablePooledObject(this);
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
