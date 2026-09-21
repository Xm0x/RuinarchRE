public struct CombatReaction
{
	public COMBAT_REACTION reaction;

	public string reason;

	public CombatReaction(COMBAT_REACTION reaction, string reason = "")
	{
		this.reaction = reaction;
		this.reason = reason;
	}
}
