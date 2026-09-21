using Locations.Settlements.Settlement_Events;

namespace Traits;

public class Lethargic : Status
{
	public Lethargic()
	{
		name = "Lethargic";
		description = "Moving very sluggishly.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(6);
		moodEffect = -4;
		hindersSocials = true;
	}

	public override void OnAddTrait(ITraitable sourceCharacter)
	{
		base.OnAddTrait(sourceCharacter);
		if (sourceCharacter is Character character)
		{
			character.movementComponent.AdjustSpeedModifier(-0.5f);
			if (ChanceData.RollChance(CHANCE_TYPE.Plagued_Event_Lethargic) && character.homeSettlement != null && PlaguedEvent.HasMinimumAmountOfPlaguedVillagersForEvent(character.homeSettlement) && !character.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event) && character.homeSettlement.eventManager.CanHaveEvents())
			{
				character.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Plagued_Event);
			}
		}
	}

	public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy)
	{
		if (sourceCharacter is Character)
		{
			(sourceCharacter as Character).movementComponent.AdjustSpeedModifier(0.5f);
		}
		base.OnRemoveTrait(sourceCharacter, removedBy);
	}
}
