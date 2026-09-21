using System.Diagnostics;

namespace UtilityScripts;

public static class MaccimaProfiler
{
	[Conditional("DEBUG_PROFILER")]
	public static void BeginSample(string message)
	{
	}

	[Conditional("DEBUG_PROFILER")]
	public static void EndSample()
	{
	}
}
