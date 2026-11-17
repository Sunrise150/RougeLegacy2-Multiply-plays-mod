using System.Collections;

namespace SceneManagement_RL;

public interface ISceneLoadingTransition : ITransition
{
	IEnumerator TransitionOut();

	IEnumerator TransitionIn();
}
