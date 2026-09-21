using Factions.Faction_Types;
using UtilityScripts;

namespace Interrupts;

public class EvaluateCultistAffiliation : Interrupt
{
	public EvaluateCultistAffiliation()
		: base(INTERRUPT.Evaluate_Cultist_Affiliation)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		string identifier = interruptHolder.identifier;
		Faction faction = null;
		Character character = interruptHolder.target as Character;
		switch (identifier)
		{
		case "Demon_Worship":
			faction = FactionManager.Instance.demonCultFaction;
			break;
		case "Divine_Worship":
			faction = FactionManager.Instance.divineCultFaction;
			break;
		case "Nature_Worship":
			faction = FactionManager.Instance.natureCultFaction;
			break;
		}
		int num = 0;
		RELIGION p_religion;
		if (actor.classComponent.IsStalkerCannotBeTurned())
		{
			num = 100;
		}
		else if (actor.traitContainer.IsReligiousCultist(out p_religion))
		{
			if (faction.factionType is CultFaction cultFaction && cultFaction.cultReligion != p_religion)
			{
				num = 100;
			}
		}
		else if (actor.traitContainer.IsBlessed() && faction.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			num = 100;
		}
		else if (!faction.CanCharacterJoinFactionBasedOnNonReligionIdeologiesAndBanning(actor))
		{
			num = 100;
		}
		else if (actor.characterClass.className == "Hero" && faction.factionType.type == FACTION_TYPE.Demon_Cult)
		{
			num = 100;
		}
		else if (character != null)
		{
			if (actor.relationshipContainer.IsFriendsWith(character))
			{
				Character firstCharacterWithRelationship = character.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
				bool flag = character.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR);
				bool flag2 = actor.relationshipContainer.IsFamilyMember(character);
				if (firstCharacterWithRelationship == actor)
				{
					num = 0;
				}
				else
				{
					Character firstCharacterWithRelationship2 = actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER);
					bool flag3 = firstCharacterWithRelationship2 == null || !actor.relationshipContainer.IsFriendsWith(firstCharacterWithRelationship2);
					if (flag && flag3)
					{
						num += 30;
					}
					if (flag2)
					{
						num += 20;
					}
					string opinionLabel = actor.relationshipContainer.GetOpinionLabel(character);
					if (opinionLabel == "Close Friend")
					{
						num += 20;
					}
					else if (opinionLabel == "Friend")
					{
						num += 50;
					}
				}
			}
			else
			{
				num = 100;
			}
		}
		if (GameUtilities.RollChance(num) && actor.ChangeFactionTo(FactionManager.Instance.vagrantFaction))
		{
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " leave", base.logTags);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
			return true;
		}
		actor.ChangeFactionTo(faction, bypassIdeologyChecking: true);
		overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " join", base.logTags);
		overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		overrideEffectLog.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
		return true;
	}
}
