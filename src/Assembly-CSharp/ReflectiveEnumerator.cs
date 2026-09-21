using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ReflectiveEnumerator
{
	static ReflectiveEnumerator()
	{
	}

	public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class
	{
		List<T> list = new List<T>();
		foreach (Type item in from myType in Assembly.GetAssembly(typeof(T)).GetTypes()
			where myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))
			select myType)
		{
			list.Add((T)Activator.CreateInstance(item, constructorArgs));
		}
		return list;
	}
}
