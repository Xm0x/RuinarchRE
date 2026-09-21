using System.Collections.Generic;
using UtilityScripts;

public class Burn : GoapAction
{
	public Burn()
		: base(INTERACTION_TYPE.BURN)
	{
		base.actionIconString = GoapActionStateDB.Burn_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Burn Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		if (node.associatedJobType != JOB_TYPE.ARSON_RAID || witness.faction != actor.faction)
		{
			node.IncreaseReactionCounter();
			CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);
			node.DecreaseReactionCounter();
		}
		List<EMOTION> list = RuinarchListPool<EMOTION>.Claim(5);
		PopulateEmotionReactionsToActor(list, actor, target, witness, node, status);
		string text = string.Empty;
		int p_totalOpinionReduction = 0;
		string p_lastStrawReasonKey = string.Empty;
		for (int i = 0; i < list.Count; i++)
		{
			text += CharacterManager.Instance.TriggerEmotion(list[i], witness, actor, status, ref p_totalOpinionReduction, ref p_lastStrawReasonKey, node, "", p_triggerOpinionChangesEffect: false);
		}
		if (p_totalOpinionReduction < 0 && !witness.reactionComponent.isDisguised && !actor.reactionComponent.isDisguised)
		{
			witness.relationshipContainer.CreateJobsOnOpinionReduced(witness, actor, p_lastStrawReasonKey, p_totalOpinionReduction);
		}
		RuinarchListPool<EMOTION>.Release(list);
		return text;
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		if (witness.traitContainer.HasTrait("Pyromaniac"))
		{
			reactions.Add(EMOTION.Approval);
			return;
		}
		if (witness.traitContainer.HasTrait("Pyrophobic"))
		{
			reactions.Add(EMOTION.Fear);
			reactions.Add(EMOTION.Resentment);
			return;
		}
		reactions.Add(EMOTION.Disapproval);
		if (witness.relationshipContainer.IsFriendsOrAcquaintancesWith(actor))
		{
			reactions.Add(EMOTION.Disappointment);
		}
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Arson;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Arson;
	}

	public void AfterBurnSuccess(ActualGoapNode goapNode)
	{
		if (goapNode.associatedJobType == JOB_TYPE.CRITICAL_BREAK)
		{
			goapNode.actor.behaviourComponent.AddCriticalBreakFire();
		}
		CombatManager.Instance.CreateProjectile(goapNode.actor.marker, goapNode.poiTarget, null, delegate(Character actor, IDamageable damageable, CombatState state, Projectile projectile)
		{
			OnHitTarget(actor, damageable, state, projectile);
		});
	}

	private void OnHitTarget(Character actor, IDamageable damageable, CombatState state, Projectile projectile)
	{
		if (damageable is IPointOfInterest pointOfInterest)
		{
			pointOfInterest.traitContainer.AddTrait(pointOfInterest, "Burning", actor, bypassElementalChance: true, -1, 0f, ELEMENTAL_TYPE.Fire);
			Messenger.Broadcast(CharacterSignals.REPROCESS_POI, pointOfInterest);
		}
	}
}
