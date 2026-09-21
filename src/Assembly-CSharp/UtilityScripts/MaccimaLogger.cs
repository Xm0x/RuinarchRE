using System.Diagnostics;
using UnityEngine;

namespace UtilityScripts;

public static class MaccimaLogger
{
	[Conditional("DEBUG_LOG")]
	public static void ConcatenateString(ref string p_str, string p_concat)
	{
		p_str += p_concat;
	}

	[Conditional("DEBUG_LOG")]
	public static void Log(string message)
	{
		UnityEngine.Debug.Log(message);
	}

	[Conditional("DEBUG_LOG")]
	public static void LogWarning(string message)
	{
		UnityEngine.Debug.LogWarning(message);
	}

	[Conditional("DEBUG_LOG")]
	public static void LogError(string message)
	{
		UnityEngine.Debug.LogError(message);
	}

	[Conditional("DEBUG_LOG")]
	public static void LogReferenceError(string p_objName, string p_referencerName)
	{
		UnityEngine.Debug.LogError(p_objName + " is still referenced in " + p_referencerName);
	}

	[Conditional("DEBUG_LOG")]
	public static void LogReferenceError(string p_objName, string p_referencerName, string p_referencerPersistentID)
	{
		UnityEngine.Debug.LogError(p_objName + " is still referenced in " + p_referencerName + " (PID: " + p_referencerPersistentID + ")");
	}

	[Conditional("ASSERTIONS")]
	public static void AssertIsNotNull(object p_obj, string p_message)
	{
	}

	[Conditional("ASSERTIONS")]
	public static void AssertIsNotNull(object p_obj)
	{
	}

	[Conditional("ASSERTIONS")]
	public static void AssertIsTrue(bool b, string s)
	{
	}

	[Conditional("ASSERTIONS")]
	public static void AssertIsFalse(bool b, string s)
	{
	}
}
