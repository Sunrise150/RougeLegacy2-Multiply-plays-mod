using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Look Library")]
public class LookLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/LookLibrary";

	[SerializeField]
	private MaterialWeight_LookData m_eyeData;

	[SerializeField]
	private MaterialWeight_LookData m_mouthData;

	[SerializeField]
	private MaterialWeight_LookData m_facialHairData;

	[SerializeField]
	private ColorWeight_LookData m_skinColorData;

	[SerializeField]
	private MaterialWeight_LookData m_hairData;

	[SerializeField]
	private ColorWeight_LookData m_hairColorData;

	[SerializeField]
	private BodyWeight_LookData m_bodyData;

	[Header("Special Materials")]
	[SerializeField]
	private Material m_vampireFangs_Material;

	[Space(5f)]
	[SerializeField]
	private Material m_clownEyes_Material;

	[SerializeField]
	private Material m_clownMouth_Material;

	[SerializeField]
	private Material m_clownHead_Material;

	private static LookLibrary m_instance;

	private static LookLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<LookLibrary>("Scriptable Objects/Libraries/LookLibrary");
			}
			return m_instance;
		}
	}

	public static Material VampireFangsMaterial => Instance.m_vampireFangs_Material;

	public static Material ClownEyesMaterial => Instance.m_clownEyes_Material;

	public static Material ClownMouthMaterial => Instance.m_clownMouth_Material;

	public static Material ClownHeadMaterial => Instance.m_clownHead_Material;

	public static List<T> GetLookDataByTag<T>(string tag, LookType lookType)
	{
		List<T> list = new List<T>();
		IList list2 = null;
		switch (lookType)
		{
		case LookType.Eyes:
			list2 = GetEyeLookData();
			break;
		case LookType.FacialHair:
			list2 = GetFacialHairLookData();
			break;
		case LookType.Mouth:
			list2 = GetMouthLookData();
			break;
		case LookType.Hair:
			list2 = GetHairLookData();
			break;
		case LookType.SkinColor:
			list2 = GetSkinColorLookData();
			break;
		case LookType.HairColor:
			list2 = GetHairColorLookData();
			break;
		case LookType.Body:
			list2 = GetBodyLookData();
			break;
		}
		_ = list2.Count;
		_ = 0;
		foreach (ILookWeight item in list2)
		{
			string[] tags = item.Tags;
			for (int i = 0; i < tags.Length; i++)
			{
				if (tags[i] == tag)
				{
					list.Add((T)item);
				}
			}
		}
		return list;
	}

	public static List<MaterialWeightObject> GetEyeLookData()
	{
		return Instance.m_eyeData.WeightList;
	}

	public static List<MaterialWeightObject> GetEyeLookData(string tag)
	{
		return GetLookDataByTag<MaterialWeightObject>(tag, LookType.Eyes);
	}

	public static List<MaterialWeightObject> GetMouthLookData()
	{
		return Instance.m_mouthData.WeightList;
	}

	public static List<MaterialWeightObject> GetMouthLookData(string tag)
	{
		return GetLookDataByTag<MaterialWeightObject>(tag, LookType.Mouth);
	}

	public static List<MaterialWeightObject> GetFacialHairLookData()
	{
		return Instance.m_facialHairData.WeightList;
	}

	public static List<MaterialWeightObject> GetFacialHairLookData(string tag)
	{
		return GetLookDataByTag<MaterialWeightObject>(tag, LookType.FacialHair);
	}

	public static List<ColorWeightObject> GetSkinColorLookData()
	{
		return Instance.m_skinColorData.WeightList;
	}

	public static List<ColorWeightObject> GetSkinColorLookData(string tag)
	{
		return GetLookDataByTag<ColorWeightObject>(tag, LookType.SkinColor);
	}

	public static List<MaterialWeightObject> GetHairLookData()
	{
		return Instance.m_hairData.WeightList;
	}

	public static List<MaterialWeightObject> GetHairLookData(string tag)
	{
		return GetLookDataByTag<MaterialWeightObject>(tag, LookType.Hair);
	}

	public static List<BodyWeightObject> GetBodyLookData()
	{
		return Instance.m_bodyData.WeightList;
	}

	public static List<BodyWeightObject> GetBodyLookData(string tag)
	{
		return GetLookDataByTag<BodyWeightObject>(tag, LookType.Body);
	}

	public static List<ColorWeightObject> GetHairColorLookData()
	{
		return Instance.m_hairColorData.WeightList;
	}

	public static List<ColorWeightObject> GetHairColorLookData(string tag)
	{
		return GetLookDataByTag<ColorWeightObject>(tag, LookType.HairColor);
	}
}
