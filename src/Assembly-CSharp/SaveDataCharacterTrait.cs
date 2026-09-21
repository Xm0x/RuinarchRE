using System.Collections.Generic;
using System.Linq;
using Traits;

public class SaveDataCharacterTrait : SaveDataTrait
{
	public List<string> alreadyInspectedTileObjects;

	public List<string> charactersAlreadySawForHope;

	public List<string> charactersThatHaveReactedToThis;

	public List<string> alreadyReactedFoodPiles;

	public bool hasBeenAbductedByPlayerMonster;

	public bool hasBeenAbductedByWildMonster;

	public Dictionary<string, List<string>> traitsFromOtherCharacterThatThisIsAwareOf;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		CharacterTrait characterTrait = trait as CharacterTrait;
		alreadyInspectedTileObjects = SaveUtilities.ConvertSavableListToIDs(characterTrait.alreadyInspectedTileObjects);
		charactersAlreadySawForHope = SaveUtilities.ConvertSavableListToIDs(characterTrait.charactersAlreadySawForHope);
		charactersThatHaveReactedToThis = SaveUtilities.ConvertSavableListToIDs(characterTrait.charactersThatHaveReactedToThis.ToList());
		alreadyReactedFoodPiles = SaveUtilities.ConvertSavableListToIDs(characterTrait.alreadyReactedToFoodPiles);
		hasBeenAbductedByPlayerMonster = characterTrait.hasBeenAbductedByPlayerMonster;
		hasBeenAbductedByWildMonster = characterTrait.hasBeenAbductedByWildMonster;
		traitsFromOtherCharacterThatThisIsAwareOf = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<Character, List<string>> item in characterTrait.traitsFromOtherCharacterThatThisIsAwareOf)
		{
			string key = item.Key.persistentID;
			List<string> value = item.Value;
			traitsFromOtherCharacterThatThisIsAwareOf.Add(key, value);
		}
	}
}
