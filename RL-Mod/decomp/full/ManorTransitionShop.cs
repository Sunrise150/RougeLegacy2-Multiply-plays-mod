using System.Collections;
using System.Runtime.CompilerServices;
using RLAudio;
using SceneManagement_RL;
using UnityEngine;

public class ManorTransitionShop : MonoBehaviour, IRootObj
{
	[SerializeField]
	private GameObject m_playerEnterPositionObj;

	public void TransitionToManor()
	{
		StartCoroutine(TransitionToManorCoroutine());
	}

	private IEnumerator TransitionToManorCoroutine()
	{
		yield return MovePlayerToPosition(m_playerEnterPositionObj.transform.position);
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_docks_transition_toSkilltree");
		SceneLoader_RL.RunTransitionWithLogic(EnterManorCoroutine(), TransitionID.GardenToManor, cleanup: false);
	}

	private IEnumerator MovePlayerToPosition(Vector3 playerPos)
	{
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(playerPos, checkXAxisOnly: true);
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	private IEnumerator EnterManorCoroutine()
	{
		PlayerManager.GetCurrentPlayerRoom().gameObject.FindObjectReference<TunnelSpawnController>("ManorTunnel").Tunnel.ForceEnterTunnel(animate: false);
		PlayerManager.GetPlayerController().SetFacing(faceRight: true);
		yield break;
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
