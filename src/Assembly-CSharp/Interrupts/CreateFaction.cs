namespace Interrupts;

public class CreateFaction : Interrupt
{
	public CreateFaction()
		: base(INTERRUPT.Create_Faction)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
		base.shouldShowNotif = true;
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Faction faction = null;
		string text = "character_create_faction";
		if (interruptHolder.identifier == "create_bandits")
		{
			text = interruptHolder.identifier;
			FactionManager.Instance.CreateBanditFaction();
			faction = FactionManager.Instance.banditFaction;
			FactionManager.Instance.RerollFactionRelationships(faction, interruptHolder.actor, isRerollForNewFaction: true, logRelationshipChangeFromLeaderRelationship: false);
			FactionManager.Instance.RevalidateFactionCrimes(faction, interruptHolder.actor);
			FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, interruptHolder.actor);
			FactionManager.Instance.RerollSpecialIdeologies(faction, interruptHolder.actor);
		}
		else if (interruptHolder.identifier == "create_religious_cult")
		{
			text = interruptHolder.identifier;
			RELIGION religion = interruptHolder.actor.religionComponent.religion;
			faction = FactionManager.Instance.CreateReligiousCultFactionForReligion(religion);
			FactionManager.Instance.RerollFactionRelationships(faction, interruptHolder.actor, isRerollForNewFaction: true, logRelationshipChangeFromLeaderRelationship: false);
		}
		else
		{
			if (interruptHolder.actor.traitContainer.IsReligiousCultist(out var p_religion) && FactionManager.Instance.GetReligiousCultFactionForReligion(p_religion) == null)
			{
				faction = FactionManager.Instance.CreateReligiousCultFactionForReligion(p_religion);
			}
			if (faction == null)
			{
				FACTION_TYPE factionType = FactionManager.Instance.GetFactionTypeForCharacter(interruptHolder.actor);
				if (interruptHolder.identifier == "create_lycan_clan")
				{
					factionType = FACTION_TYPE.Lycan_Clan;
				}
				faction = FactionManager.Instance.CreateNewFaction(factionType, "", null, interruptHolder.actor.race);
				faction.factionType.SetAsDefault(faction);
				FactionManager.Instance.RerollPeaceTypeIdeology(faction, interruptHolder.actor);
				FactionManager.Instance.RerollInclusiveTypeIdeology(faction, interruptHolder.actor);
				FactionManager.Instance.RerollReligionTypeIdeology(faction, interruptHolder.actor);
				FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, interruptHolder.actor);
				FactionManager.Instance.RerollSpecialIdeologies(faction, interruptHolder.actor);
				FactionManager.Instance.RevalidateFactionCrimes(faction, interruptHolder.actor);
				if (!string.IsNullOrEmpty(interruptHolder.identifier) && interruptHolder.identifier == "own_settlement" && interruptHolder.actor.homeSettlement != null)
				{
					LandmarkManager.Instance.OwnSettlement(faction, interruptHolder.actor.homeSettlement);
				}
				FactionManager.Instance.RerollFactionRelationships(faction, interruptHolder.actor, isRerollForNewFaction: true, logRelationshipChangeFromLeaderRelationship: false);
			}
		}
		interruptHolder.actor.ChangeFactionTo(faction, bypassIdeologyChecking: true);
		faction.SetLeader(interruptHolder.actor);
		Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, faction);
		Messenger.Broadcast(FactionSignals.FACTION_IDEOLOGIES_CHANGED, faction);
		overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " " + text, LOG_TAG.Life_Changes);
		overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
		overrideEffectLog.AddToFillers(interruptHolder.actor.currentRegion, interruptHolder.actor.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);
		Messenger.Broadcast(FactionSignals.CREATE_FACTION_INTERRUPT, faction, interruptHolder.actor);
		return true;
	}
}
