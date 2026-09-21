public class NPCSettlementComponent
{
	public NPCSettlement owner { get; private set; }

	public void SetOwner(NPCSettlement owner)
	{
		this.owner = owner;
	}
}
