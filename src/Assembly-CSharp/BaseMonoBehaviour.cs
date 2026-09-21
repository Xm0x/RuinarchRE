using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class BaseMonoBehaviour : MonoBehaviour
{
	protected virtual void OnDestroy()
	{
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			Type fieldType = fieldInfo.FieldType;
			if (typeof(IList).IsAssignableFrom(fieldType) && fieldInfo.GetValue(this) is IList list)
			{
				list.Clear();
			}
			if (typeof(IDictionary).IsAssignableFrom(fieldType) && fieldInfo.GetValue(this) is IDictionary dictionary)
			{
				dictionary.Clear();
			}
			if (!fieldType.IsPrimitive)
			{
				fieldInfo.SetValue(this, null);
			}
		}
	}
}
