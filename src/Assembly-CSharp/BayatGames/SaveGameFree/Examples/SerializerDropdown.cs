using System.Collections.Generic;
using BayatGames.SaveGameFree.Serializers;
using UnityEngine;
using UnityEngine.UI;

namespace BayatGames.SaveGameFree.Examples;

public class SerializerDropdown : Dropdown
{
	private static SerializerDropdown m_Singleton;

	private static ISaveGameSerializer[] m_Serializers = new ISaveGameSerializer[3]
	{
		new SaveGameXmlSerializer(),
		new SaveGameJsonSerializer(),
		new SaveGameBinarySerializer()
	};

	protected ISaveGameSerializer m_ActiveSerializer;

	public static SerializerDropdown Singleton => m_Singleton;

	public ISaveGameSerializer ActiveSerializer
	{
		get
		{
			if (m_ActiveSerializer == null)
			{
				m_ActiveSerializer = new SaveGameJsonSerializer();
			}
			return m_ActiveSerializer;
		}
	}

	protected override void Awake()
	{
		if (m_Singleton != null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		m_Singleton = this;
		base.Awake();
		base.options = new List<OptionData>
		{
			new OptionData("XML"),
			new OptionData("JSON"),
			new OptionData("Binary")
		};
		base.onValueChanged.AddListener(OnValueChanged);
		base.value = SaveGame.Load("serializer", 0, new SaveGameJsonSerializer());
	}

	protected virtual void OnValueChanged(int index)
	{
		m_ActiveSerializer = m_Serializers[index];
	}

	protected virtual void OnApplicationQuit()
	{
		SaveGame.Save("serializer", base.value, new SaveGameJsonSerializer());
	}
}
