using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Class Library")]
public class ClassLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/ClassLibrary";

	[SerializeField]
	private ClassTypeClassDataDictionary m_classLibrary;

	private static ClassLibrary m_instance;

	private static ClassLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<ClassLibrary>("Scriptable Objects/Libraries/ClassLibrary");
			}
			return m_instance;
		}
	}

	public static ClassData GetClassData(ClassType classType)
	{
		ClassData value = null;
		ClassTypeClassDataDictionary classLibrary = Instance.m_classLibrary;
		if (classLibrary != null)
		{
			classLibrary.TryGetValue(classType, out value);
			if (value == null)
			{
				Debug.LogWarning("<color=red>Class: " + classType.ToString() + " not found in Class Library. Please ensure the class exists in the Class Library scriptable object.</color>");
			}
			return value;
		}
		throw new Exception("Class Library is null.");
	}
}
