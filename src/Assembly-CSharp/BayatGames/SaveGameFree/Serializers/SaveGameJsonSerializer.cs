using System;
using System.IO;
using System.Text;
using FullSerializer;
using UnityEngine;

namespace BayatGames.SaveGameFree.Serializers;

public class SaveGameJsonSerializer : ISaveGameSerializer
{
	public void Serialize<T>(T obj, Stream stream, Encoding encoding)
	{
		try
		{
			StreamWriter streamWriter = new StreamWriter(stream, encoding);
			fsSerializer fsSerializer = new fsSerializer();
			fsData data = new fsData();
			fsSerializer.TrySerialize(obj, out data);
			streamWriter.Write(fsJsonPrinter.CompressedJson(data));
			streamWriter.Dispose();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public T Deserialize<T>(Stream stream, Encoding encoding)
	{
		T instance = default(T);
		try
		{
			StreamReader streamReader = new StreamReader(stream, encoding);
			fsSerializer fsSerializer = new fsSerializer();
			fsData data = fsJsonParser.Parse(streamReader.ReadToEnd());
			fsSerializer.TryDeserialize(data, ref instance);
			if (instance == null)
			{
				instance = default(T);
			}
			streamReader.Dispose();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return instance;
	}
}
