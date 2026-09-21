using System.Collections.Generic;

public class SaveDataPartyBanningComponent : SaveData<PartyBanningComponent>
{
	public List<string> bannedCharacters;

	public override void Save(PartyBanningComponent data)
	{
		if (data.bannedCharacters != null)
		{
			bannedCharacters = SaveUtilities.ConvertSavableListToIDs(data.bannedCharacters);
		}
	}

	public override PartyBanningComponent Load()
	{
		return new PartyBanningComponent();
	}
}
