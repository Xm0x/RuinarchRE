public struct DateRange
{
	public GameDate startDate;

	public GameDate endDate;

	public int rangeInTicks => GetRangeInTicks();

	public DateRange(GameDate startDate, GameDate endDate)
	{
		this.startDate = startDate;
		this.endDate = endDate;
	}

	public bool IsDateInRange(GameDate date)
	{
		if (date.IsSameDate(startDate) || date.IsSameDate(endDate))
		{
			return true;
		}
		if (date.IsAfter(startDate) && date.IsBefore(endDate))
		{
			return true;
		}
		return false;
	}

	public bool HasConflictWith(DateRange other)
	{
		if (IsDateInRange(other.startDate) || IsDateInRange(other.endDate))
		{
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return startDate.ConvertToContinuousDaysWithTime() + " - " + endDate.ConvertToContinuousDaysWithTime();
	}

	private int GetRangeInTicks()
	{
		int num = 0;
		GameDate gameDate;
		GameDate gameDate2;
		if (startDate.IsBefore(endDate))
		{
			gameDate = startDate;
			gameDate2 = endDate;
		}
		else
		{
			gameDate = endDate;
			gameDate2 = startDate;
		}
		while (!gameDate.IsSameDate(gameDate2))
		{
			gameDate.AddDays(1);
			num++;
		}
		return num;
	}
}
