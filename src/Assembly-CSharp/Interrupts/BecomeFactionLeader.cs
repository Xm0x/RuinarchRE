using System.Collections.Generic;
using Object_Pools;
using UtilityScripts;

namespace Interrupts;

public class BecomeFactionLeader : Interrupt
{
	public BecomeFactionLeader()
		: base(INTERRUPT.Become_Faction_Leader)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Major };
		base.shouldShowNotif = true;
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		Faction faction = actor.faction;
		if (faction == FactionManager.Instance.banditFaction)
		{
			faction.SetLeader(actor);
			if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableFactionIdeologyChanges)
			{
				FactionManager.Instance.RerollFactionRelationships(faction, interruptHolder.actor, isRerollForNewFaction: true, logRelationshipChangeFromLeaderRelationship: false);
				FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, actor);
				FactionManager.Instance.RerollSpecialIdeologies(faction, actor, interruptHolder.identifier == "succession");
				Messenger.Broadcast(FactionSignals.FACTION_IDEOLOGIES_CHANGED, faction);
			}
			FactionManager.Instance.RevalidateFactionCrimes(faction, actor);
			Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, faction);
			OverrideLogBecomeLeader(ref overrideEffectLog, actor, faction);
			return true;
		}
		bool wasFactionMergedWithReligiousCult = false;
		RELIGION p_religion;
		bool flag = actor.traitContainer.IsReligiousCultist(out p_religion);
		bool flag2 = false;
		if (flag)
		{
			flag2 = p_religion.GetFactionTypeForReligion() == faction.factionType.type;
			if (!flag2 && FactionManager.Instance.FactionLeaderCultistProcessing(actor, p_religion, out wasFactionMergedWithReligiousCult, out var createdReligiousCultFaction))
			{
				if (overrideEffectLog != null)
				{
					LogPool.Release(overrideEffectLog);
				}
				string text = (createdReligiousCultFaction ? "convert_to_cult" : "merged_with_cult");
				overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " " + text, LOG_TAG.Major);
				overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
				Faction religiousCultFactionForReligion = FactionManager.Instance.GetReligiousCultFactionForReligion(p_religion);
				overrideEffectLog.AddToFillers(religiousCultFactionForReligion, religiousCultFactionForReligion.name, LOG_IDENTIFIER.FACTION_2);
				overrideEffectLog.AddToFillers(null, TraitManager.Instance.GetLocalizedNameOfTrait(p_religion.GetCultistTraitNameForReligion()), LOG_IDENTIFIER.STRING_1);
				if (createdReligiousCultFaction)
				{
					FactionManager.Instance.RerollFactionRelationships(religiousCultFactionForReligion, actor, isRerollForNewFaction: true, logRelationshipChangeFromLeaderRelationship: false);
				}
			}
		}
		if (!wasFactionMergedWithReligiousCult)
		{
			if (flag && !flag2)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "cannot_join_demon_cult", LOG_TAG.Major);
				log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
				Faction religiousCultFactionForReligion2 = FactionManager.Instance.GetReligiousCultFactionForReligion(p_religion);
				log.AddToFillers(religiousCultFactionForReligion2, religiousCultFactionForReligion2.name, LOG_IDENTIFIER.STRING_1);
				log.AddLogToDatabase(releaseLogAfter: true);
			}
			faction.SetLeader(actor);
			if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableFactionIdeologyChanges)
			{
				FactionManager.Instance.RerollPeaceTypeIdeology(faction, actor);
				FactionManager.Instance.RerollInclusiveTypeIdeology(faction, actor);
				FactionManager.Instance.RerollReligionTypeIdeology(faction, actor);
				FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, actor);
				FactionManager.Instance.RerollSpecialIdeologies(faction, actor, interruptHolder.identifier == "succession");
				Messenger.Broadcast(FactionSignals.FACTION_IDEOLOGIES_CHANGED, faction);
			}
			FactionManager.Instance.RevalidateFactionCrimes(faction, actor);
			Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, faction);
			if (!WorldSettings.Instance.worldSettingsData.factionSettings.disableFactionIdeologyChanges)
			{
				FactionManager.Instance.RerollFactionRelationships(faction, actor, isRerollForNewFaction: false, logRelationshipChangeFromLeaderRelationship: true);
			}
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Faction", "Faction_Table", "ideology_change", LOG_TAG.Life_Changes);
			log2.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
			log2.AddLogToDatabase(releaseLogAfter: true);
			List<Character> list = RuinarchListPool<Character>.Claim();
			list.AddRange(faction.characters);
			list.Remove(actor);
			for (int i = 0; i < list.Count; i++)
			{
				Character character = list[i];
				faction.CheckIfCharacterStillFitsIdeology(character);
			}
			RuinarchListPool<Character>.Release(list);
			OverrideLogBecomeLeader(ref overrideEffectLog, actor, faction);
		}
		return true;
	}

	private void OverrideLogBecomeLeader(ref Log p_log, Character p_actor, Faction p_faction)
	{
		if (p_log != null)
		{
			LogPool.Release(p_log);
		}
		p_log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " became_leader", LOG_TAG.Major);
		p_log.AddToFillers(p_actor, p_actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		p_log.AddToFillers(p_faction, p_faction.name, LOG_IDENTIFIER.FACTION_1);
	}
}
