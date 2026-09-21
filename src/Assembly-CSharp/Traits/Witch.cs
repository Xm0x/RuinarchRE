namespace Traits;

public class Witch : Trait
{
	public override bool isSingleton => true;

	public Witch()
	{
		name = "Witch";
		description = "An extreme Nature Worshiper.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = false;
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character { isDead: false } character)
		{
			CharacterManager.Instance.IncreaseActiveReligiousCultist(RELIGION.Nature_Worship, character);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.behaviourComponent.AddBehaviourComponent(typeof(WitchBehaviour));
			CharacterManager.Instance.IncreaseActiveReligiousCultist(RELIGION.Nature_Worship, character);
			if (PlayerManager.Instance != null && PlayerManager.Instance.player != null)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Skills", "PlayerPowerAlerts_Table", "chaotic_energy_costs_increased", LOG_TAG.Player, LOG_TAG.Major);
				log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			}
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.behaviourComponent.RemoveBehaviourComponent(typeof(WitchBehaviour));
			CharacterManager.Instance.DecreaseActiveReligiousCultist(RELIGION.Nature_Worship, character);
		}
	}

	public override bool OnDeath(Character character)
	{
		CharacterManager.Instance.DecreaseActiveReligiousCultist(RELIGION.Nature_Worship, character);
		character.behaviourComponent.RemoveBehaviourComponent(typeof(WitchBehaviour));
		return base.OnDeath(character);
	}
}
