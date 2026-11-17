public static class DamageType_RL
{
	public const string Strength_String = "Strength";

	public const string Magic_String = "Magic";

	public static DamageType ToEnum(string damageType)
	{
		return damageType switch
		{
			"Magic" => DamageType.Magic, 
			_ => DamageType.Strength, 
		};
	}
}
