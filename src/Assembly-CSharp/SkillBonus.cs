using System;
using System.Collections.Generic;

[Serializable]
public class SkillBonus
{
	public int maxHPBonus;

	public int attackBonus;

	public int critBonus;

	public List<string> canBecomeClasses = new List<string>();
}
