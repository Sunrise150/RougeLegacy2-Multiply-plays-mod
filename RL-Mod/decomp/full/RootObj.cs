using System.Runtime.CompilerServices;
using UnityEngine;

public class RootObj : MonoBehaviour, IRootObj
{
	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
