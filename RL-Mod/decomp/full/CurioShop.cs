using System.Runtime.CompilerServices;
using UnityEngine;

public class CurioShop : MonoBehaviour, IRootObj
{
	private void OnEnable()
	{
		UpdateFlip();
	}

	private void Update()
	{
		if (PlayerManager.IsInstantiated)
		{
			UpdateFlip();
		}
	}

	private void UpdateFlip()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		bool flag = base.transform.localScale.x > 0f;
		bool flag2 = playerController.transform.localPosition.x > base.transform.localPosition.x;
		if ((flag && !flag2) || (!flag && flag2))
		{
			base.transform.SetLocalScaleX(base.transform.localScale.x * -1f);
		}
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
