using System.Collections.Generic;
using Inner_Maps.Location_Structures;

public class PartyBanningComponent : PartyComponent
{
	public List<Character> bannedCharacters { get; private set; }

	public PartyBanningComponent()
	{
		bannedCharacters = new List<Character>();
	}

	public void BanCharacter(Character p_character)
	{
		if (!bannedCharacters.Contains(p_character))
		{
			bannedCharacters.Add(p_character);
		}
	}

	public void UnBanCharacter(Character p_character)
	{
		bannedCharacters.Remove(p_character);
	}

	public bool IsBanned(Character p_character)
	{
		return bannedCharacters.Contains(p_character);
	}

	public void Reset()
	{
		bannedCharacters.Clear();
	}

	public void LoadReferences(SaveDataPartyBanningComponent data)
	{
		if (data.bannedCharacters != null)
		{
			bannedCharacters.AddRange(SaveUtilities.ConvertIDListToCharacters(data.bannedCharacters));
		}
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		UnBanCharacter(p_character);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		bannedCharacters.Contains(p_character);
	}
}
