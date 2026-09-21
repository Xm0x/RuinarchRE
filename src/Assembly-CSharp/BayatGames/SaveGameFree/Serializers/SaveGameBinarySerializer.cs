using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace BayatGames.SaveGameFree.Serializers;

public class SaveGameBinarySerializer : ISaveGameSerializer
{
	public void Serialize<T>(T obj, Stream stream, Encoding encoding)
	{
		try
		{
			new BinaryFormatter().Serialize(stream, obj);
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
			result = (T)new BinaryFormatter().Deserialize(stream);
			return result;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return result;
	}
}
