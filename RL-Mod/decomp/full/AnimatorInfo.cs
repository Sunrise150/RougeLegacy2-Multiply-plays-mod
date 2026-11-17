using UnityEngine;

public class AnimatorInfo
{
	private struct AnimatorInfoStateInternal
	{
		public int StateNameHash;

		public int LayerIndex;
	}

	public struct AnimatorInfoParameterInternal
	{
		public int NameHash;

		public int DefaultInt;

		public bool DefaultBool;

		public float DefaultFloat;

		public AnimatorControllerParameterType ParameterType;
	}

	private AnimatorInfoStateInternal[] m_states;

	private int m_stateIndex = -1;

	private uint m_stateCount;

	public AnimatorInfoParameterInternal[] Parameters { get; private set; }

	public int StartingStateHash { get; private set; }

	public bool HasState(Animator animator, string stateName)
	{
		return GetStateLayerIndex(animator, stateName) != -1;
	}

	public bool HasState(Animator animator, int stateNameHash)
	{
		return GetStateLayerIndex(animator, stateNameHash) != -1;
	}

	public int GetStateLayerIndex(Animator animator, string stateName)
	{
		int stateNameHash = Animator.StringToHash(stateName);
		return GetStateLayerIndex(animator, stateNameHash);
	}

	public int GetStateLayerIndex(Animator animator, int stateNameHash)
	{
		if (m_stateIndex + 1 == m_stateCount)
		{
			m_states = Expand(m_states, m_stateCount * 2, m_stateCount);
			m_stateCount *= 2u;
		}
		for (int i = 0; i <= m_stateIndex; i++)
		{
			AnimatorInfoStateInternal animatorInfoStateInternal = m_states[i];
			if (animatorInfoStateInternal.StateNameHash == stateNameHash)
			{
				return animatorInfoStateInternal.LayerIndex;
			}
		}
		int layerIndex = -1;
		int layerCount = animator.layerCount;
		for (int j = 0; j < layerCount; j++)
		{
			if (animator.HasState(j, stateNameHash))
			{
				layerIndex = j;
				break;
			}
		}
		AnimatorInfoStateInternal animatorInfoStateInternal2 = default(AnimatorInfoStateInternal);
		animatorInfoStateInternal2.StateNameHash = stateNameHash;
		animatorInfoStateInternal2.LayerIndex = layerIndex;
		AnimatorInfoStateInternal animatorInfoStateInternal3 = animatorInfoStateInternal2;
		m_stateIndex++;
		m_states[m_stateIndex] = animatorInfoStateInternal3;
		return animatorInfoStateInternal3.LayerIndex;
	}

	public bool HasParameter(int parameterNameHash)
	{
		int num = Parameters.Length;
		for (int i = 0; i < num; i++)
		{
			if (Parameters[i].NameHash == parameterNameHash)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasParameter(string parameterName)
	{
		return HasParameter(Animator.StringToHash(parameterName));
	}

	public AnimatorInfo(Animator animator)
	{
		StartingStateHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
		int parameterCount = animator.parameterCount;
		Parameters = new AnimatorInfoParameterInternal[parameterCount];
		AnimatorControllerParameter[] parameters = animator.parameters;
		for (int i = 0; i < parameterCount; i++)
		{
			AnimatorControllerParameter animatorControllerParameter = parameters[i];
			AnimatorInfoParameterInternal animatorInfoParameterInternal = new AnimatorInfoParameterInternal
			{
				NameHash = animatorControllerParameter.nameHash,
				ParameterType = animatorControllerParameter.type,
				DefaultBool = animatorControllerParameter.defaultBool,
				DefaultFloat = animatorControllerParameter.defaultFloat,
				DefaultInt = animatorControllerParameter.defaultInt
			};
			Parameters[i] = animatorInfoParameterInternal;
		}
		m_stateCount = 1u;
		m_states = new AnimatorInfoStateInternal[m_stateCount];
	}

	private AnimatorInfoStateInternal[] Expand(AnimatorInfoStateInternal[] arr, uint cap, uint count)
	{
		AnimatorInfoStateInternal[] array = new AnimatorInfoStateInternal[cap];
		for (int i = 0; i < count; i++)
		{
			array[i] = arr[i];
		}
		return array;
	}
}
