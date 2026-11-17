using System;

public class InsightType_RL
{
	private static InsightType[] m_insightTypeArray;

	public static InsightType[] TypeArray
	{
		get
		{
			if (m_insightTypeArray == null)
			{
				m_insightTypeArray = Enum.GetValues(typeof(InsightType)) as InsightType[];
			}
			return m_insightTypeArray;
		}
	}

	public static InsightType GetInsightFromHeirloomType(HeirloomType heirloomType)
	{
		return heirloomType switch
		{
			HeirloomType.UnlockAirDash => InsightType.HeirloomDash, 
			HeirloomType.UnlockMemory => InsightType.HeirloomMemory, 
			HeirloomType.UnlockDoubleJump => InsightType.HeirloomDoubleJump, 
			HeirloomType.UnlockBouncableDownstrike => InsightType.HeirloomSpinKick_Projectiles, 
			HeirloomType.UnlockVoidDash => InsightType.HeirloomVoidDash, 
			HeirloomType.UnlockEarthShift => InsightType.HeirloomEarthShift, 
			_ => InsightType.None, 
		};
	}
}
