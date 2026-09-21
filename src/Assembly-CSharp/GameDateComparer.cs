using System.Collections.Generic;

public class GameDateComparer : IEqualityComparer<GameDate>
{
	public bool Equals(GameDate x, GameDate y)
	{
		if (x.year == y.year && x.month == y.month && x.day == y.day && x.tick == y.tick)
		{
			return true;
		}
		return false;
	}

	public int GetHashCode(GameDate obj)
	{
		return obj.GetHashCode();
	}
}
