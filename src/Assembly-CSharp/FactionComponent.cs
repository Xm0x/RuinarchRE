public class FactionComponent
{
	public Faction owner { get; private set; }

	public void SetOwner(Faction owner)
	{
		this.owner = owner;
	}

	public override string ToString()
	{
		return owner?.name + " - " + GetType().ToString();
	}
}
