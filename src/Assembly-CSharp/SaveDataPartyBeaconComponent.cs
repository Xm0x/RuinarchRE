public class SaveDataPartyBeaconComponent : SaveData<PartyBeaconComponent>
{
	public string currentBeaconCharacter;

	public override void Save(PartyBeaconComponent data)
	{
		if (data.currentBeaconCharacter != null)
		{
			currentBeaconCharacter = data.currentBeaconCharacter.persistentID;
		}
	}
}
