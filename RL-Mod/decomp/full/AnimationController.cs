using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
	[SerializeField]
	private Animator m_animator;

	[SerializeField]
	private List<AnimationControllerEntry> m_entries;

	public Animator Animator
	{
		get
		{
			return m_animator;
		}
		set
		{
			m_animator = value;
		}
	}

	public List<AnimationControllerEntry> AnimationEntries
	{
		get
		{
			return m_entries;
		}
		set
		{
			m_entries = value;
		}
	}

	private void OnValidate()
	{
		if (m_entries != null)
		{
			for (int i = 0; i < m_entries.Count; i++)
			{
				m_entries[i].Id = i;
			}
		}
	}

	public void Trigger(int animationEntryID)
	{
		if (Animator != null)
		{
			AnimationControllerEntry entry = GetEntry(animationEntryID);
			if (entry != null)
			{
				switch (entry.ParameterType)
				{
				case AnimatorControllerParameterType.Float:
					Animator.SetFloat(entry.AnimationParameter, entry.FloatValue);
					break;
				case AnimatorControllerParameterType.Int:
					Animator.SetInteger(entry.AnimationParameter, entry.IntValue);
					break;
				case AnimatorControllerParameterType.Bool:
					Animator.SetBool(entry.AnimationParameter, entry.BoolValue);
					break;
				case AnimatorControllerParameterType.Trigger:
					Animator.SetTrigger(entry.AnimationParameter);
					break;
				}
			}
		}
		else
		{
			Debug.LogFormat("<color=red>[{0}] Animator Field is null</color>", this);
		}
	}

	private AnimationControllerEntry GetEntry(int animationEntryID)
	{
		if (AnimationEntries != null && AnimationEntries.Count > 0 && animationEntryID >= 0 && animationEntryID < AnimationEntries.Count)
		{
			return AnimationEntries[animationEntryID];
		}
		if (AnimationEntries == null || AnimationEntries.Count == 0)
		{
			Debug.LogFormat("<color=red>[{0}] Animation Entries are null or empty</color>", this);
		}
		if (animationEntryID < 0 || animationEntryID >= AnimationEntries.Count)
		{
			Debug.LogFormat("<color=red>[{0}] animationEntryID ({1}) is out of bounds</color>", this, animationEntryID);
		}
		return null;
	}

	public bool GetParameterExistsInAnimator(string animatorParameter)
	{
		if (Animator != null)
		{
			return Animator.parameters.Any((AnimatorControllerParameter parameter) => parameter.name == animatorParameter);
		}
		return false;
	}

	public bool GetIsParameterTypeValid(AnimationControllerEntry entry)
	{
		if (entry != null && Animator != null && AnimationEntries != null && AnimationEntries.Count > 0 && GetParameterExistsInAnimator(entry.AnimationParameter))
		{
			int index = 0;
			for (int i = 0; i < Animator.parameters.Length; i++)
			{
				if (entry.AnimationParameter == Animator.parameters[i].name)
				{
					index = i;
					break;
				}
			}
			AnimatorControllerParameter parameter = Animator.GetParameter(index);
			return entry.ParameterType == parameter.type;
		}
		return false;
	}
}
