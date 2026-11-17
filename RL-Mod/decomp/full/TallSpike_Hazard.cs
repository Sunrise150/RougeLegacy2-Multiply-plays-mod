using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TallSpike_Hazard : Hazard, IFerr2DHazard, IHazard, IRootObj, IMirror
{
	private bool m_flipColliders;

	private bool m_collidersFlipped;

	private IHitboxController m_hbController;

	public Ferr2DT_PathTerrain Ferr2D { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		m_hbController = GetComponentInChildren<IHitboxController>();
	}

	private void Start()
	{
		if (base.transform.localEulerAngles.z != 0f || base.transform.localScale.x != 1f || base.transform.localScale.y != 1f)
		{
			BaseRoom componentInParent = GetComponentInParent<BaseRoom>();
			string text = ((componentInParent != null) ? componentInParent.gameObject.name : "NULL ROOM");
			Debug.LogFormat("<color=red>TallSpike Hazard {0} was not created correctly in room {1}.  Check transform values.</color>", this.GetRoot().name, text);
		}
		if (Application.isPlaying)
		{
			CameraLayerController component = GetComponent<CameraLayerController>();
			Renderer component2 = GetComponent<Renderer>();
			component2.sortingLayerID = CameraLayerUtility.GetLayer(component.CameraLayer);
			component2.sortingOrder = CameraLayerUtility.GetSpriteOrderInLayer(component.CameraLayer, component.SubLayer);
		}
	}

	public override void Initialize(HazardArgs hazardArgs)
	{
		if (hazardArgs is Ferr2DHazardArgs ferr2DHazardArgs)
		{
			if (m_hbController == null)
			{
				throw new MissingComponentException("HitboxControllerLite");
			}
			Ferr2D = GetComponent<Ferr2DT_PathTerrain>();
			Ferr2D.transform.localPosition = ferr2DHazardArgs.LocalPosition;
			Ferr2D.ClearPoints();
			for (int i = 0; i < ferr2DHazardArgs.Points.Count; i++)
			{
				Ferr2D.AddPoint(ferr2DHazardArgs.Points[i]);
			}
			Ferr2D.Build();
			if (Time.inFixedTimeStep)
			{
				Invoke("InitializeHitboxes", 0f);
			}
			else
			{
				InitializeHitboxes();
			}
		}
	}

	private void InitializeHitboxes()
	{
		m_hbController.Initialize();
	}

	public override void ResetHazard()
	{
	}

	private void OnEnable()
	{
		if (m_flipColliders && !m_collidersFlipped)
		{
			StartCoroutine(MirrorTerrain());
		}
	}

	public void Mirror()
	{
		if (!m_hbController.IsInitialized)
		{
			m_flipColliders = true;
		}
	}

	private IEnumerator MirrorTerrain()
	{
		yield return new WaitUntil(() => m_hbController.IsInitialized);
		Collider2D collider = m_hbController.GetCollider(HitboxType.Terrain);
		Collider2D collider2 = m_hbController.GetCollider(HitboxType.Platform);
		Collider2D component = GetComponent<PolygonCollider2D>();
		if (!(collider2 is PolygonCollider2D))
		{
			collider2.offset = new Vector2(0f - collider2.offset.x, collider2.offset.y);
			if (component != null)
			{
				Object.Destroy(component);
			}
		}
		if (collider != null)
		{
			CDGHelper.CopyComponent(collider2, collider.gameObject);
		}
		m_collidersFlipped = true;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
