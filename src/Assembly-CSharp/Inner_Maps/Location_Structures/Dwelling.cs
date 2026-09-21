using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Dwelling : ManMadeStructure
{
	private static readonly TILE_OBJECT_TYPE[] _preplacedObjectsToIgnoreWhenBuilding = new TILE_OBJECT_TYPE[5]
	{
		TILE_OBJECT_TYPE.BED,
		TILE_OBJECT_TYPE.TABLE,
		TILE_OBJECT_TYPE.GUITAR,
		TILE_OBJECT_TYPE.TORCH,
		TILE_OBJECT_TYPE.DIVINE_ORB
	};

	public override bool isDwelling => true;

	public override Type serializedData => typeof(SaveDataDwelling);

	public override TILE_OBJECT_TYPE[] preplacedObjectsToIgnoreWhenBuilding => _preplacedObjectsToIgnoreWhenBuilding;

	public Dwelling(Region location)
		: base(STRUCTURE_TYPE.DWELLING, location)
	{
		base.maxResidentCapacity = 2;
		SetMaxHPAndReset(3500);
	}

	public Dwelling(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		base.maxResidentCapacity = 2;
		SetMaxHP(3500);
	}

	protected override string GenerateName()
	{
		if (base.residents != null && base.residents.Count > 0)
		{
			Character character = base.residents[0];
			if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("Indonesian (id)"))
			{
				return base.structureType.LocalizedStructureName() + " " + Utilities.PossessionString(character.name);
			}
			if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Latin America) (es)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("Spanish (Spain) (es-ES)"))
			{
				return base.structureType.LocalizedStructureName() + " de " + Utilities.PossessionString(character.name);
			}
			if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Italian (it)"))
			{
				return base.structureType.LocalizedStructureName() + " di " + Utilities.PossessionString(character.name);
			}
			return Utilities.PossessionString(character.name) + " " + base.structureType.LocalizedStructureName();
		}
		return LocalizationManager.Instance.GetLocalizedValue("Structures_Table", "DWELLING");
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		base.SubscribeListeners(shouldLock);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
	}

	protected override void UnsubscribeListeners()
	{
		base.UnsubscribeListeners();
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_NAME, OnCharacterChangedName);
	}

	private void OnCharacterChangedName(Character p_character)
	{
		if (p_character.homeStructure == this)
		{
			ReGenerateName();
		}
	}

	protected override void OnAddResident(Character newResident)
	{
		base.OnAddResident(newResident);
		if (GameManager.Instance.gameHasStarted)
		{
			ProcessAllTileObjects(delegate(TileObject t)
			{
				if (t.isPreplaced)
				{
					t.UpdateOwners();
				}
			});
		}
		if (base.residents.Count == 1)
		{
			ReGenerateName();
		}
	}

	protected override void OnRemoveResident(Character p_resident)
	{
		base.OnRemoveResident(p_resident);
		if (base.residents.Count <= 0)
		{
			ReGenerateName();
		}
	}

	public override bool CanBeResidentHere(Character character)
	{
		if (base.residents.Count == 0)
		{
			return true;
		}
		for (int i = 0; i < base.residents.Count; i++)
		{
			List<RELATIONSHIP_TYPE> list = base.residents[i].relationshipContainer.GetRelationshipDataWith(character)?.relationships ?? null;
			if (list != null && list.Contains(RELATIONSHIP_TYPE.LOVER))
			{
				return true;
			}
		}
		return false;
	}

	public override string GetNameRelativeTo(Character character)
	{
		if (character != null && character.homeStructure == this)
		{
			Dictionary<string, string> args = LocalizationManager.sourceMalePronouns;
			if (character.gender == GENDER.FEMALE)
			{
				args = LocalizationManager.sourceFemalePronouns;
			}
			return LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", "At_Home", args);
		}
		if (base.residents.Count > 0)
		{
			string text = base.residents[0].name;
			for (int i = 1; i < base.residents.Count; i++)
			{
				text = ((i + 1 != base.residents.Count) ? (text + ", ") : (text + " " + LocalizationManager.And + " "));
				text += base.residents[i].name;
			}
			if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("Indonesian (id)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("Thai (th)"))
			{
				return LocalizationManager.Instance.GetLocalizedValue("Structures_Table", "home") + " " + Utilities.PossessionString(text);
			}
			if (LocalizationSettings.SelectedLocale.LocaleName.Equals("Italian (it)"))
			{
				return LocalizationManager.Instance.GetLocalizedValue("Structures_Table", "home") + " di " + Utilities.PossessionString(text);
			}
			return Utilities.PossessionString(text) + " " + LocalizationManager.Instance.GetLocalizedValue("Structures_Table", "home");
		}
		return LocalizationManager.Instance.GetLocalizedValue("Structures_Table", "Empty_House");
	}
}
