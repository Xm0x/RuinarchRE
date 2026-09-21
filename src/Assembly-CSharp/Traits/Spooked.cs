using System;
using System.Collections.Generic;
using Logs;
using UtilityScripts;

namespace Traits;

public class Spooked : Status
{
	public List<string> sourceOfFearIDs { get; private set; }

	public List<PLAYER_SKILL_TYPE> sourceOfFearSpells { get; private set; }

	public override Type serializedData => typeof(SaveDataSpooked);

	public Spooked()
	{
		name = "Spooked";
		description = "Something recently scared it.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
		moodEffect = -6;
		isStacking = true;
		stackLimit = 5;
		stackModifier = 0.25f;
		hindersSocials = true;
		sourceOfFearIDs = new List<string>();
		sourceOfFearSpells = new List<PLAYER_SKILL_TYPE>();
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		if (p_saveDataTrait is SaveDataSpooked saveDataSpooked)
		{
			if (saveDataSpooked.sourceOfFearIDs != null && saveDataSpooked.sourceOfFearIDs.Count > 0)
			{
				sourceOfFearIDs.AddRange(saveDataSpooked.sourceOfFearIDs);
			}
			if (saveDataSpooked.sourceOfFearSpells != null && saveDataSpooked.sourceOfFearSpells.Count > 0)
			{
				sourceOfFearSpells.AddRange(saveDataSpooked.sourceOfFearSpells);
			}
		}
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (IsSourceOfFear(targetPOI))
		{
			characterThatWillDoJob.combatComponent.Flight(targetPOI, "Saw_Spooky");
			return true;
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		sourceOfFearIDs.Remove(p_character.persistentID);
	}

	public void AddSourceOfFear(IPointOfInterest p_poi)
	{
		if (p_poi != null && !sourceOfFearIDs.Contains(p_poi.persistentID))
		{
			sourceOfFearIDs.Add(p_poi.persistentID);
		}
	}

	private bool IsSourceOfFear(IPointOfInterest p_poi)
	{
		return sourceOfFearIDs.Contains(p_poi.persistentID);
	}

	public void AddSourceOfFear(PLAYER_SKILL_TYPE p_type)
	{
		if (!sourceOfFearSpells.Contains(p_type))
		{
			sourceOfFearSpells.Add(p_type);
		}
	}

	private bool IsSourceOfFear(PLAYER_SKILL_TYPE p_type)
	{
		return sourceOfFearSpells.Contains(p_type);
	}

	public bool TryTriggerFeelingSpooked(Character p_owner)
	{
		if (GameUtilities.RollChance(50 * p_owner.traitContainer.stacks[name]))
		{
			p_owner.traitContainer.AddTrait(p_owner, "Abstain Tiredness");
			p_owner.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Spooked, p_owner);
			return true;
		}
		return false;
	}

	protected override void GetMoodEffectFlavorTextActiveCharacterFiller(out ILogFiller p_obj, out string p_name)
	{
		if (sourceOfFearSpells.Count > 0)
		{
			PLAYER_SKILL_TYPE pLAYER_SKILL_TYPE = sourceOfFearSpells[0];
			SkillData skillData = PlayerSkillManager.Instance.GetSkillData(pLAYER_SKILL_TYPE);
			p_obj = null;
			p_name = skillData.localizedName;
		}
		else
		{
			base.GetMoodEffectFlavorTextActiveCharacterFiller(out p_obj, out p_name);
		}
	}

	public override string GetTestingData(ITraitable traitable = null)
	{
		string testingData = base.GetTestingData(traitable);
		testingData += "\n\tSources of Fear: ";
		for (int i = 0; i < sourceOfFearIDs.Count; i++)
		{
			string key = sourceOfFearIDs[i];
			if (DatabaseManager.Instance.tileObjectDatabase.tileObjectsByGUID.ContainsKey(key))
			{
				TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.tileObjectsByGUID[key];
				testingData = testingData + "\n\t\t" + tileObject.nameWithID;
			}
			else if (DatabaseManager.Instance.characterDatabase.allCharacters.ContainsKey(key))
			{
				Character character = DatabaseManager.Instance.characterDatabase.allCharacters[key];
				testingData = testingData + "\n\t\t" + character.name;
			}
		}
		for (int j = 0; j < sourceOfFearSpells.Count; j++)
		{
			testingData = testingData + "\n\t\t" + sourceOfFearSpells[j];
		}
		return testingData;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		sourceOfFearIDs.Contains(p_character.persistentID);
	}
}
