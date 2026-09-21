using System.Collections.Generic;
using UnityEngine;

public class SerializableDictionaryExample : MonoBehaviour
{
	[SerializeField]
	private StringStringDictionary m_stringStringDictionary;

	public ObjectColorDictionary m_objectColorDictionary;

	public StringColorArrayDictionary m_objectColorArrayDictionary;

	public IDictionary<string, string> StringStringDictionary
	{
		get
		{
			return m_stringStringDictionary;
		}
		set
		{
			m_stringStringDictionary.CopyFrom(value);
		}
	}

	private void Reset()
	{
		StringStringDictionary = new Dictionary<string, string>
		{
			{ "first key", "value A" },
			{ "second key", "value B" },
			{ "third key", "value C" }
		};
		m_objectColorDictionary = new ObjectColorDictionary
		{
			{
				base.gameObject,
				Color.blue
			},
			{
				this,
				Color.red
			}
		};
	}
}
