using System;
using System.Collections.Generic;
using UnityEngine;

public class GenericPool_RL<T> where T : MonoBehaviour, IGenericPoolObj
{
	private T m_sourcePrefab;

	private List<T> m_genericList;

	private Action<MonoBehaviour, EventArgs> m_reparentAllPooledObjects;

	private bool m_disableAwakeOnCreation;

	public List<T> ObjectList => m_genericList;

	public bool HasActiveObjects
	{
		get
		{
			if (IsInitialized)
			{
				foreach (T generic in m_genericList)
				{
					if (generic.isActiveAndEnabled)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public int PoolSize { get; private set; }

	public bool IsPoolingDisabled { get; private set; }

	public bool IsInitialized { get; private set; }

	public void Initialize(T prefab, int poolSize, bool disablePooling, bool disableAwakeOnCreation = true)
	{
		if (IsInitialized)
		{
			Debug.LogWarning("Pool of type: " + prefab.GetType()?.ToString() + " already initialized.");
			return;
		}
		m_genericList = new List<T>();
		m_sourcePrefab = prefab;
		IsPoolingDisabled = disablePooling;
		m_disableAwakeOnCreation = disableAwakeOnCreation;
		if (!DisablePooledObjectManager.DisablePoolObjsHelper_STATIC)
		{
			DisablePooledObjectManager.DisablePoolObjsHelper_STATIC = new GameObject("Disable Pool Objs Helper");
			DisablePooledObjectManager.DisablePoolObjsHelper_STATIC.SetActive(value: false);
			DisablePooledObjectManager.DisablePoolObjsHelper_STATIC.transform.SetParent(null);
			UnityEngine.Object.DontDestroyOnLoad(DisablePooledObjectManager.DisablePoolObjsHelper_STATIC);
		}
		if (!disablePooling)
		{
			for (int i = 0; i < poolSize; i++)
			{
				CreateNew();
			}
		}
		m_reparentAllPooledObjects = ReparentAllPooledObjects;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.UpdatePools, m_reparentAllPooledObjects);
		IsInitialized = true;
	}

	private T CreateNew()
	{
		T val = null;
		if (!IsPoolingDisabled)
		{
			if (!m_disableAwakeOnCreation)
			{
				val = UnityEngine.Object.Instantiate(m_sourcePrefab, null, worldPositionStays: false);
			}
			else
			{
				val = UnityEngine.Object.Instantiate(m_sourcePrefab, DisablePooledObjectManager.DisablePoolObjsHelper_STATIC.transform, worldPositionStays: false);
				val.gameObject.SetActive(value: false);
				val.transform.SetParent(null);
			}
			UnityEngine.Object.DontDestroyOnLoad(val);
		}
		else
		{
			val = UnityEngine.Object.Instantiate(m_sourcePrefab);
		}
		val.name = m_sourcePrefab.name;
		if (!IsPoolingDisabled)
		{
			m_genericList.Add(val);
			if (val.gameObject.activeSelf)
			{
				val.gameObject.SetActive(value: false);
			}
			PoolSize++;
		}
		val.IsFreePoolObj = true;
		return val;
	}

	public void ResizePool(int newSize)
	{
		if (m_genericList == null)
		{
			Debug.Log("Cannot resize a non-existent pool.  Call Initialize() instead.");
			return;
		}
		DisableAll();
		ReparentAllPooledObjects(null, null);
		if (newSize == PoolSize)
		{
			return;
		}
		int num = newSize - PoolSize;
		if (num < 0)
		{
			for (int i = 0; i < num; i++)
			{
				CreateNew();
			}
		}
		else
		{
			for (int num2 = PoolSize - 1; num2 >= newSize; num2--)
			{
				UnityEngine.Object.Destroy(m_genericList[num2].gameObject);
				m_genericList.RemoveAt(num2);
			}
		}
		PoolSize = newSize;
	}

	public T GetFreeObj()
	{
		if (!IsPoolingDisabled)
		{
			int count = m_genericList.Count;
			for (int i = 0; i < count; i++)
			{
				T val = m_genericList[i];
				if (!val)
				{
					throw new Exception("A pooled object got destroyed!!!");
				}
				if (val.IsFreePoolObj)
				{
					if ((bool)val.transform.parent)
					{
						val.transform.SetParent(null);
						UnityEngine.Object.DontDestroyOnLoad(val);
					}
					val.IsFreePoolObj = false;
					if (val.IsAwakeCalled)
					{
						val.ResetValues();
					}
					return val;
				}
			}
		}
		T val2 = CreateNew();
		val2.IsFreePoolObj = false;
		return val2;
	}

	public bool DisableAll()
	{
		try
		{
			foreach (T generic in m_genericList)
			{
				DisablePooledObjectManager.DisablePooledObject(generic);
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	private void ReparentAllPooledObjects(object sender, EventArgs args)
	{
		foreach (T generic in m_genericList)
		{
			if ((bool)generic.transform.parent)
			{
				generic.transform.SetParent(null);
				UnityEngine.Object.DontDestroyOnLoad(generic);
			}
			if (generic.gameObject.activeSelf)
			{
				generic.gameObject.SetActive(value: false);
			}
		}
	}

	public void DestroyPool()
	{
		foreach (T generic in m_genericList)
		{
			if ((bool)generic)
			{
				UnityEngine.Object.Destroy(generic.gameObject);
			}
		}
		m_genericList.Clear();
		m_genericList = null;
		m_sourcePrefab = null;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.UpdatePools, m_reparentAllPooledObjects);
	}
}
