public class AreaComponent
{
	public Area owner { get; private set; }

	public void SetOwner(Area owner)
	{
		this.owner = owner;
	}

	public override string ToString()
	{
		return $"{owner?.name} - {GetType()}";
	}
}
