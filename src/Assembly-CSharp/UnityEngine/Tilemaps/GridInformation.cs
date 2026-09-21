using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Tilemaps;

[Serializable]
[AddComponentMenu("Tilemap/Grid Information")]
public class GridInformation : MonoBehaviour, ISerializationCallbackReceiver
{
	internal struct GridInformationValue
	{
		public GridInformationType type;

		public object data;
	}

	[Serializable]
	internal struct GridInformationKey
	{
		public Vector3Int position;

		public string name;
	}

	private Dictionary<GridInformationKey, GridInformationValue> m_PositionProperties = new Dictionary<GridInformationKey, GridInformationValue>();

	[SerializeField]
	[HideInInspector]
	private List<GridInformationKey> m_PositionIntKeys = new List<GridInformationKey>();

	[SerializeField]
	[HideInInspector]
	private List<int> m_PositionIntValues = new List<int>();

	[SerializeField]
	[HideInInspector]
	private List<GridInformationKey> m_PositionStringKeys = new List<GridInformationKey>();

	[SerializeField]
	[HideInInspector]
	private List<string> m_PositionStringValues = new List<string>();

	[SerializeField]
	[HideInInspector]
	private List<GridInformationKey> m_PositionFloatKeys = new List<GridInformationKey>();

	[SerializeField]
	[HideInInspector]
	private List<float> m_PositionFloatValues = new List<float>();

	[SerializeField]
	[HideInInspector]
	private List<GridInformationKey> m_PositionDoubleKeys = new List<GridInformationKey>();

	[SerializeField]
	[HideInInspector]
	private List<double> m_PositionDoubleValues = new List<double>();

	[SerializeField]
	[HideInInspector]
	private List<GridInformationKey> m_PositionObjectKeys = new List<GridInformationKey>();

	[SerializeField]
	[HideInInspector]
	private List<Object> m_PositionObjectValues = new List<Object>();

	[SerializeField]
	[HideInInspector]
	private List<GridInformationKey> m_PositionColorKeys = new List<GridInformationKey>();

	[SerializeField]
	[HideInInspector]
	private List<Color> m_PositionColorValues = new List<Color>();

	internal Dictionary<GridInformationKey, GridInformationValue> PositionProperties => m_PositionProperties;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (GetComponentInParent<Grid>() == null)
		{
			return;
		}
		m_PositionIntKeys.Clear();
		m_PositionIntValues.Clear();
		m_PositionStringKeys.Clear();
		m_PositionStringValues.Clear();
		m_PositionFloatKeys.Clear();
		m_PositionFloatValues.Clear();
		m_PositionDoubleKeys.Clear();
		m_PositionDoubleValues.Clear();
		m_PositionObjectKeys.Clear();
		m_PositionObjectValues.Clear();
		m_PositionColorKeys.Clear();
		m_PositionColorValues.Clear();
		foreach (KeyValuePair<GridInformationKey, GridInformationValue> positionProperty in m_PositionProperties)
		{
			switch (positionProperty.Value.type)
			{
			case GridInformationType.Integer:
				m_PositionIntKeys.Add(positionProperty.Key);
				m_PositionIntValues.Add((int)positionProperty.Value.data);
				break;
			case GridInformationType.String:
				m_PositionStringKeys.Add(positionProperty.Key);
				m_PositionStringValues.Add(positionProperty.Value.data as string);
				break;
			case GridInformationType.Float:
				m_PositionFloatKeys.Add(positionProperty.Key);
				m_PositionFloatValues.Add((float)positionProperty.Value.data);
				break;
			case GridInformationType.Double:
				m_PositionDoubleKeys.Add(positionProperty.Key);
				m_PositionDoubleValues.Add((double)positionProperty.Value.data);
				break;
			case GridInformationType.Color:
				m_PositionColorKeys.Add(positionProperty.Key);
				m_PositionColorValues.Add((Color)positionProperty.Value.data);
				break;
			default:
				m_PositionObjectKeys.Add(positionProperty.Key);
				m_PositionObjectValues.Add(positionProperty.Value.data as Object);
				break;
			}
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		m_PositionProperties.Clear();
		GridInformationValue value = default(GridInformationValue);
		for (int i = 0; i != Math.Min(m_PositionIntKeys.Count, m_PositionIntValues.Count); i++)
		{
			value.type = GridInformationType.Integer;
			value.data = m_PositionIntValues[i];
			m_PositionProperties.Add(m_PositionIntKeys[i], value);
		}
		GridInformationValue value2 = default(GridInformationValue);
		for (int j = 0; j != Math.Min(m_PositionStringKeys.Count, m_PositionStringValues.Count); j++)
		{
			value2.type = GridInformationType.String;
			value2.data = m_PositionStringValues[j];
			m_PositionProperties.Add(m_PositionStringKeys[j], value2);
		}
		GridInformationValue value3 = default(GridInformationValue);
		for (int k = 0; k != Math.Min(m_PositionFloatKeys.Count, m_PositionFloatValues.Count); k++)
		{
			value3.type = GridInformationType.Float;
			value3.data = m_PositionFloatValues[k];
			m_PositionProperties.Add(m_PositionFloatKeys[k], value3);
		}
		GridInformationValue value4 = default(GridInformationValue);
		for (int l = 0; l != Math.Min(m_PositionDoubleKeys.Count, m_PositionDoubleValues.Count); l++)
		{
			value4.type = GridInformationType.Double;
			value4.data = m_PositionDoubleValues[l];
			m_PositionProperties.Add(m_PositionDoubleKeys[l], value4);
		}
		GridInformationValue value5 = default(GridInformationValue);
		for (int m = 0; m != Math.Min(m_PositionObjectKeys.Count, m_PositionObjectValues.Count); m++)
		{
			value5.type = GridInformationType.UnityObject;
			value5.data = m_PositionObjectValues[m];
			m_PositionProperties.Add(m_PositionObjectKeys[m], value5);
		}
		GridInformationValue value6 = default(GridInformationValue);
		for (int n = 0; n != Math.Min(m_PositionColorKeys.Count, m_PositionColorValues.Count); n++)
		{
			value6.type = GridInformationType.Color;
			value6.data = m_PositionColorValues[n];
			m_PositionProperties.Add(m_PositionColorKeys[n], value6);
		}
	}

