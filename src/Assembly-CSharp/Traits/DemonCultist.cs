using System.Collections.Generic;
using Maccima_Games.Util;
using UnityEngine;

namespace Traits;

public class DemonCultist : Trait
{
	public override bool isSingleton => true;

	public override bool affectsNameIcon => true;

	public DemonCultist()
	{
		name = "Demon Cultist";
		description = "Worships us, but only secretly. Produces a Chaos Orb when praying. May produce 4 Chaos Orbs by performing a Dark Ritual.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = false;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.SACRIFICE_SELF,
			INTERACTION_TYPE.CREATE_CULTIST_KIT
		};
		AddTraitOverrideFunctionIdentifier("Death_Trait");
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			character.AddPlayerAction(PLAYER_SKILL_TYPE.CULTIST_POISON, broadcastSignal: false);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP, broadcastSignal: false);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.EVANGELIZE, broadcastSignal: false);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.SPREAD_RUMOR, broadcastSignal: false);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.FOUND_CULT, broadcastSignal: false);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.ABSORB_CULTIST, broadcastSignal: false);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.FABRICATE_CRIME, broadcastSignal: false);
			character.jobComponent.AddAbleJob(JOB_TYPE.STEAL_CORPSE);
			character.jobComponent.AddAbleJob(JOB_TYPE.SUMMON_BONE_GOLEM);
			CharacterManager.Instance.IncreaseActiveReligiousCultist(RELIGION.Demon_Worship, character);
		}
	}

	public override void OnAddTrait(ITraitable sourceCharacter)
	{
		base.OnAddTrait(sourceCharacter);
		if (sourceCharacter is Character character)
		{
			character.behaviourComponent.AddBehaviourComponent(typeof(DemonCultistBehaviour));
			character.isInfoUnlocked = true;
			character.AddPlayerAction(PLAYER_SKILL_TYPE.CULTIST_POISON);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.EVANGELIZE);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.SPREAD_RUMOR);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.FOUND_CULT);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.ABSORB_CULTIST);
			character.AddPlayerAction(PLAYER_SKILL_TYPE.FABRICATE_CRIME);
			character.jobComponent.AddAbleJob(JOB_TYPE.STEAL_CORPSE);
			character.jobComponent.AddAbleJob(JOB_TYPE.SUMMON_BONE_GOLEM);
			character.traitContainer.AddTrait(character, "Nocturnal");
			if (character.traitContainer.HasTrait("Necromancer"))
			{
				FactionManager.Instance.undeadFaction.SetRelationshipFor(PlayerManager.Instance.player.playerFaction, FACTION_RELATIONSHIP_STATUS.Friendly);
			}
			character.traitContainer.RemoveTrait(character, "Blessed");
			CharacterManager.Instance.IncreaseActiveReligiousCultist(RELIGION.Demon_Worship, character);
			Messenger.Broadcast(CharacterSignals.CHARACTER_BECOME_DEMON_CULTIST, character);
			if (!character.isDead)
			{
				PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_DEMON_CULTIST);
				PlayerManager.Instance?.player?.playerSkillComponent.GetPrismEvent<CultLeaderEvent>().AdjustNumberOfAliveDemonCultists(1);
			}
			character.ResetUIString();
		}
	}

	public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy)
	{
		base.OnRemoveTrait(sourceCharacter, removedBy);
		if (sourceCharacter is Character character)
		{
			character.behaviourComponent.RemoveBehaviourComponent(typeof(DemonCultistBehaviour));
			character.RemovePlayerAction(PLAYER_SKILL_TYPE.CULTIST_POISON);
			character.RemovePlayerAction(PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP);
			character.RemovePlayerAction(PLAYER_SKILL_TYPE.EVANGELIZE);
			character.RemovePlayerAction(PLAYER_SKILL_TYPE.SPREAD_RUMOR);
			character.RemovePlayerAction(PLAYER_SKILL_TYPE.FOUND_CULT);
			character.RemovePlayerAction(PLAYER_SKILL_TYPE.ABSORB_CULTIST);
			character.RemovePlayerAction(PLAYER_SKILL_TYPE.FABRICATE_CRIME);
			character.jobComponent.RemoveAbleJob(JOB_TYPE.STEAL_CORPSE);
			character.jobComponent.RemoveAbleJob(JOB_TYPE.SUMMON_BONE_GOLEM);
			CharacterManager.Instance.DecreaseActiveReligiousCultist(RELIGION.Demon_Worship, character);
			Messenger.Broadcast(CharacterSignals.CHARACTER_NO_LONGER_CULTIST, character);
			if (!character.isDead)
			{
				PlayerManager.Instance.player.playerSkillComponent.GetPrismEvent<CultLeaderEvent>().AdjustNumberOfAliveDemonCultists(-1);
			}
			character.ResetUIString();
		}
	}

	public override bool OnDeath(Character character)
	{
		CharacterManager.Instance.DecreaseActiveReligiousCultist(RELIGION.Demon_Worship, character);
		return base.OnDeath(character);
	}

	protected override string GetDescriptionInUI()
	{
		string text = base.GetDescriptionInUI();
		SkillData skillData = PlayerSkillManager.Instance.GetSkillData(PLAYER_SKILL_TYPE.BRAINWASH);
		if (skillData != null)
		{
			if (skillData.hasUnliChaosOrbs)
			{
				text = text + "\n<color=#" + ColorUtility.ToHtmlStringRGB(PlayerSkillManager.Instance.withChaosOrbsTextColor) + ">" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Unlimited_Chaos_Orbs") + "</color>";
			}
			else if (skillData.hasRemainingChaosOrbs)
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("chaosOrbs", skillData.remainingChaosOrbs.ToString());
				text = text + "\n<color=#" + ColorUtility.ToHtmlStringRGB(PlayerSkillManager.Instance.withChaosOrbsTextColor) + ">" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Remaining_Chaos_Orbs", dictionary) + "</color>";
				MaccimaDictionaryPool<string, string>.Release(dictionary);
			}
		}
		return text;
	}
}
