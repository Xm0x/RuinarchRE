using Object_Pools;

public class ExpelData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.EXPEL;

	public override string name => "Expel";

	public override string description => "This Ability kicks out a character from its current Village and Faction.\nExpelling a hostile Villager from its Faction will produce a Chaos Orb.";

	public ExpelData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		if (targetPOI is Character character)
		{
			character.jobQueue.CancelAllJobs();
			if (character.faction != null)
			{
				character.faction.KickOutCharacterByPlayer(character);
			}
			if (character.homeSettlement != null && character.homeSettlement.locationType == LOCATION_TYPE.VILLAGE)
			{
				character.MigrateHomeStructureTo(null);
			}
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Skills", "PlayerPowerAlerts_Table", "Expel expel_success", LOG_TAG.Player, LOG_TAG.Life_Changes);
			log.AddToFillers(null, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(null, character.prevFaction.name, LOG_IDENTIFIER.FACTION_1);
			log.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
			LogPool.Release(log);
			Messenger.Broadcast(PlayerSkillSignals.EXPEL_ACTIVATED, targetPOI);
		}
		base.ActivateAbility(targetPOI);
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (target is Character character)
		{
			if (base.IsValid(target) && character.faction != null)
			{
				return character.faction.isMajorNonPlayer;
			}
			return false;
		}
		return false;
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (targetCharacter.isDead)
		{
			return false;
		}
		return base.CanPerformAbilityTowards(targetCharacter);
	}
}
