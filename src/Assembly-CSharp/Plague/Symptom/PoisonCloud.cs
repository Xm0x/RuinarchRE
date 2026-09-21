using Inner_Maps;
using UtilityScripts;

namespace Plague.Symptom;

public class PoisonCloud : PlagueSymptom
{
	public override PLAGUE_SYMPTOM symptomType => PLAGUE_SYMPTOM.Poison_Cloud;

	protected override void ActivateSymptom(Character p_character)
	{
		int stacks = GameUtilities.RandomBetweenTwoNumbers(2, 5);
		InnerMapManager.Instance.SpawnPoisonCloud(p_character.gridTileLocation, stacks);
		Messenger.Broadcast(PlayerSkillSignals.ON_PLAGUE_POISON_CLOUD_ACTIVATED, p_character);
	}

	public override void PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (GameUtilities.RollChance(1.5f))
		{
			ActivateSymptomOn(p_character);
		}
	}
}
