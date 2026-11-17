using System.Collections.Generic;
using UnityEngine;

public static class AnimatorUtility
{
	private static Dictionary<RuntimeAnimatorController, string> m_animatorNameTable = new Dictionary<RuntimeAnimatorController, string>();

	private static Dictionary<string, AnimatorInfo> m_animatorInfoDict = new Dictionary<string, AnimatorInfo>();

	private static bool AddAnimatorInfo(Animator animator)
	{
		if (!animator || !animator.isInitialized)
		{
			return false;
		}
		string cachedAnimatorName = animator.GetCachedAnimatorName();
		if (string.IsNullOrEmpty(cachedAnimatorName))
		{
			return false;
		}
		if (!m_animatorInfoDict.ContainsKey(cachedAnimatorName))
		{
			AnimatorInfo value = new AnimatorInfo(animator);
			m_animatorInfoDict.Add(cachedAnimatorName, value);
		}
		return true;
	}

	public static AnimatorInfo GetAnimatorInfo(Animator animator)
	{
		if (!animator)
		{
			return null;
		}
		string cachedAnimatorName = animator.GetCachedAnimatorName();
		if (string.IsNullOrEmpty(cachedAnimatorName))
		{
			return null;
		}
		if (AddAnimatorInfo(animator))
		{
			return m_animatorInfoDict[cachedAnimatorName];
		}
		return null;
	}

	public static bool HasParameter(Animator animator, int paramHash)
	{
		if (!animator)
		{
			return false;
		}
		return GetAnimatorInfo(animator)?.HasParameter(paramHash) ?? false;
	}

	public static bool HasParameter(Animator animator, string parameterName)
	{
		int paramHash = Animator.StringToHash(parameterName);
		return HasParameter(animator, paramHash);
	}

	public static bool HasState(Animator animator, int stateHash)
	{
		if (!animator)
		{
			return false;
		}
		return GetAnimatorInfo(animator)?.HasState(animator, stateHash) ?? false;
	}

	public static bool HasState(Animator animator, string stateName)
	{
		int stateHash = Animator.StringToHash(stateName);
		return HasState(animator, stateHash);
	}

	public static int GetLayerIndex(Animator animator, int stateHash)
	{
		if (!animator)
		{
			return -1;
		}
		return GetAnimatorInfo(animator)?.GetStateLayerIndex(animator, stateHash) ?? (-1);
	}

	public static int GetLayerIndex(Animator animator, string stateName)
	{
		int stateHash = Animator.StringToHash(stateName);
		return GetLayerIndex(animator, stateHash);
	}

	public static void ResetAnimator(Animator animator)
	{
		if (!animator)
		{
			return;
		}
		if (!animator.gameObject.activeInHierarchy)
		{
			animator.Rebind();
			return;
		}
		AnimatorInfo animatorInfo = GetAnimatorInfo(animator);
		if (animatorInfo == null)
		{
			return;
		}
		int num = animatorInfo.Parameters.Length;
		for (int i = 0; i < num; i++)
		{
			AnimatorInfo.AnimatorInfoParameterInternal animatorInfoParameterInternal = animatorInfo.Parameters[i];
			int nameHash = animatorInfoParameterInternal.NameHash;
			switch (animatorInfoParameterInternal.ParameterType)
			{
			case AnimatorControllerParameterType.Bool:
			case AnimatorControllerParameterType.Trigger:
				animator.SetBool(nameHash, animatorInfoParameterInternal.DefaultBool);
				break;
			case AnimatorControllerParameterType.Float:
				animator.SetFloat(nameHash, animatorInfoParameterInternal.DefaultFloat);
				break;
			case AnimatorControllerParameterType.Int:
				animator.SetInteger(nameHash, animatorInfoParameterInternal.DefaultInt);
				break;
			}
		}
		animator.Play(animatorInfo.StartingStateHash, 0, 0f);
	}

	public static string GetCachedAnimatorName(this Animator animator)
	{
		if (!animator.runtimeAnimatorController)
		{
			return null;
		}
		if (m_animatorNameTable.TryGetValue(animator.runtimeAnimatorController, out var value))
		{
			return value;
		}
		value = animator.runtimeAnimatorController.name;
		m_animatorNameTable.Add(animator.runtimeAnimatorController, value);
		return value;
	}

	public static void ClearAnimatorTables()
	{
		m_animatorNameTable.Clear();
		m_animatorInfoDict.Clear();
	}
}
