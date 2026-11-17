using System.Collections;
using System.Runtime.CompilerServices;
using RL_Windows;
using SceneManagement_RL;
using UnityEngine;

public class ParadeTransition : MonoBehaviour, IRootObj
{
	[SerializeField]
	private GameObject m_playerEnterPositionObj;

	public void TransitionToParade()
	{
		StartCoroutine(TransitionToParadeCoroutine());
	}

	private IEnumerator TransitionToParadeCoroutine()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return MovePlayerToPosition(m_playerEnterPositionObj.transform.position);
		ISceneLoadingTransition transition = TransitionLibrary.GetTransitionInstance(TransitionID.FadeToBlackNoLoading) as ISceneLoadingTransition;
		transition.GameObject.SetActive(value: true);
		yield return null;
		yield return transition.TransitionIn();
		float delay = Time.time + 1f;
		while (Time.time < delay)
		{
			yield return null;
		}
		PlayerManager.GetPlayerController().StopActiveAbilities(stopPersistentAbilities: true);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerEnterParade, null, null);
		WindowManager.SetWindowIsOpen(WindowID.PlayerDeath, isOpen: true);
		yield return null;
		yield return transition.TransitionOut();
		transition.GameObject.SetActive(value: false);
	}

	private IEnumerator MovePlayerToPosition(Vector3 playerPos)
	{
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(playerPos, checkXAxisOnly: true);
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
