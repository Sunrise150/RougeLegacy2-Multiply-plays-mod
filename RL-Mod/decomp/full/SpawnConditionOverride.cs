using UnityEngine;

public class SpawnConditionOverride
{
	public BiomeType Biome { get; private set; }

	public bool HasDash { get; private set; }

	public bool HasDoubleJump { get; private set; }

	public bool OverrideHasDash { get; private set; }

	public bool OverrideHasDoubleJump { get; private set; }

	public SpawnConditionOverride(BiomeType biome)
	{
		Biome = biome;
	}

	public void SetIsOverrideOn(SpawnConditionOverrideID condition, bool isOn)
	{
		switch (condition)
		{
		case SpawnConditionOverrideID.Dash:
			OverrideHasDash = isOn;
			return;
		case SpawnConditionOverrideID.DoubleJump:
			OverrideHasDoubleJump = isOn;
			return;
		}
		Debug.LogFormat("<color=red>| {0} | Unhandled condition ({1}). If you see this message, please add a bug to pivotal along with the Stack Trace</color>", this, condition);
	}

	public void SetOverrideValue(SpawnConditionOverrideID condition, bool value)
	{
		switch (condition)
		{
		case SpawnConditionOverrideID.Dash:
			HasDash = value;
			return;
		case SpawnConditionOverrideID.DoubleJump:
			HasDoubleJump = value;
			return;
		}
		Debug.LogFormat("<color=red>| {0} | Unhandled condition ({1}). If you see this message, please add a bug to pivotal along with the Stack Trace</color>", this, condition);
	}
}
