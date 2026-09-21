using System;
using System.Collections.Generic;

namespace Traits;

public class Aroused : Status
{
	public List<string> arousedTargetIDs { get; private set; }

	public override Type serializedData => typeof(SaveDataAroused);

	public Character latestValidArousedTarget => GetLatestValidArousedTarget();

	public Aroused()
	{
		name = "Aroused";
		description = "Feeling hot and frisky!";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.POSITIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8);
		arousedTargetIDs = new List<string>();
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataAroused saveDataAroused = p_saveDataTrait as SaveDataAroused;
		arousedTargetIDs.AddRange(saveDataAroused.arousedTargetIDs);
	}

	public void AddArousedTarget(Character p_character)
	{
		if (!IsArousedTarget(p_character))
		{
			arousedTargetIDs.Add(p_character.persistentID);
		}
	}

	public bool IsArousedTarget(Character p_character)
	{
		return arousedTargetIDs.Contains(p_character.persistentID);
	}

	private Character GetLatestValidArousedTarget()
	{
		if (arousedTargetIDs.Count > 0)
		{
			for (int num = arousedTargetIDs.Count - 1; num >= 0; num--)
			{
				Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(arousedTargetIDs[num]);
				if (!characterByPersistentID.isDead && characterByPersistentID.hasMarker)
				{
					return characterByPersistentID;
				}
			}
		}
		return null;
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		arousedTargetIDs.Remove(p_character.persistentID);
	}
}
