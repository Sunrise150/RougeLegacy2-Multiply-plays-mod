using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Text Popup Library")]
public class TextPopupLibrary : ScriptableObject
{
	[Serializable]
	public class TextPopupEntry
	{
		[SerializeField]
		private TextPopupObj m_textPopupPrefab;

		[SerializeField]
		private int m_poolSize = 5;

		public TextPopupObj TextPopupPrefab
		{
			get
			{
				return m_textPopupPrefab;
			}
			set
			{
				m_textPopupPrefab = value;
			}
		}

		public int PoolSize
		{
			get
			{
				return m_poolSize;
			}
			set
			{
				m_poolSize = value;
			}
		}
	}

	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/TextPopupLibrary";

	[SerializeField]
	private TextPopupTypeTextPopupObjDictionary m_textPopupDict;

	private static TextPopupLibrary m_instance;

	public static TextPopupTypeTextPopupObjDictionary TextPopupDict => Instance.m_textPopupDict;

	public static TextPopupLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<TextPopupLibrary>("Scriptable Objects/Libraries/TextPopupLibrary");
			}
			return m_instance;
		}
	}
}
