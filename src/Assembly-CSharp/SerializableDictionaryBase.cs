using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

public abstract class SerializableDictionaryBase<TKey, TValue, TValueStorage> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField]
	private TKey[] m_keys;

	[SerializeField]
	private TValueStorage[] m_values;

	public SerializableDictionaryBase()
	{
	}

	public SerializableDictionaryBase(IDictionary<TKey, TValue> dict)
		: base(dict.Count)
	{
		foreach (KeyValuePair<TKey, TValue> item in dict)
		{
			base[item.Key] = item.Value;
		}
	}

	protected SerializableDictionaryBase(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	protected abstract void SetValue(TValueStorage[] storage, int i, TValue value);

	protected abstract TValue GetValue(TValueStorage[] storage, int i);

	public void CopyFrom(IDictionary<TKey, TValue> dict)
	{
		Clear();
		foreach (KeyValuePair<TKey, TValue> item in dict)
		{
			base[item.Key] = item.Value;
		}
	}

	public void OnAfterDeserialize()
	{
		if (m_keys != null && m_values != null && m_keys.Length == m_values.Length)
		{
			Clear();
			int num = m_keys.Length;
			for (int i = 0; i < num; i++)
			{
				base[m_keys[i]] = GetValue(m_values, i);
			}
			m_keys = null;
			m_values = null;
		}
	}

	public void OnBeforeSerialize()
	{
		int num = base.Count;
		m_keys = new TKey[num];
		m_values = new TValueStorage[num];
		int num2 = 0;
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TKey, TValue> current = enumerator.Current;
			m_keys[num2] = current.Key;
			SetValue(m_values, num2, current.Value);
			num2++;
		}
	}
}
