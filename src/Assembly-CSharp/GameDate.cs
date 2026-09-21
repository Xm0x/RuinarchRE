using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

[Serializable]
public struct GameDate
{
	public int month;

	public int day;

	public int year;

	public int tick;

	public bool hasValue;

	public GameDate(int month, int day, int year, int tick)
	{
		this.month = month;
		this.day = day;
		this.year = year;
		this.tick = tick;
		hasValue = true;
	}

	public GameDate AddTicks(int amount)
	{
		tick += amount;
		while (tick > 480)
		{
			tick -= 480;
			AddDays(1);
		}
		return this;
	}

	public GameDate AddDays(int amount)
	{
		day += amount;
		int num = 0;
		while (day > 30)
		{
			day -= 30;
			num++;
		}
		if (num > 0)
		{
			AddMonths(num);
		}
		return this;
	}

	public void AddMonths(int amount)
	{
		month += amount;
		while (month > 12)
		{
			month -= 12;
			AddYears(1);
		}
		if (day > 30)
		{
			day = 30;
		}
	}

	public void AddYears(int amount)
	{
		year += amount;
	}

	public void ReduceTicks(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			tick--;
			if (tick <= 0)
			{
				ReduceDays(1);
				tick = 480;
			}
		}
	}

	public void ReduceDays(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			day--;
			if (day == 0)
			{
				ReduceMonth(1);
				day = 30;
			}
		}
	}

	public void ReduceMonth(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			month--;
			if (month == 0)
			{
				month = 12;
				ReduceYear(1);
			}
		}
	}

	public void ReduceYear(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			year--;
		}
	}

	public void SetDate(int month, int day, int year, int tick)
	{
		this.month = month;
		this.day = day;
		this.year = year;
		this.tick = tick;
	}

	public void SetTicks(int tick)
	{
		this.tick = tick;
	}

	public void SetDate(GameDate gameDate)
	{
		month = gameDate.month;
		day = gameDate.day;
		year = gameDate.year;
		tick = gameDate.tick;
	}

	public bool IsSameDate(int month, int day, int year, int tick)
	{
		if (this.month == month && this.day == day && this.year == year && this.tick == tick)
		{
			return true;
		}
		return false;
	}

	public bool IsSameDate(GameDate gameDate)
	{
		if (month == gameDate.month && day == gameDate.day && year == gameDate.year && tick == gameDate.tick)
		{
			return true;
		}
		return false;
	}

	public bool IsBefore(GameDate otherDate)
	{
		if (year < otherDate.year)
		{
			return true;
		}
		if (year == otherDate.year)
		{
			if (month < otherDate.month)
			{
				return true;
			}
			if (month == otherDate.month)
			{
				if (day < otherDate.day)
				{
					return true;
				}
				if (day == otherDate.day)
				{
					if (tick < otherDate.tick)
					{
						return true;
					}
					_ = tick;
					_ = otherDate.tick;
					return false;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	public bool IsAfter(GameDate otherDate)
	{
		if (year < otherDate.year)
		{
			return false;
		}
		if (year == otherDate.year)
		{
			if (month < otherDate.month)
			{
				return false;
			}
			if (month == otherDate.month)
			{
				if (day < otherDate.day)
				{
					return false;
				}
				if (day == otherDate.day)
				{
					if (tick < otherDate.tick)
					{
						return false;
					}
					if (tick == otherDate.tick)
					{
						return false;
					}
					return true;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	public string ToStringDate()
	{
		string[] array = new string[7];
		MONTH mONTH = (MONTH)month;
		array[0] = mONTH.ToString();
		array[1] = " ";
		array[2] = day.ToString();
		array[3] = ", ";
		array[4] = year.ToString();
		array[5] = " T: ";
		array[6] = tick.ToString();
		return string.Concat(array);
	}

	public override string ToString()
	{
		return ConvertToContinuousDaysWithTime();
	}

	public int ConvertToContinuousDays()
	{
		int num = 0;
		if (year > GameManager.Instance.startYear)
		{
			int num2 = year - GameManager.Instance.startYear;
			num += num2 * 12 * 30;
		}
		return num + ((month - 1) * 30 + day);
	}

	public int Sum()
	{
		return ConvertToContinuousDays() + tick;
	}

	public int GetHourDifference(GameDate otherDate)
	{
		int num = Math.Abs(year - otherDate.year);
		int num2 = Math.Abs(month - otherDate.month);
		int num3 = Math.Abs(tick - otherDate.tick);
		int num4 = num * 172800;
		int num5 = num2 * 14400;
		int ticks = num4 + num5 + num3;
		return GameManager.Instance.GetHoursBasedOnTicks(ticks);
	}

	public int GetTickDifference(GameDate otherDate)
	{
		int num = Math.Abs(ConvertToContinuousDays() - otherDate.ConvertToContinuousDays());
		int num2 = Math.Abs(tick - otherDate.tick);
		int num3 = num * 480;
		int num4 = 0;
		if (num > 0)
		{
			if (tick < otherDate.tick)
			{
				return num4 + (num3 + num2);
			}
			return num4 + (num3 - num2);
		}
		return num4 + num2;
	}

	public int GetTickDifferenceNonAbsoluteOrZeroIfReached(GameDate otherDate)
	{
		int num = otherDate.year - year;
		int num2 = otherDate.month - month;
		int num3 = otherDate.day - day;
		int num4 = Mathf.Abs(otherDate.tick - tick);
		int num5 = num * 172800;
		int num6 = num2 * 14400;
		int num7 = num3 * 480;
		if (num3 <= -1)
		{
			return 0;
		}
		if (num3 <= 0 && tick >= otherDate.tick)
		{
			return 0;
		}
		int num8 = num5 + num6;
		if (num3 > 0)
		{
			if (tick < otherDate.tick)
			{
				return num8 + (num7 + num4);
			}
			return num8 + (num7 - num4);
		}
		return num8 + num4;
	}

	public string GetTimeDifferenceString(GameDate otherDate)
	{
		int tickDifference = GetTickDifference(otherDate);
		if (tickDifference >= 20)
		{
			int hoursBasedOnTicks = GameManager.Instance.GetHoursBasedOnTicks(tickDifference);
			if (hoursBasedOnTicks > 1)
			{
				return hoursBasedOnTicks + " " + LocalizationManager.Hours;
			}
			return hoursBasedOnTicks + " " + LocalizationManager.Hour;
		}
		int minutesBasedOnTicks = GameManager.Instance.GetMinutesBasedOnTicks(tickDifference);
		if (minutesBasedOnTicks > 1)
		{
			return minutesBasedOnTicks + " " + LocalizationManager.Minutes;
		}
		return minutesBasedOnTicks + " " + LocalizationManager.Minute;
	}

	public string ConvertToContinuousDaysWithTime(bool nextLineTime = false, bool capitalizedDay = false)
	{
		if (!hasValue)
		{
			return "???";
		}
		string text = (capitalizedDay ? LocalizationManager.Capitalized_Day : LocalizationManager.Day);
		if (LocalizationSettings.SelectedLocale.Identifier.Code == "ja")
		{
			if (nextLineTime)
			{
				return $"{ConvertToContinuousDays()} {text}\n{ConvertToTime()}";
			}
			return $"{ConvertToContinuousDays()} {text} {ConvertToTime()}";
		}
		if (nextLineTime)
		{
			return $"{text} {ConvertToContinuousDays()}\n{ConvertToTime()}";
		}
		return $"{text} {ConvertToContinuousDays()} {ConvertToTime()}";
	}

	public string ConvertToTime()
	{
		return GameManager.Instance.ConvertTickToTime(tick) ?? "";
	}

	public static bool operator ==(GameDate left, GameDate right)
	{
		if (left.month == right.month && left.day == right.day && left.year == right.year && left.tick == right.tick)
		{
			return left.hasValue == right.hasValue;
		}
		return false;
	}

	public static bool operator !=(GameDate left, GameDate right)
	{
		if (left.month == right.month && left.day == right.day && left.year == right.year && left.tick == right.tick)
		{
			return left.hasValue != right.hasValue;
		}
		return true;
	}
}
