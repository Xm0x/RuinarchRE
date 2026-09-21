using System.Collections.Generic;

public class CharacterFactionComparer : IComparer<Character>
{
	public int Compare(Character x, Character y)
	{
		return x.faction.id.CompareTo(y.faction.id);
	}
}
