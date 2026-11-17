using System.Collections.Generic;
using UnityEngine;

public class TextureSizeProfile : ScriptableObject
{
	[SerializeField]
	private List<string> m_textureNameList = new List<string>();

	[SerializeField]
	private List<int> m_textureSizeList = new List<int>();

	private Dictionary<string, int> m_textureTable = new Dictionary<string, int>();

	private bool m_tableIsDirty = true;

	public List<string> TextureNameList => m_textureNameList;

	public List<int> TextureSizeList => m_textureSizeList;

	public void AddProfileEntry(string texturePath, int maxTextureSize)
	{
		int num = m_textureNameList.IndexOf(texturePath);
		if (num == -1)
		{
			m_textureNameList.Add(texturePath);
			m_textureSizeList.Add(maxTextureSize);
		}
		else
		{
			Debug.Log("<color=red>WARNING: Texture: '" + texturePath + "' already found in TextureSize profile.</color>");
			m_textureSizeList[num] = maxTextureSize;
		}
		m_tableIsDirty = true;
	}

	public int GetEntryTextureSize(string texturePath)
	{
		RefreshTextureTable();
		if (m_textureTable.TryGetValue(texturePath, out var value))
		{
			return value;
		}
		return -1;
	}

	private void RefreshTextureTable()
	{
		if (m_tableIsDirty || m_textureTable.Count != m_textureNameList.Count)
		{
			m_textureTable.Clear();
			for (int i = 0; i < m_textureNameList.Count; i++)
			{
				m_textureTable.Add(m_textureNameList[i], m_textureSizeList[i]);
			}
			m_tableIsDirty = false;
		}
	}
}
