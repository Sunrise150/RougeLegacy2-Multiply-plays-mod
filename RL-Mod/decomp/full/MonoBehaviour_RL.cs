using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class MonoBehaviour_RL : MonoBehaviour
{
	public virtual void OnDestroy()
	{
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			Type fieldType = fieldInfo.FieldType;
			if (typeof(IList).IsAssignableFrom(fieldType) && fieldInfo.GetValue(this) is IList list)
			{
				list.Clear();
				IList list2 = null;
				return;
			}
			if (typeof(IDictionary).IsAssignableFrom(fieldType) && fieldInfo.GetValue(this) is IDictionary dictionary)
			{
				dictionary.Clear();
				IDictionary dictionary2 = null;
			}
			if (typeof(Array).IsAssignableFrom(fieldType) && fieldInfo.GetValue(this) is Array array)
			{
				Array.Clear(array, 0, array.Length);
				Array array2 = null;
			}
			if (!fieldType.IsPrimitive)
			{
				fieldInfo.SetValue(this, null);
			}
		}
		PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			Type propertyType = propertyInfo.PropertyType;
			if (typeof(IList).IsAssignableFrom(propertyType) && propertyInfo.GetValue(this, null) is IList list3)
			{
				list3.Clear();
				IList list4 = null;
				break;
			}
			if (typeof(IDictionary).IsAssignableFrom(propertyType) && propertyInfo.GetValue(this, null) is IDictionary dictionary3)
			{
				dictionary3.Clear();
				IDictionary dictionary4 = null;
			}
			if (typeof(Array).IsAssignableFrom(propertyType) && propertyInfo.GetValue(this, null) is Array array3)
			{
				Array.Clear(array3, 0, array3.Length);
				Array array4 = null;
			}
			if (!propertyType.IsPrimitive)
			{
				propertyInfo.SetValue(this, null, null);
			}
		}
	}
}
