using System.Collections;
using UnityEngine;

namespace SceneManagement_RL;

public abstract class Transition_V2 : MonoBehaviour, ITransition
{
	public GameObject GameObject => base.gameObject;

	public abstract TransitionID ID { get; }

	protected virtual void Awake()
	{
	}

	public abstract IEnumerator Run();
}
