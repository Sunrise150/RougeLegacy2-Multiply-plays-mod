using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class CDGExtensions
{
	private static List<Transform> m_recursiveLayersHelper = new List<Transform>();

	public static Transform FindDeep(this Transform aParent, string aName)
	{
		Transform transform = aParent.Find(aName);
		if (transform != null)
		{
			return transform;
		}
		foreach (Transform item in aParent)
		{
			transform = item.FindDeep(aName);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public static void FindAllTransforms(this Transform parent, List<Transform> outputList)
	{
		for (int i = 0; i < parent.childCount; i++)
		{
			Transform child = parent.GetChild(i);
			outputList.Add(child);
			child.FindAllTransforms(outputList);
		}
	}

	public static string FullHierarchyPath(this GameObject gameObject)
	{
		string text = gameObject.name;
		Transform parent = gameObject.transform.parent;
		while (parent != null)
		{
			text = parent.name + "/" + text;
			parent = parent.parent;
		}
		return text;
	}

	public static GameObject FindObjectReference(this GameObject gameObject, string id)
	{
		ObjectReferenceFinder component = gameObject.GetComponent<ObjectReferenceFinder>();
		if ((bool)component)
		{
			return component.GetObject(id);
		}
		return null;
	}

	public static T FindObjectReference<T>(this GameObject gameObject, string id, bool searchAllChildren = false, bool includeInactive = false) where T : UnityEngine.Object
	{
		ObjectReferenceFinder component = gameObject.GetComponent<ObjectReferenceFinder>();
		if ((bool)component)
		{
			return component.GetObject<T>(id, searchAllChildren, includeInactive);
		}
		return null;
	}

	public static bool Intersects(this Rect r1, Rect r2, out Rect area)
	{
		area = default(Rect);
		if (r2.Overlaps(r1))
		{
			float num = Mathf.Min(r1.xMax, r2.xMax);
			float num2 = Mathf.Max(r1.xMin, r2.xMin);
			float num3 = Mathf.Min(r1.yMax, r2.yMax);
			float num4 = Mathf.Max(r1.yMin, r2.yMin);
			area.x = Mathf.Min(num, num2);
			area.y = Mathf.Min(num3, num4);
			area.width = Mathf.Max(0f, num - num2);
			area.height = Mathf.Max(0f, num3 - num4);
			return true;
		}
		return false;
	}

	public static GameObject GetRoot(this Component obj, bool includeInactive = false)
	{
		return obj.gameObject.GetRoot(includeInactive);
	}

	public static GameObject GetRoot(this GameObject obj, bool includeInactive = false)
	{
		if (obj.transform.parent == null)
		{
			return obj;
		}
		GameObject result = obj;
		IRootObj componentInParent = obj.GetComponentInParent<IRootObj>(includeInactive);
		if (componentInParent != null)
		{
			result = componentInParent.gameObject;
		}
		return result;
	}

	public static bool Contains(this LayerMask mask, int layer)
	{
		return (mask.value & (1 << layer)) > 0;
	}

	public static bool Contains(this LayerMask mask, GameObject gameobject)
	{
		return (mask.value & (1 << gameobject.layer)) > 0;
	}

	public static void SetLayerRecursively(this GameObject gameObject, LayerMask layer, bool includeInactive = false)
	{
		m_recursiveLayersHelper.Clear();
		gameObject.GetComponentsInChildren(includeInactive, m_recursiveLayersHelper);
		foreach (Transform item in m_recursiveLayersHelper)
		{
			item.gameObject.layer = layer;
		}
	}

	public static void SetAlpha(this Image image, float alpha)
	{
		Color color = image.color;
		color.a = alpha;
		image.color = color;
	}

	public static void SetAlpha(this RawImage image, float alpha)
	{
		Color color = image.color;
		color.a = alpha;
		image.color = color;
	}

	public static void SetAlpha(this SpriteRenderer image, float alpha)
	{
		Color color = image.color;
		color.a = alpha;
		image.color = color;
	}

	public static T[] InsertAt<T>(this T[] source, int index, T element)
	{
		T[] array = new T[source.Length + 1];
		for (int i = 0; i < source.Length; i++)
		{
			if (i < index)
			{
				array[i] = source[i];
			}
			else if (i >= index)
			{
				array[i + 1] = source[i];
			}
		}
		array[index] = element;
		return array;
	}

	public static T[] RemoveAt<T>(this T[] source, int index)
	{
		T[] array = new T[source.Length - 1];
		if (index > 0)
		{
			Array.Copy(source, 0, array, 0, index);
		}
		if (index < source.Length - 1)
		{
			Array.Copy(source, index + 1, array, index, source.Length - index - 1);
		}
		return array;
	}

	public static T[] Remove<T>(this T[] source, T element)
	{
		int num = source.IndexOf(element);
		if (num > -1)
		{
			source = source.RemoveAt(num);
		}
		return source;
	}

	public static T[] Add<T>(this T[] source, T element)
	{
		T[] array = new T[source.Length + 1];
		for (int i = 0; i < source.Length; i++)
		{
			array[i] = source[i];
		}
		array[array.Length - 1] = element;
		return array;
	}

	public static int IndexOf<T>(this T[] source, T element)
	{
		for (int i = 0; i < source.Length; i++)
		{
			if (source[i].Equals(element))
			{
				return i;
			}
		}
		return -1;
	}

	public static bool IsNativeNull(this object obj)
	{
		return obj?.Equals(null) ?? true;
	}

	public static string ToCIString(this float value)
	{
		return value.ToString(LocalizationManager.GetCurrentCultureInfo());
	}
}
