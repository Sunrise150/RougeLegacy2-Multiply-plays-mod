using System;
using UnityEngine;

public class DestroyOnRetailBuild : MonoBehaviour
{
	[HelpBox("This monobehaviour will trigger because #RETAIL_BUILD is ON. For security reasons, this monobehaviour will only destroy objects that are on this GameObject.", HelpBoxMessageType.Warning)]
	[SerializeField]
	private bool m_destroyGameObject;

	[SerializeField]
	private Component[] m_componentsToDestroy = new Component[0];

	public Component[] ComponentsToDestroyArray
	{
		get
		{
			return m_componentsToDestroy;
		}
		set
		{
			m_componentsToDestroy = value;
		}
	}

	private void Awake()
	{
		if (m_destroyGameObject)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		Component[] componentsToDestroy = m_componentsToDestroy;
		foreach (Component component in componentsToDestroy)
		{
			if (component.gameObject != base.gameObject)
			{
				throw new Exception("Cannot destroy component: " + component.ToString() + " on GameObject: " + base.name + ". Component MUST reside on same GameObject as DestroyOnRetailBuild.");
			}
			UnityEngine.Object.Destroy(component);
		}
	}
}
