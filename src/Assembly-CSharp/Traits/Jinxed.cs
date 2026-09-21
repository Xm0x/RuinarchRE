using Characters.Components;

namespace Traits;

public class Jinxed : Trait, CharacterEventDispatcher.IMoodListener
{
	public Jinxed()
	{
		name = "Jinxed";
		description = "Drains Chaotic Energy from the player whenever it enters Bad or Critical Mood.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character { isDead: false } character)
		{
			character.eventDispatcher.SubscribeToMoodEvents(this);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character { isDead: false } character)
		{
			character.eventDispatcher.SubscribeToMoodEvents(this);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.eventDispatcher.UnsubscribeToMoodEvents(this);
		}
	}

	public override bool OnDeath(Character character)
	{
		character.eventDispatcher.UnsubscribeToMoodEvents(this);
		return base.OnDeath(character);
	}

	public void OnMoodChanged(Character p_character, MOOD_STATE p_previousMood, MOOD_STATE p_newMood)
	{
		int num = 0;
		if (p_newMood == MOOD_STATE.Critical)
		{
			num = 20;
		}
		if (num > 0 && PlayerManager.Instance.player.currenciesComponent.chaoticEnergy > 0)
		{
			PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergy(-num);
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Traits", "Traits_Table", "Jinxed chaotic_drain", LOG_TAG.Player, LOG_TAG.Major);
			log.AddToFillers(null, num.ToString(), LOG_IDENTIFIER.STRING_1);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		}
	}
}