	public bool SetPositionProperty<T>(Vector3Int position, string name, T positionProperty)
	{
		throw new NotImplementedException("Storing this type is not accepted in GridInformation");
	}

	public bool SetPositionProperty(Vector3Int position, string name, int positionProperty)
	{
		return SetPositionProperty(position, name, GridInformationType.Integer, positionProperty);
	}

	public bool SetPositionProperty(Vector3Int position, string name, string positionProperty)
	{
		return SetPositionProperty(position, name, GridInformationType.String, positionProperty);
	}

	public bool SetPositionProperty(Vector3Int position, string name, float positionProperty)
	{
		return SetPositionProperty(position, name, GridInformationType.Float, positionProperty);
	}

	public bool SetPositionProperty(Vector3Int position, string name, double positionProperty)
	{
		return SetPositionProperty(position, name, GridInformationType.Double, positionProperty);
	}

	public bool SetPositionProperty(Vector3Int position, string name, Object positionProperty)
	{
		return SetPositionProperty(position, name, GridInformationType.UnityObject, positionProperty);
	}

	public bool SetPositionProperty(Vector3Int position, string name, Color positionProperty)
	{
		return SetPositionProperty(position, name, GridInformationType.Color, positionProperty);
	}

	private bool SetPositionProperty(Vector3Int position, string name, GridInformationType dataType, object positionProperty)
	{
		if (GetComponentInParent<Grid>() != null && positionProperty != null)
		{
			GridInformationKey key = default(GridInformationKey);
			key.position = position;
			key.name = name;
			GridInformationValue value = default(GridInformationValue);
			value.type = dataType;
			value.data = positionProperty;
			m_PositionProperties[key] = value;
			return true;
		}
		return false;
	}

	public T GetPositionProperty<T>(Vector3Int position, string name, T defaultValue) where T : Object
	{
		GridInformationKey key = default(GridInformationKey);
		key.position = position;
		key.name = name;
		if (m_PositionProperties.TryGetValue(key, out var value))
		{
			if (value.type != GridInformationType.UnityObject)
			{
				throw new InvalidCastException("Value stored in GridInformation is not of the right type");
			}
			return value.data as T;
		}
		return defaultValue;
	}

	public int GetPositionProperty(Vector3Int position, string name, int defaultValue)
	{
		GridInformationKey key = default(GridInformationKey);
		key.position = position;
		key.name = name;
		if (m_PositionProperties.TryGetValue(key, out var value))
		{
			if (value.type != GridInformationType.Integer)
			{
				throw new InvalidCastException("Value stored in GridInformation is not of the right type");
			}
			return (int)value.data;
		}
		return defaultValue;
	}

	public string GetPositionProperty(Vector3Int position, string name, string defaultValue)
	{
		GridInformationKey key = default(GridInformationKey);
		key.position = position;
		key.name = name;
		if (m_PositionProperties.TryGetValue(key, out var value))
		{
			if (value.type != GridInformationType.String)
			{
				throw new InvalidCastException("Value stored in GridInformation is not of the right type");
			}
			return (string)value.data;
		}
		return defaultValue;
	}

	public float GetPositionProperty(Vector3Int position, string name, float defaultValue)
	{
		GridInformationKey key = default(GridInformationKey);
		key.position = position;
		key.name = name;
		if (m_PositionProperties.TryGetValue(key, out var value))
		{
			if (value.type != GridInformationType.Float)
			{
				throw new InvalidCastException("Value stored in GridInformation is not of the right type");
			}
			return (float)value.data;
		}
		return defaultValue;
	}

	public double GetPositionProperty(Vector3Int position, string name, double defaultValue)
	{
		GridInformationKey key = default(GridInformationKey);
		key.position = position;
		key.name = name;
		if (m_PositionProperties.TryGetValue(key, out var value))
		{
			if (value.type != GridInformationType.Double)
			{
				throw new InvalidCastException("Value stored in GridInformation is not of the right type");
			}
			return (double)value.data;
		}
		return defaultValue;
	}

	public Color GetPositionProperty(Vector3Int position, string name, Color defaultValue)
	{
		GridInformationKey key = default(GridInformationKey);
		key.position = position;
		key.name = name;
		if (m_PositionProperties.TryGetValue(key, out var value))
		{
			if (value.type != GridInformationType.Color)
			{
				throw new InvalidCastException("Value stored in GridInformation is not of the right type");
			}
			return (Color)value.data;
		}
		return defaultValue;
	}

	public bool ErasePositionProperty(Vector3Int position, string name)
	{
		GridInformationKey key = default(GridInformationKey);
		key.position = position;
		key.name = name;
		return m_PositionProperties.Remove(key);
	}

	public virtual void Reset()
	{
		m_PositionProperties.Clear();
	}

	public Vector3Int[] GetAllPositions(string propertyName)
	{
		return (from x in m_PositionProperties.Keys.ToList().FindAll((GridInformationKey x) => x.name == propertyName)
			select x.position).ToArray();
	}
}
