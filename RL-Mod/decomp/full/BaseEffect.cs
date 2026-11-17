using System;
using System.Runtime.CompilerServices;
using Sigtrap.Relays;
using UnityEngine;

public abstract class BaseEffect : MonoBehaviour, IGenericPoolObj
{
	private Vector3 m_startingScale;

	private Vector3 m_startingRotation;

	private Vector3 m_startingPosition;

	private int m_startingLayer;

	private IPreOnDisable m_disableObj;

	private Relay<BaseEffect> m_onPlayRelay = new Relay<BaseEffect>();

	private Action<IPreOnDisable> m_detachFromSource;

	public IRelayLink<BaseEffect> OnPlayRelay => m_onPlayRelay.link;

	public bool IsFreePoolObj { get; set; }

	public bool IsAwakeCalled { get; protected set; }

	public int AnimatorLayer { get; set; }

	public bool DisableDestroyOnRoomChange { get; set; }

	public bool IsPlaying { get; protected set; }

	public EffectTriggerDirection EffectDirection { get; set; }

	public bool IsFlipped => base.gameObject.transform.localScale.x < 0f;

	public BaseEffect EffectPrefab { get; set; }

	public GameObject Source { get; set; }

	public Animator SourceAnimator { get; set; }

	protected virtual void Awake()
	{
		m_startingScale = base.transform.localScale;
		m_startingRotation = base.transform.localEulerAngles;
		m_startingPosition = base.transform.position;
		m_startingLayer = base.gameObject.layer;
		m_detachFromSource = DetachFromSource;
		IsAwakeCalled = true;
	}

	protected virtual void OnDisable()
	{
		if (m_disableObj != null)
		{
			RemoveDetachListener(m_disableObj);
		}
		DisablePooledObjectManager.DisablePooledObject(this);
	}

	public void AddDetachListener(IPreOnDisable disableObj)
	{
		m_disableObj = disableObj;
		disableObj.PreOnDisableRelay.AddOnce(m_detachFromSource);
	}

	public void RemoveDetachListener(IPreOnDisable disableObj)
	{
		m_disableObj = null;
		disableObj.PreOnDisableRelay.RemoveOnce(m_detachFromSource);
	}

	private void DetachFromSource(IPreOnDisable disableObj)
	{
		Source = null;
		SourceAnimator = null;
		base.transform.SetParent(null, worldPositionStays: true);
	}

	public virtual void Play(float duration, EffectStopType stopType)
	{
		IsPlaying = true;
		m_onPlayRelay.Dispatch(this);
	}

	public abstract void Stop(EffectStopType stopType);

	protected virtual void PlayComplete()
	{
		IsPlaying = false;
		if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public virtual void Flip()
	{
		base.transform.localScale = Vector3.Scale(base.transform.localScale, new Vector3(-1f, 1f, 1f));
	}

	public virtual void ResetValues()
	{
		if (m_disableObj != null)
		{
			RemoveDetachListener(m_disableObj);
		}
		base.transform.localScale = m_startingScale;
		base.transform.localEulerAngles = m_startingRotation;
		base.transform.position = m_startingPosition;
		Source = null;
		SourceAnimator = null;
		EffectDirection = EffectTriggerDirection.None;
		DisableDestroyOnRoomChange = false;
		if (base.gameObject.layer != m_startingLayer)
		{
			base.gameObject.SetLayerRecursively(m_startingLayer, includeInactive: true);
		}
	}

	[SpecialName]
	GameObject IGenericPoolObj.get_gameObject()
	{
		return base.gameObject;
	}
}
