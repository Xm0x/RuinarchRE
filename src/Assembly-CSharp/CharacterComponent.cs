public class CharacterComponent
{
	public Character owner { get; private set; }

	public virtual void SetOwner(Character owner)
	{
		this.owner = owner;
	}

	public virtual void CleanUp()
	{
		SetOwner(null);
	}

	public override string ToString()
	{
		return $"{owner?.name} - {GetType()}";
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = owner;
	}
}
