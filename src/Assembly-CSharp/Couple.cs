using System;

public class Couple : IEquatable<Couple>
{
	public PreCharacterData character1 { get; }

	public PreCharacterData character2 { get; }

	public Couple(PreCharacterData _character1, PreCharacterData _character2)
	{
		character1 = _character1;
		character2 = _character2;
	}

	public bool Equals(Couple other)
	{
		if (other == null)
		{
			return false;
		}
		if (character1.id != other.character1.id || character2.id != other.character2.id)
		{
			if (character1.id == other.character2.id)
			{
				return character2.id == other.character1.id;
			}
			return false;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as Couple);
	}

	public override int GetHashCode()
	{
		return character1.id + character2.id;
	}
}
