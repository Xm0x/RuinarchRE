using System;

[Serializable]
public class BuffStatsBonus
{
	public int strength;

	public int intelligence;

	public void SetStrength(int p_amount)
	{
		strength = p_amount;
	}

	public void SetIntelligence(int p_amount)
	{
		intelligence = p_amount;
	}

	public void Reset()
	{
		strength = 0;
		intelligence = 0;
	}
}
