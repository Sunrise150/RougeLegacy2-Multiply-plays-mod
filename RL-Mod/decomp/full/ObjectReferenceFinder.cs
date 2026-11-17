using UnityEngine;

public class ObjectReferenceFinder : MonoBehaviour
{
	[SerializeField]
	private StringGameObjectDictionary m_objectReferenceTable;

	public GameObject GetObject(string ID)
	{
		if (m_objectReferenceTable.TryGetValue(ID, out var value))
		{
			return value;
		}
		Debug.Log("Object reference '" + ID + "' could not be found.  Are you sure this object was added to the Object Reference Finder component?");
		return null;
	}

	public T GetObject<T>(string ID, bool searchAllChildren = false, bool includeInactive = false) where T : Object
	{
		GameObject @object = GetObject(ID);
		if ((bool)@object)
		{
			if (!searchAllChildren)
			{
				return @object.GetComponent<T>();
			}
			return @object.GetComponentInChildren<T>(includeInactive);
		}
		return null;
	}
}
