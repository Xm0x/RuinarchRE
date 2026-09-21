using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public static class ObjectCopier
{
	public static T Clone<T>(T source)
	{
		if (!typeof(T).IsSerializable)
		{
			throw new ArgumentException("The type must be serializable.", "source");
		}
		if (source == null)
		{
			return default(T);
		}
		IFormatter formatter = new BinaryFormatter();
		Stream stream = new MemoryStream();
		using (stream)
		{
			formatter.Serialize(stream, source);
			stream.Seek(0L, SeekOrigin.Begin);
			return (T)formatter.Deserialize(stream);
		}
	}
}
