using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class DecoLocation
{
	[SerializeField]
	[Range(0f, 1f)]
	private float m_chanceEmpty;

	[SerializeField]
	private Vector2 m_position;

	[SerializeField]
	private DecoEntry[] m_potentialDecos;

	private Vector2 m_parentPositionOffset;

	public float ChanceEmpty => m_chanceEmpty;

	public DecoController DecoController { get; private set; }

	public Deco DecoInstance { get; set; }

	public Deco DecoPrefab { get; set; }

	public int Index { get; set; }

	public bool IsFlipped { get; set; }

	public Vector2 Position => m_position;

	public DecoEntry[] PotentialDecos => m_potentialDecos;

	public void Initialize(DecoController decoController, Deco decoPrefab, bool isFlipped)
	{
		DecoController = decoController;
		DecoPrefab = decoPrefab;
		if ((bool)DecoPrefab)
		{
			IsFlipped = isFlipped;
			SetDecoPropInstance();
		}
	}

	public void ReturnDecoToPool()
	{
		if ((bool)DecoInstance)
		{
			DecoInstance.gameObject.SetActive(value: false);
			DecoInstance.gameObject.transform.SetParent(null);
			DecoInstance = null;
		}
	}

	private void SetDecoPropInstance()
	{
		if ((bool)DecoPrefab)
		{
			m_parentPositionOffset = Vector2.zero;
			DecoInstance = DecoManager.GetDeco(DecoPrefab.NameHash);
			if ((bool)DecoInstance)
			{
				DecoInstance.gameObject.SetActive(value: true);
				Prop[] props = DecoInstance.Props;
				for (int i = 0; i < props.Length; i++)
				{
					props[i].SetRoom(DecoController.OwnerProp.Room);
				}
				Transform transform = (DecoController.OwnerProp.CameraLayerController as CameraLayerController).Visuals.transform;
				Vector2 vector = Position;
				if (DecoController.OwnerProp.Pivot.transform.localEulerAngles.z != 0f)
				{
					vector = CDGHelper.RotatedPoint(Position, Vector2.zero, DecoController.OwnerProp.Pivot.transform.localEulerAngles.z);
				}
				DecoInstance.gameObject.transform.localPosition = (Vector3)(vector * transform.transform.lossyScale) + transform.position;
				DecoInstance.SetCameraLayerControllers(DecoController.OwnerProp);
				Vector3 localScale = DecoInstance.gameObject.transform.localScale;
				if (IsFlipped)
				{
					localScale.x = Mathf.Abs(localScale.x) * -1f;
				}
				else
				{
					localScale.x = Mathf.Abs(localScale.x);
				}
				localScale = Vector3.Scale(localScale, transform.transform.lossyScale);
				DecoInstance.gameObject.transform.localScale = localScale;
				DecoInstance.gameObject.transform.rotation = DecoController.OwnerProp.Pivot.transform.rotation;
				m_parentPositionOffset = DecoInstance.transform.localPosition - transform.position;
				if ((bool)DecoController.OwnerProp.CorgiController)
				{
					DecoController.StartCoroutine(FollowParentPositionCoroutine());
				}
			}
		}
		else
		{
			DecoPrefab = null;
			DecoInstance = null;
		}
	}

	private IEnumerator AttachToParentCoroutine(Transform parent)
	{
		if (DecoController.OwnerProp.HitboxController != null)
		{
			while (!DecoController.OwnerProp.HitboxController.ResponseMethodsInitialized)
			{
				yield return null;
			}
		}
		if (!DecoInstance)
		{
			yield break;
		}
		bool hbInitSuccessful = true;
		Prop[] props = DecoInstance.Props;
		foreach (Prop prop in props)
		{
			if (prop.HitboxController == null)
			{
				continue;
			}
			while (!prop.HitboxController.ResponseMethodsInitialized)
			{
				if (!prop.gameObject.activeSelf)
				{
					yield break;
				}
				yield return null;
			}
		}
		if (hbInitSuccessful)
		{
			DecoInstance.gameObject.transform.SetParent(parent, worldPositionStays: true);
		}
	}

	private IEnumerator FollowParentPositionCoroutine()
	{
		while ((bool)DecoController.OwnerProp && !DecoInstance.IsNativeNull())
		{
			Vector3 localPosition = DecoController.OwnerProp.transform.position + (Vector3)m_parentPositionOffset;
			localPosition.z = DecoInstance.transform.localPosition.z;
			DecoInstance.transform.localPosition = localPosition;
			yield return null;
		}
	}

	public void SetDecoController(DecoController decoController)
	{
		DecoController = decoController;
	}
}
