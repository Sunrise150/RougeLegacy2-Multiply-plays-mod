using System;
using UnityEngine;

public class CollisionType_RL
{
	private static CollisionType[] m_typeArray;

	public static CollisionType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(CollisionType)) as CollisionType[];
			}
			return m_typeArray;
		}
	}

	public static TagType GetEquivalentTag(CollisionType collisionType)
	{
		switch (collisionType)
		{
		case CollisionType.Player:
			return TagType.Player;
		case CollisionType.PlayerProjectile:
			return TagType.PlayerProjectile;
		case CollisionType.Enemy:
			return TagType.Enemy;
		case CollisionType.EnemyProjectile:
			return TagType.EnemyProjectile;
		case CollisionType.ItemDrop:
			return TagType.ItemDrop;
		case CollisionType.Breakable:
			return TagType.Breakable;
		case CollisionType.Hazard:
			return TagType.Hazard;
		case CollisionType.Platform:
			return TagType.Platform;
		case CollisionType.Chest:
			return TagType.Chest;
		case CollisionType.NPC:
			return TagType.NPC;
		case CollisionType.TriggerHazard:
			return TagType.TriggerHazard;
		case CollisionType.Player_Dodging:
			return TagType.Player_Dodging;
		case CollisionType.FlimsyBreakable:
			return TagType.FlimsyBreakable;
		case CollisionType.Generic_Bounceable:
			return TagType.Generic_Bounceable;
		case CollisionType.NonResonant_Bounceable:
			return TagType.NonResonant_Bounceable;
		case CollisionType.All:
		case CollisionType.None:
			return TagType.Untagged;
		default:
			throw new Exception("Cannot find Tag from CollisionType: " + collisionType.ToString() + ". Please make sure any tags related to collision are added to CollisionType_RL.");
		}
	}

	public static CollisionType GetEquivalentCollisionType(TagType tagType)
	{
		switch (tagType)
		{
		case TagType.Player:
			return CollisionType.Player;
		case TagType.PlayerProjectile:
			return CollisionType.PlayerProjectile;
		case TagType.Enemy:
			return CollisionType.Enemy;
		case TagType.EnemyProjectile:
			return CollisionType.EnemyProjectile;
		case TagType.ItemDrop:
			return CollisionType.ItemDrop;
		case TagType.Breakable:
			return CollisionType.Breakable;
		case TagType.Hazard:
			return CollisionType.Hazard;
		case TagType.Platform:
		case TagType.OneWay:
		case TagType.MagicPlatform:
			return CollisionType.Platform;
		case TagType.Chest:
			return CollisionType.Chest;
		case TagType.NPC:
			return CollisionType.NPC;
		case TagType.TriggerHazard:
			return CollisionType.TriggerHazard;
		case TagType.Player_Dodging:
			return CollisionType.Player_Dodging;
		case TagType.FlimsyBreakable:
			return CollisionType.FlimsyBreakable;
		case TagType.Generic_Bounceable:
			return CollisionType.Generic_Bounceable;
		case TagType.NonResonant_Bounceable:
			return CollisionType.NonResonant_Bounceable;
		default:
			return CollisionType.None;
		}
	}

	public static bool IsProjectile(GameObject obj)
	{
		if (!obj.CompareTag("Projectile") && !obj.CompareTag("PlayerProjectile") && !obj.CompareTag("EnemyProjectile"))
		{
			return false;
		}
		return true;
	}
}
