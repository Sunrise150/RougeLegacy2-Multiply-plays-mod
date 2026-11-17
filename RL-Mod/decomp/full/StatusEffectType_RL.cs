using System;

public class StatusEffectType_RL
{
	private static StatusEffectType[] m_typeArray;

	public static StatusEffectType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(StatusEffectType)) as StatusEffectType[];
			}
			return m_typeArray;
		}
	}

	public static StatusBarEntryType GetStatusBarType(StatusEffectType statusEffectType)
	{
		switch (statusEffectType)
		{
		case StatusEffectType.Enemy_Freeze:
			return StatusBarEntryType.Freeze;
		case StatusEffectType.Enemy_FreezeImmunity:
			return StatusBarEntryType.FreezeImmunity;
		case StatusEffectType.Enemy_Burn:
			return StatusBarEntryType.Burn;
		case StatusEffectType.Enemy_Burn_Immunity:
			return StatusBarEntryType.BurnImmunity;
		case StatusEffectType.Player_GodMode:
			return StatusBarEntryType.GodMode;
		case StatusEffectType.Enemy_ManaBurn:
			return StatusBarEntryType.ManaBurn;
		case StatusEffectType.Enemy_SporeBurst:
			return StatusBarEntryType.SporeBurst;
		case StatusEffectType.Enemy_ArmorBreak:
			return StatusBarEntryType.ArmorBreak;
		case StatusEffectType.Enemy_MagicBreak:
			return StatusBarEntryType.MagicBreak;
		case StatusEffectType.Player_Suave:
			return StatusBarEntryType.Suave;
		case StatusEffectType.Enemy_Phased:
			return StatusBarEntryType.Phased;
		case StatusEffectType.Enemy_Knockout:
			return StatusBarEntryType.Knockout;
		case StatusEffectType.Enemy_Dizzy:
			return StatusBarEntryType.Dizzy;
		case StatusEffectType.Enemy_DeathDelay:
			return StatusBarEntryType.DeathDelay;
		case StatusEffectType.Enemy_Combo:
		case StatusEffectType.Player_Combo:
			return StatusBarEntryType.Combo;
		case StatusEffectType.Enemy_Size:
			return StatusBarEntryType.Size;
		case StatusEffectType.Enemy_Speed:
			return StatusBarEntryType.Speed;
		case StatusEffectType.Enemy_AppliesInvuln:
			return StatusBarEntryType.AppliesInvuln;
		case StatusEffectType.Enemy_Invuln:
			return StatusBarEntryType.Invuln;
		case StatusEffectType.Enemy_FreeHit:
			return StatusBarEntryType.FreeHit;
		case StatusEffectType.Enemy_Disarm:
			return StatusBarEntryType.Disarm;
		case StatusEffectType.Player_Disarmed:
			return StatusBarEntryType.Disarmed;
		case StatusEffectType.Enemy_Curse_Projectile:
			return StatusBarEntryType.CurseProjectile;
		case StatusEffectType.Enemy_Explode:
			return StatusBarEntryType.Explode;
		case StatusEffectType.Enemy_ArmorShred:
			return StatusBarEntryType.ArmorShred;
		case StatusEffectType.Player_Exhaust:
			return StatusBarEntryType.Exhaust;
		case StatusEffectType.Enemy_Poison:
			return StatusBarEntryType.Poison;
		case StatusEffectType.Enemy_Bleed:
			return StatusBarEntryType.Bleed;
		case StatusEffectType.Enemy_Bane:
			return StatusBarEntryType.Bane;
		case StatusEffectType.Player_Dance:
			return StatusBarEntryType.Dance;
		case StatusEffectType.Enemy_Vulnerable:
			return StatusBarEntryType.Vulnerable;
		case StatusEffectType.Player_FreeCrit:
			return StatusBarEntryType.FreeCrit;
		case StatusEffectType.Player_Cloak:
			return StatusBarEntryType.Cloak;
		case StatusEffectType.Player_NoContactDamage:
			return StatusBarEntryType.NoContactDamage;
		case StatusEffectType.Enemy_InvulnWindow:
			return StatusBarEntryType.InvulnWindow;
		case StatusEffectType.Enemy_Aggro:
			return StatusBarEntryType.Aggro;
		case StatusEffectType.Enemy_Flamer:
			return StatusBarEntryType.Flamer;
		case StatusEffectType.Enemy_InvulnTimer:
			return StatusBarEntryType.InvulnTimer;
		default:
			return StatusBarEntryType.None;
		}
	}

	public static StatusEffectType GetStatusEffectType(StatusBarEntryType statusBarType)
	{
		return statusBarType switch
		{
			StatusBarEntryType.Freeze => StatusEffectType.Enemy_Freeze, 
			StatusBarEntryType.FreezeImmunity => StatusEffectType.Enemy_FreezeImmunity, 
			StatusBarEntryType.Burn => StatusEffectType.Enemy_Burn, 
			StatusBarEntryType.BurnImmunity => StatusEffectType.Enemy_Burn_Immunity, 
			StatusBarEntryType.GodMode => StatusEffectType.Player_GodMode, 
			StatusBarEntryType.SporeBurst => StatusEffectType.Enemy_SporeBurst, 
			StatusBarEntryType.ManaBurn => StatusEffectType.Enemy_ManaBurn, 
			StatusBarEntryType.ArmorBreak => StatusEffectType.Enemy_ArmorBreak, 
			StatusBarEntryType.MagicBreak => StatusEffectType.Enemy_MagicBreak, 
			StatusBarEntryType.Suave => StatusEffectType.Player_Suave, 
			StatusBarEntryType.Phased => StatusEffectType.Enemy_Phased, 
			StatusBarEntryType.Knockout => StatusEffectType.Enemy_Knockout, 
			StatusBarEntryType.Dizzy => StatusEffectType.Enemy_Dizzy, 
			StatusBarEntryType.DeathDelay => StatusEffectType.Enemy_DeathDelay, 
			StatusBarEntryType.Combo => StatusEffectType.Enemy_Combo, 
			StatusBarEntryType.Size => StatusEffectType.Enemy_Size, 
			StatusBarEntryType.Speed => StatusEffectType.Enemy_Speed, 
			StatusBarEntryType.AppliesInvuln => StatusEffectType.Enemy_AppliesInvuln, 
			StatusBarEntryType.Invuln => StatusEffectType.Enemy_Invuln, 
			StatusBarEntryType.FreeHit => StatusEffectType.Enemy_FreeHit, 
			StatusBarEntryType.Disarm => StatusEffectType.Enemy_Disarm, 
			StatusBarEntryType.Disarmed => StatusEffectType.Player_Disarmed, 
			StatusBarEntryType.CurseProjectile => StatusEffectType.Enemy_Curse_Projectile, 
			StatusBarEntryType.Exhaust => StatusEffectType.Player_Exhaust, 
			StatusBarEntryType.Poison => StatusEffectType.Enemy_Poison, 
			StatusBarEntryType.Bleed => StatusEffectType.Enemy_Bleed, 
			StatusBarEntryType.Bane => StatusEffectType.Enemy_Bane, 
			StatusBarEntryType.Dance => StatusEffectType.Player_Dance, 
			StatusBarEntryType.Vulnerable => StatusEffectType.Enemy_Vulnerable, 
			StatusBarEntryType.FreeCrit => StatusEffectType.Player_FreeCrit, 
			StatusBarEntryType.Cloak => StatusEffectType.Player_Cloak, 
			StatusBarEntryType.NoContactDamage => StatusEffectType.Player_NoContactDamage, 
			StatusBarEntryType.InvulnWindow => StatusEffectType.Enemy_InvulnWindow, 
			StatusBarEntryType.Aggro => StatusEffectType.Enemy_Aggro, 
			StatusBarEntryType.Flamer => StatusEffectType.Enemy_Flamer, 
			StatusBarEntryType.InvulnTimer => StatusEffectType.Enemy_InvulnTimer, 
			_ => StatusEffectType.None, 
		};
	}
}
