using System;

public struct SignalMethod
{
	public string methodName;

	public Type objectType;

	public bool Equals(Delegate d)
	{
		if (d.Method.Name.Contains(methodName) && (d.Target.GetType() == objectType || d.Target.GetType().BaseType == objectType))
		{
			return true;
		}
		if (string.IsNullOrEmpty(methodName) && (d.Target.GetType() == objectType || d.Target.GetType().BaseType == objectType))
		{
			return true;
		}
		return false;
	}
}
