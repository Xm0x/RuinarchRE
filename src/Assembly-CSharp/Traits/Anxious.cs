using System;
using System.Collections.Generic;

namespace Traits;

public class Anxious : Status
{
	public List<string> sourceOfAnxietyIDs { get; private set; }

	public override Type serializedData => typeof(SaveDataAnxious);

	public Anxious()
	{
		name = "Anxious";
		description = "Worried about something.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		moodEffect = -8;
		isStacking = true;
		stackLimit = 3;
		stackModifier = 0.5f;
		hindersSocials = true;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(4);
		sourceOfAnxietyIDs = new List<string>();
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataAnxious saveDataAnxious = p_saveDataTrait as SaveDataAnxious;
		sourceOfAnxietyIDs.AddRange(saveDataAnxious.sourceOfAnxietyIDs);
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (IsSourceOfAnxiety(targetPOI))
		{
			characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Anxious, targetPOI);
			return true;
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	public override string GetTestingData(ITraitable traitable = null)
	{
		string text = base.GetTestingData(traitable);
		for (int i = 0; i < sourceOfAnxietyIDs.Count; i++)
		{
			string text2 = sourceOfAnxietyIDs[i];
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(text2);
			text = ((characterByPersistentID != null) ? (text + ", " + characterByPersistentID.name) : (text + ", " + text2));
		}
		return text;
	}

	public void AddSourceOfAnxiety(IPointOfInterest p_poi)
	{
		if (p_poi != null && !sourceOfAnxietyIDs.Contains(p_poi.persistentID))
		{
			sourceOfAnxietyIDs.Add(p_poi.persistentID);
		}
	}

	public bool IsSourceOfAnxiety(IPointOfInterest p_poi)
	{
		return sourceOfAnxietyIDs.Contains(p_poi.persistentID);
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		sourceOfAnxietyIDs.Remove(p_character.persistentID);
	}
}
