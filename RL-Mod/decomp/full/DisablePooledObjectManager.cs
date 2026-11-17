using UnityEngine;

public class DisablePooledObjectManager : MonoBehaviour
{
	public static GameObject DisablePoolObjsHelper_STATIC;

	public static DisablePooledObjectManager Instance { get; private set; }

	public static bool IsInitialized { get; private set; }

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
			IsInitialized = true;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	public static void DisablePooledObject(IGenericPoolObj pooledObj, bool reattachToOwnerPool = false)
	{
		if (IsInitialized)
		{
			if (pooledObj.gameObject.activeSelf)
			{
				pooledObj.gameObject.SetActive(value: false);
			}
			pooledObj.IsFreePoolObj = true;
		}
	}

	private void OnDestroy()
	{
		Instance = null;
		IsInitialized = false;
	}
}
