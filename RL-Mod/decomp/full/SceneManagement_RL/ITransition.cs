using System.Collections;
using UnityEngine;

namespace SceneManagement_RL;

public interface ITransition
{
	GameObject GameObject { get; }

	TransitionID ID { get; }

	IEnumerator Run();
}
