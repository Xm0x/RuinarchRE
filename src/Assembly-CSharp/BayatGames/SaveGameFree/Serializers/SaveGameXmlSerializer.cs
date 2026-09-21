using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace BayatGames.SaveGameFree.Serializers;

public class SaveGameXmlSerializer : ISaveGameSerializer
{
	public void Serialize<T>(T obj, Stream stream, Encoding encoding)
	{
		try
		{
			new XmlSerializer(typeof(T)).Serialize(stream, obj);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public T Deserialize<T>(Stream stream, Encoding encoding)
	{
		T result = default(T);
		try
		{
			result = (T)new XmlSerializer(typeof(T)).Deserialize(stream);
			return result;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return result;
	}
}
