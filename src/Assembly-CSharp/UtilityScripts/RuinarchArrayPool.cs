using Pathfinding.Util;

namespace UtilityScripts;

public static class RuinarchArrayPool<T>
{
	public static T[] Claim(int length)
	{
		return ArrayPool<T>.Claim(length);
	}

	public static T[] ClaimWithExactLength(int length)
	{
		return ArrayPool<T>.ClaimWithExactLength(length);
	}

	public static void Release(T[] array)
	{
		ArrayPool<T>.Release(ref array);
	}

	public static void ReleaseWithExactLength(T[] array)
	{
		ArrayPool<T>.Release(ref array, allowNonPowerOfTwo: true);
	}
}
