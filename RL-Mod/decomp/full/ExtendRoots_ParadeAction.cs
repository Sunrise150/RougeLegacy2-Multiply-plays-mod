using System;
using System.Collections;
using UnityEngine;

public class ExtendRoots_ParadeAction : MonoBehaviour, IParadeAction
{
	[Serializable]
	private class ExtendRootEntry
	{
		[SerializeField]
		private Animator m_animator;

		[SerializeField]
		private float m_delay;

		public float Delay => m_delay;

		public Animator Animator => m_animator;
	}

	[SerializeField]
	private ExtendRootEntry[] m_rootsToExtend;

	public IEnumerator TriggerAction(ParadeController controller)
	{
		controller.SetRootMoveAudioParam(moving: true);
		ExtendRootEntry[] rootsToExtend = m_rootsToExtend;
		foreach (ExtendRootEntry entry in rootsToExtend)
		{
			entry.Animator.speed = 1.15f;
			if (entry.Delay > 0f)
			{
				float delay2 = entry.Delay * 0.85f;
				delay2 += Time.time;
				while (Time.time < delay2)
				{
					yield return null;
				}
			}
			entry.Animator.SetBool("Extend", value: true);
		}
		controller.SetRootMoveAudioParam(moving: false);
	}
}
