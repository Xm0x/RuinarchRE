using System.Collections.Generic;
using System.Runtime.Serialization;

public class SerializableDictionary<TKey, TValue> : SerializableDictionaryBase<TKey, TValue, TValue>
{
	public SerializableDictionary()
	{
	}

	public SerializableDictionary(IDictionary<TKey, TValue> dict)
		: base(dict)
	{
	}

	protected SerializableDictionary(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	protected override TValue GetValue(TValue[] storage, int i)
	{
		return storage[i];
	}

	protected override void SetValue(TValue[] storage, int i, TValue value)
	{
		storage[i] = value;
	}
}
public static class SerializableDictionary
{
	public class Storage<T>
	{
		public T data;
	}
}
public class SerializableDictionary<TKey, TValue, TValueStorage> : SerializableDictionaryBase<TKey, TValue, TValueStorage> where TValueStorage : SerializableDictionary.Storage<TValue>, new()
{
	public SerializableDictionary()
	{
	}

	public SerializableDictionary(IDictionary<TKey, TValue> dict)
		: base(dict)
	{
	}

	protected SerializableDictionary(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	protected override TValue GetValue(TValueStorage[] storage, int i)
	{
		return storage[i].data;
	}

	protected override void SetValue(TValueStorage[] storage, int i, TValue value)
	{
		storage[i] = new TValueStorage();
		storage[i].data = value;
	}
}
