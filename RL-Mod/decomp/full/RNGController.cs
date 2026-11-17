using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RNGController
{
	private RngID m_id;

	private System.Random m_randomNumberGenerator;

	private int m_seed = -1;

	public List<RNGData> CallerData { get; private set; }

	public RngID ID
	{
		get
		{
			return m_id;
		}
		private set
		{
			m_id = value;
		}
	}

	public bool IsInitialized { get; private set; }

	public int Seed
	{
		get
		{
			return m_seed;
		}
		private set
		{
			m_seed = value;
		}
	}

	public RNGController(RngID id)
	{
		ID = id;
		Seed = -1;
	}

	public int GetRandomNumber(string callerDescription, int min, int max)
	{
		if (min >= max)
		{
			throw new ArgumentException("min or max", $"|{this}| argument min ({min}) must be less than argument max ({max})");
		}
		if (string.IsNullOrEmpty(callerDescription))
		{
			Debug.LogFormat("<color=red>| {0} | callerDescription argument is empty. Please add a description to your method call</color>", this);
		}
		return m_randomNumberGenerator.Next(min, max);
	}

	public float GetRandomNumber(string callerDescription, float min, float max)
	{
		if (min >= max)
		{
			throw new ArgumentException("min or max", $"|{this}| argument min ({min}) must be less than argument max ({max})");
		}
		if (string.IsNullOrEmpty(callerDescription))
		{
			Debug.LogFormat("<color=red>|{0}| callerDescription argument is empty. Please add a description to your method call</color>", this);
		}
		return min + (float)m_randomNumberGenerator.NextDouble() * (max - min);
	}

	private void InitialiseRandomNumberGenerator(int seed)
	{
		Seed = seed;
		m_randomNumberGenerator = new System.Random(Seed);
	}

	public void SetSeed(int seed)
	{
		if (Application.isPlaying)
		{
			if (CallerData == null)
			{
				CallerData = new List<RNGData>();
			}
			else
			{
				CallerData.Clear();
			}
			InitialiseRandomNumberGenerator(seed);
			IsInitialized = true;
		}
		else
		{
			Debug.LogFormat("<color=red>|{0}| RNGController's Initialise Method was called during Edit Mode, BUT SHOULD NOT BE</color>", this);
		}
	}
}
