public class PartyComponent
{
	public Party owner { get; private set; }

	public void SetOwner(Party owner)
	{
		this.owner = owner;
	}
}
