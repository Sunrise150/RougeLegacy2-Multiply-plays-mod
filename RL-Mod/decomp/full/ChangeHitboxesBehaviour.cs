using UnityEngine;

public class ChangeHitboxesBehaviour : StateMachineBehaviour
{
	[Space(10f)]
	[SerializeField]
	private string[] m_hitboxesToDisable;

	[Space(10f)]
	[SerializeField]
	private string[] m_hitboxesToEnable;

	private GameObject[] m_hitboxesToDisableObjList;

	private GameObject[] m_hitboxesToEnableObjList;

	private bool m_cached;

	public void CacheHitboxes(Animator animator)
	{
		if (m_cached)
		{
			return;
		}
		m_hitboxesToEnableObjList = new GameObject[m_hitboxesToEnable.Length];
		m_hitboxesToDisableObjList = new GameObject[m_hitboxesToDisable.Length];
		IHitboxController componentInChildren = animator.GetRoot().GetComponentInChildren<IHitboxController>(includeInactive: true);
		if (componentInChildren != null)
		{
			for (int i = 0; i < m_hitboxesToDisable.Length; i++)
			{
				string objName = m_hitboxesToDisable[i];
				GameObject gameObject = CheckForHitbox(objName, componentInChildren);
				if (gameObject != null)
				{
					gameObject.SetActive(value: false);
					m_hitboxesToDisableObjList[i] = gameObject;
				}
			}
			for (int j = 0; j < m_hitboxesToEnable.Length; j++)
			{
				string objName2 = m_hitboxesToEnable[j];
				GameObject gameObject2 = CheckForHitbox(objName2, componentInChildren);
				if (gameObject2 != null)
				{
					gameObject2.SetActive(value: true);
					m_hitboxesToEnableObjList[j] = gameObject2;
				}
			}
		}
		m_cached = true;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		CacheHitboxes(animator);
		GameObject[] hitboxesToEnableObjList = m_hitboxesToEnableObjList;
		foreach (GameObject gameObject in hitboxesToEnableObjList)
		{
			if ((bool)gameObject)
			{
				gameObject.SetActive(value: true);
			}
		}
		hitboxesToEnableObjList = m_hitboxesToDisableObjList;
		foreach (GameObject gameObject2 in hitboxesToEnableObjList)
		{
			if ((bool)gameObject2)
			{
				gameObject2.SetActive(value: false);
			}
		}
	}

	private GameObject CheckForHitbox(string objName, IHitboxController hbController)
	{
		GameObject gameObject = hbController.ContainsHitbox(HitboxType.Body, objName);
		if (gameObject != null)
		{
			return gameObject;
		}
		gameObject = hbController.ContainsHitbox(HitboxType.Weapon, objName);
		if (gameObject != null)
		{
			return gameObject;
		}
		gameObject = hbController.ContainsHitbox(HitboxType.Terrain, objName);
		if (gameObject != null)
		{
			return gameObject;
		}
		return null;
	}
}
