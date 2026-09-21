public class SaveDataNewcomerOpinionModifier : SaveDataSharedOpinionModifier
{
	public override SharedOpinionModifier Load()
	{
		return new NewcomerOpinionModifier(this);
	}
}
