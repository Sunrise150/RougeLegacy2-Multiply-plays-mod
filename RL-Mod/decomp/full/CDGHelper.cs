using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CDGHelper
{
	private static Dictionary<string, TagType> m_tagToEnumTable = new Dictionary<string, TagType>();

	public static T FindStaticInstance<T>(bool createNewIfNull) where T : MonoBehaviour
	{
		T[] array = UnityEngine.Object.FindObjectsOfType<T>();
		if (array.Length == 0 && createNewIfNull)
		{
			return new GameObject(typeof(T).ToString()).AddComponent<T>();
		}
		if (array.Length == 1)
		{
			return array[0].GetComponent<T>();
		}
		if (array.Length > 1)
		{
			throw new Exception($"{Time.frameCount}: Found multiple instances of given Type");
		}
		return null;
	}

	public static T FindStaticInstance<T>(string instanceName, bool createNewIfNull) where T : Component
	{
		GameObject gameObject = GameObject.Find(instanceName);
		T val = null;
		if (gameObject != null)
		{
			val = gameObject.GetComponent<T>();
			if (val == null)
			{
				throw new MissingComponentException(instanceName);
			}
		}
		if (gameObject == null && createNewIfNull)
		{
			gameObject = new GameObject();
			gameObject.name = instanceName;
			val = gameObject.AddComponent<T>();
		}
		return val;
	}

	public static T FindStaticInstance<T>(string instanceName, string prefabPath, bool createNewIfNull) where T : Component
	{
		GameObject gameObject = GameObject.Find(instanceName);
		T val = null;
		if (gameObject != null)
		{
			val = gameObject.GetComponent<T>();
			if (val == null)
			{
				throw new MissingComponentException(instanceName);
			}
		}
		if (gameObject == null && createNewIfNull)
		{
			val = UnityEngine.Object.Instantiate(CDGResources.Load<T>(prefabPath));
			val.gameObject.name = instanceName;
			if (val == null)
			{
				throw new Exception("Cannot create new static instance.");
			}
		}
		return val;
	}

	public static int RandomPlusMinus()
	{
		if (UnityEngine.Random.Range(0, 2) < 1)
		{
			return -1;
		}
		return 1;
	}

	public static Vector2 AngleToVector(float degAngle)
	{
		float num = degAngle;
		degAngle *= (float)Math.PI / 180f;
		Vector2 result = new Vector2(Mathf.Cos(degAngle), Mathf.Sin(degAngle));
		if (num == 90f || num == -90f || num == 270f || num == -270f)
		{
			result.x = 0f;
		}
		if (num == 0f || num == 360f || num == 180f || num == -180f)
		{
			result.y = 0f;
		}
		return result;
	}

	public static float VectorToAngle(Vector2 pt)
	{
		return AngleBetweenPts(Vector2.zero, pt);
	}

	public static float AngleBetweenPts(Vector2 pt1, Vector2 pt2)
	{
		float x = pt2.x - pt1.x;
		return WrapAngleDegrees(Mathf.Atan2(pt2.y - pt1.y, x) * 57.29578f, convertToHalfRadius: false);
	}

	public static Vector2 VectorBetweenPts(Vector2 pt1, Vector2 pt2)
	{
		return new Vector2(pt2.x - pt1.x, pt2.y - pt1.y);
	}

	public static float DistanceBetweenPts(Vector2 pt1, Vector2 pt2)
	{
		float num = pt2.x - pt1.x;
		float num2 = pt2.y - pt1.y;
		return Mathf.Sqrt(num * num + num2 * num2);
	}

	public static float ConvertToGameUnits(float pixelFloat)
	{
		return pixelFloat / 60f;
	}

	public static Vector2 ConvertToGameUnits(Vector2 pixelVector)
	{
		return new Vector2(pixelVector.x / 60f, pixelVector.y / 60f);
	}

	public static float ConvertToPixelUnits(float gameFloat)
	{
		return gameFloat * 60f;
	}

	public static Vector2 ConvertToPixelUnits(Vector2 gameVector)
	{
		return new Vector2(gameVector.x * 60f, gameVector.y * 60f);
	}

	public static Vector3 ToVector3(Vector2 vector)
	{
		return new Vector3(vector.x, vector.y, 0f);
	}

	public static Vector3 ToVector3(float value)
	{
		return new Vector3(value, value, value);
	}

	public static Mesh BuildMeshFromPolygonCollider2D(PolygonCollider2D poly2D)
	{
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		int pathCount = poly2D.pathCount;
		for (int i = 0; i < pathCount; i++)
		{
			Vector2[] path = poly2D.GetPath(i);
			for (int j = 0; j < path.Length; j++)
			{
				list.Add(new Vector3(path[j].x, path[j].y, 0f));
			}
			int[] array = new Triangulator(path).Triangulate();
			for (int k = 0; k < array.Length; k++)
			{
				array[k] += i * pathCount;
			}
			list2.AddRange(array);
		}
		mesh.vertices = list.ToArray();
		mesh.triangles = list2.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

	public static Mesh BuildMeshFromPointList(List<Vector2> pointsList)
	{
		Mesh mesh = new Mesh();
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		Vector2[] array = pointsList.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			list.Add(new Vector3(array[i].x, array[i].y, 0f));
		}
		int[] collection = new Triangulator(array).Triangulate();
		list2.AddRange(collection);
		mesh.vertices = list.ToArray();
		mesh.triangles = list2.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		return mesh;
	}

	public static Vector2 RotatedPoint(Vector2 thePoint, Vector2 theOrigin, float theRotation)
	{
		theRotation *= (float)Math.PI / 180f;
		double num = Mathf.Cos(theRotation);
		double num2 = Mathf.Sin(theRotation);
		Vector2 result = default(Vector2);
		result.x = (float)((double)theOrigin.x + (double)(thePoint.x - theOrigin.x) * num - (double)(thePoint.y - theOrigin.y) * num2);
		result.y = (float)((double)theOrigin.y + (double)(thePoint.y - theOrigin.y) * num + (double)(thePoint.x - theOrigin.x) * num2);
		return result;
	}

	public static Vector2 ChangeVector2Length(Vector2 thePoint, Vector2 theOrigin, float theLength)
	{
		thePoint -= theOrigin;
		thePoint = thePoint.normalized * theLength;
		return thePoint + theOrigin;
	}

	public static float GetLockedAngle(float currentAngle, int numberOfDirections)
	{
		currentAngle = WrapAngleDegrees(currentAngle, convertToHalfRadius: false);
		float num = 360f / (float)numberOfDirections;
		float num2 = num / 2f;
		float num3 = 0f;
		for (int i = 0; i <= numberOfDirections; i++)
		{
			float num4 = num3 + num2;
			float num5 = num3 - num2;
			if (currentAngle <= num4 && currentAngle >= num5)
			{
				if (num3 >= 360f)
				{
					return 0f;
				}
				return num3;
			}
			num3 += num;
		}
		return currentAngle;
	}

	public static float WrapAngleDegrees(float degrees, bool convertToHalfRadius)
	{
		return ToDegrees(WrapAngleRadians(ToRadians(degrees), convertToHalfRadius));
	}

	public static float WrapAngleRadians(float radians, bool convertToHalfRadius)
	{
		float num = (float)Math.PI * 2f;
		int num2 = (int)(radians / num);
		radians -= num * (float)num2;
		if (convertToHalfRadius)
		{
			if (radians > (float)Math.PI)
			{
				return radians - num;
			}
			if (radians < -(float)Math.PI)
			{
				return radians + num;
			}
			return radians;
		}
		if (radians < 0f)
		{
			return radians + num;
		}
		return radians;
	}

	public static float TurnToFaceRadians(Vector2 sourcePosition, Vector2 targetPosition, float sourceTurnSpeedInRads, float sourceOrientationInRads, float elapsedTime, bool convertToHalfRadius)
	{
		float x = targetPosition.x - sourcePosition.x;
		float num = WrapAngleRadians(Mathf.Atan2(targetPosition.y - sourcePosition.y, x), convertToHalfRadius: false);
		float num2 = WrapAngleRadians(sourceOrientationInRads, convertToHalfRadius: false);
		float num3 = WrapAngleRadians(sourceOrientationInRads, convertToHalfRadius: true);
		float num4 = num - num2;
		bool num5 = num4 > (float)Math.PI || num4 < -(float)Math.PI;
		float num6 = sourceTurnSpeedInRads * elapsedTime;
		if (num5)
		{
			float value = WrapAngleRadians(num4, convertToHalfRadius: true);
			value = Mathf.Clamp(value, 0f - num6, num6);
			return WrapAngleRadians(num3 + value, convertToHalfRadius);
		}
		num4 = Mathf.Clamp(num4, 0f - num6, num6);
		return WrapAngleRadians(num2 + num4, convertToHalfRadius);
	}

	public static float TurnToFaceDegrees(Vector2 sourcePosition, Vector2 targetPosition, float sourceTurnSpeedInDegrees, float sourceOrientationInDegrees, float elapsedTime, bool convertToHalfRadius)
	{
		return ToDegrees(TurnToFaceRadians(sourcePosition, targetPosition, ToRadians(sourceTurnSpeedInDegrees), ToRadians(sourceOrientationInDegrees), elapsedTime, convertToHalfRadius));
	}

	public static int GetRandomOdds(List<float> oddsArray)
	{
		int count = oddsArray.Count;
		float num = 0f;
		for (int i = 0; i < count; i++)
		{
			num += oddsArray[i];
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		int num4 = 0;
		foreach (float item in oddsArray)
		{
			if (item == 0f)
			{
				num4++;
				continue;
			}
			num3 += item;
			if (num2 <= num3)
			{
				return num4;
			}
			num4++;
		}
		return -1;
	}

	public static int GetRandomOdds(float[] oddsArray)
	{
		int num = oddsArray.Length;
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			num2 += oddsArray[i];
		}
		float num3 = UnityEngine.Random.Range(0f, num2);
		float num4 = 0f;
		int num5 = 0;
		foreach (float num6 in oddsArray)
		{
			if (num6 == 0f)
			{
				num5++;
				continue;
			}
			num4 += num6;
			if (num3 <= num4)
			{
				return num5;
			}
			num5++;
		}
		return -1;
	}

	public static int GetRandomOdds(List<int> oddsArray)
	{
		int count = oddsArray.Count;
		int num = 0;
		for (int i = 0; i < count; i++)
		{
			num += oddsArray[i];
		}
		int num2 = UnityEngine.Random.Range(0, num + 1);
		int num3 = 0;
		int num4 = 0;
		foreach (int item in oddsArray)
		{
			if (item == 0)
			{
				num4++;
				continue;
			}
			num3 += item;
			if (num2 <= num3)
			{
				return num4;
			}
			num4++;
		}
		return -1;
	}

	public static int GetRandomOdds(int[] oddsArray)
	{
		int num = oddsArray.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 += oddsArray[i];
		}
		int num3 = UnityEngine.Random.Range(0, num2 + 1);
		int num4 = 0;
		int num5 = 0;
		foreach (int num6 in oddsArray)
		{
			if (num6 == 0)
			{
				num5++;
				continue;
			}
			num4 += num6;
			if (num3 <= num4)
			{
				return num5;
			}
			num5++;
		}
		return -1;
	}

	public static void Shuffle<T>(List<T> array)
	{
		for (int num = array.Count; num > 1; num--)
		{
			int index = UnityEngine.Random.Range(0, num);
			T value = array[index];
			array[index] = array[num - 1];
			array[num - 1] = value;
		}
	}

	public static void Shuffle<T>(T[] array)
	{
		for (int num = array.Length; num > 1; num--)
		{
			int num2 = UnityEngine.Random.Range(0, num);
			T val = array[num2];
			array[num2] = array[num - 1];
			array[num - 1] = val;
		}
	}

	public static T CopyComponent<T>(T original, GameObject destination, bool forceAdd = false) where T : Component
	{
		Collider2D collider2D = original as Collider2D;
		bool flag = false;
		if (collider2D != null && collider2D.isTrigger)
		{
			flag = true;
		}
		Type type = original.GetType();
		Component component = ((!forceAdd) ? destination.GetComponent(type) : null);
		if (component == null)
		{
			component = destination.AddComponent(type) as T;
		}
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!fieldInfo.IsStatic)
			{
				fieldInfo.SetValue(component, fieldInfo.GetValue(original));
			}
		}
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.CanWrite && propertyInfo.CanRead && (!flag || !(propertyInfo.Name == "density")) && (!(type == typeof(CircleCollider2D)) || !(propertyInfo.Name == "usedByComposite")) && (!(type == typeof(CapsuleCollider2D)) || !(propertyInfo.Name == "usedByComposite")) && propertyInfo.CanWrite && !(propertyInfo.Name == "name"))
			{
				propertyInfo.SetValue(component, propertyInfo.GetValue(original, null), null);
			}
		}
		return component as T;
	}

	public static bool ComponentsEqual(Component component1, Component component2)
	{
		Type type = component1.GetType();
		FieldInfo[] fields = type.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.Name == "name" || fieldInfo.Name == "transform" || fieldInfo.IsStatic)
			{
				continue;
			}
			object value = fieldInfo.GetValue(component1);
			if (!(value.GetType() == typeof(UnityEngine.Object)))
			{
				object value2 = fieldInfo.GetValue(component2);
				if (value == fieldInfo.GetValue(component2))
				{
					Debug.Log("Diff on " + fieldInfo.Name + ". Val 1: " + value.ToString() + ". Val 2: " + value2.ToString());
					return false;
				}
			}
		}
		PropertyInfo[] properties = type.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.Name == "name" || propertyInfo.Name == "transform")
			{
				continue;
			}
			object value3 = propertyInfo.GetValue(component1, null);
			if (!(value3.GetType() == typeof(UnityEngine.Object)))
			{
				object value4 = propertyInfo.GetValue(component2, null);
				if (!value3.Equals(value4))
				{
					Debug.Log("Diff on " + propertyInfo.Name + ". Val 1: " + value3.ToString() + ". Val 2: " + value4.ToString());
					return false;
				}
			}
		}
		return true;
	}

	public static float ToDegrees(float rads)
	{
		return rads * 57.29578f;
	}

	public static float ToRadians(float degrees)
	{
		return degrees * ((float)Math.PI / 180f);
	}

	public static Color NormalizeRGB(float r, float g, float b, float a)
	{
		return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
	}

	public static Color NormalizeRGB(Vector4 color)
	{
		return NormalizeRGB(color.x, color.y, color.z, color.w);
	}

	public static Color NormalizeRGB(Color color)
	{
		return NormalizeRGB(color.r, color.g, color.b, color.a);
	}

	public static BoxCollider2D ConvertPolyToBoxCollider(PolygonCollider2D polyCollider, bool destroyPolyCollider)
	{
		GameObject gameObject = polyCollider.gameObject;
		Vector3 localEulerAngles = gameObject.transform.localEulerAngles;
		gameObject.transform.localEulerAngles = Vector3.zero;
		if (!CanPolyColliderBeBoxCollider(polyCollider))
		{
			gameObject.transform.localEulerAngles = localEulerAngles;
			return null;
		}
		BoxCollider2D boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
		boxCollider2D.size = polyCollider.bounds.size;
		boxCollider2D.offset = polyCollider.bounds.center - polyCollider.gameObject.transform.position;
		boxCollider2D.isTrigger = polyCollider.isTrigger;
		boxCollider2D.usedByComposite = polyCollider.usedByComposite;
		boxCollider2D.usedByEffector = polyCollider.usedByEffector;
		if ((!Application.isEditor || Application.isPlaying) && destroyPolyCollider)
		{
			UnityEngine.Object.DestroyImmediate(polyCollider, allowDestroyingAssets: true);
		}
		gameObject.transform.localEulerAngles = localEulerAngles;
		return boxCollider2D;
	}

	public static bool CanPolyColliderBeBoxCollider(PolygonCollider2D polyCollider)
	{
		if (polyCollider.GetTotalPointCount() != 4)
		{
			return false;
		}
		Vector2 vector = new Vector2(float.MinValue, float.MinValue);
		int num = 0;
		Vector2 vector2 = new Vector2(float.MinValue, float.MinValue);
		int num2 = 0;
		Vector2[] points = polyCollider.points;
		for (int i = 0; i < points.Length; i++)
		{
			Vector2 vector3 = points[i];
			if (vector.x != vector3.x && vector.y != vector3.x)
			{
				switch (num)
				{
				case 0:
					vector.x = vector3.x;
					break;
				case 1:
					vector.y = vector3.x;
					break;
				}
				num++;
			}
			if (vector2.x != vector3.y && vector2.y != vector3.y)
			{
				switch (num2)
				{
				case 0:
					vector2.x = vector3.y;
					break;
				case 1:
					vector2.y = vector3.y;
					break;
				}
				num2++;
			}
		}
		if (num != 2 || num2 != 2)
		{
			return false;
		}
		return true;
	}

	public static string ToRoman(int number)
	{
		if (number < 0 || number > 3999)
		{
			Debug.Log("Cannot return roman numberals for: " + number + ". Value must be between 1 and 3999");
			return string.Empty;
		}
		if (number < 1)
		{
			return string.Empty;
		}
		if (number >= 1000)
		{
			return "M" + ToRoman(number - 1000);
		}
		if (number >= 900)
		{
			return "CM" + ToRoman(number - 900);
		}
		if (number >= 500)
		{
			return "D" + ToRoman(number - 500);
		}
		if (number >= 400)
		{
			return "CD" + ToRoman(number - 400);
		}
		if (number >= 100)
		{
			return "C" + ToRoman(number - 100);
		}
		if (number >= 90)
		{
			return "XC" + ToRoman(number - 90);
		}
		if (number >= 50)
		{
			return "L" + ToRoman(number - 50);
		}
		if (number >= 40)
		{
			return "XL" + ToRoman(number - 40);
		}
		if (number >= 10)
		{
			return "X" + ToRoman(number - 10);
		}
		if (number >= 9)
		{
			return "IX" + ToRoman(number - 9);
		}
		if (number >= 5)
		{
			return "V" + ToRoman(number - 5);
		}
		if (number >= 4)
		{
			return "IV" + ToRoman(number - 4);
		}
		if (number >= 1)
		{
			return "I" + ToRoman(number - 1);
		}
		return string.Empty;
	}

	public static bool IsPercent(float value)
	{
		return Mathf.Floor(value) != value;
	}

	public static int ToPercent(float value)
	{
		return Mathf.RoundToInt(value * 100f);
	}

	public static bool DoCollisionTypesCollide_V2(CollisionType collidesWithType, GameObject other)
	{
		TagType tagType = TagType.Untagged;
		bool flag = false;
		foreach (KeyValuePair<string, TagType> item in m_tagToEnumTable)
		{
			if (other.CompareTag(item.Key))
			{
				flag = true;
				tagType = item.Value;
				break;
			}
		}
		if (!flag)
		{
			string tag = other.tag;
			tagType = TagType_RL.ToEnum(tag);
			m_tagToEnumTable.Add(tag, tagType);
		}
		return (CollisionType_RL.GetEquivalentCollisionType(tagType) & collidesWithType) != 0;
	}
}
