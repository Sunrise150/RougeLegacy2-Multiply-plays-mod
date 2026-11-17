using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct RoomID : IEquatable<RoomID>
{
	[SerializeField]
	private string m_sceneName;

	[SerializeField]
	private int m_number;

	public int Number
	{
		get
		{
			return m_number;
		}
		set
		{
			m_number = value;
		}
	}

	public string SceneName
	{
		get
		{
			return m_sceneName;
		}
		set
		{
			m_sceneName = value;
		}
	}

	public RoomID(Scene scene, int number)
		: this(scene.name, number)
	{
	}

	public RoomID(string sceneName, int number)
	{
		m_sceneName = sceneName;
		m_number = number;
	}

	public override string ToString()
	{
		string result = "NONE";
		if (!string.IsNullOrEmpty(SceneName))
		{
			result = $"{SceneName} {Number}";
		}
		return result;
	}

	public static bool operator ==(RoomID id1, RoomID id2)
	{
		if (id1.SceneName == id2.SceneName)
		{
			return id1.Number == id2.Number;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is RoomID))
		{
			return false;
		}
		return this == (RoomID)obj;
	}

	public static bool operator !=(RoomID id1, RoomID id2)
	{
		return !(id1 == id2);
	}

	public override int GetHashCode()
	{
		return SceneName.GetHashCode() * Number;
	}

	public bool Equals(RoomID other)
	{
		return this == other;
	}
}
