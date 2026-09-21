using Inner_Maps;
using UnityEngine;

public class SkinAnimal : GoapAction
{
	public int m_amountProducedPerTick = 1;

	private const float _coinGainMultiplier = 1.375f;

	public SkinAnimal()
		: base(INTERACTION_TYPE.SKIN_ANIMAL)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.shouldAddLogs = false;
		base.canBeAdvertisedEvenIfTargetIsUnavailable = true;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Skin Animal Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 10;
	}

	public override void AddFillersToLog(Log log, ActualGoapNode node)
	{
		base.AddFillersToLog(log, node);
		log.AddToFillers(node.target, node.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		SetPrecondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.DEATH, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.TARGET), IsTargetDead);
	}

	private bool IsTargetDead(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType)
	{
		if (poiTarget is Character character)
		{
			return character.isDead;
		}
		return true;
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		if (node.ticksPerformingCurrentState > 0)
		{
			ProduceMatsPile(node, pickUpItem: false);
		}
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		return base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
	}

	public void AfterSkinAnimalSuccess(ActualGoapNode p_node)
	{
		ResourcePile resourcePile = ProduceMatsPile(p_node, pickUpItem: true);
		if (resourcePile != null && resourcePile.resourceInPile > 0)
		{
			p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(resourcePile);
		}
	}

	private ResourcePile ProduceMatsPile(ActualGoapNode p_node, bool pickUpItem)
	{
		Summon summon = p_node.target as Summon;
		SkinnableAnimal skinnableAnimal = summon as SkinnableAnimal;
		int num = p_node.ticksPerformingCurrentState * m_amountProducedPerTick;
		if (skinnableAnimal.count - num < 0)
		{
			num = skinnableAnimal.count;
		}
		if (num <= 0)
		{
			return null;
		}
		skinnableAnimal.count = (int)Mathf.Clamp(skinnableAnimal.count - num, 0f, 1000f);
		ResourcePile resourcePile = InnerMapManager.Instance.CreateNewTileObject<ResourcePile>(skinnableAnimal.produceableMaterial);
		p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt((float)num * 1.375f));
		p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Resources).AdjustExperience(12, p_node.actor);
		resourcePile.SetResourceInPile(num);
		if (pickUpItem)
		{
			p_node.actor.PickUpItem(resourcePile, changeCharacterOwnership: false, setOwnership: false);
		}
		else
		{
			LocationGridTile locationGridTile = p_node.actor.gridTileLocation;
			if (locationGridTile != null && locationGridTile.tileObjectComponent.objHere != null)
			{
				locationGridTile = p_node.actor.gridTileLocation.GetFirstNearestTileFromThisWithNoObject();
			}
			locationGridTile?.structure.AddPOI(resourcePile, locationGridTile);
		}
		ProduceLogs(p_node);
		if (summon.gridTileLocation != null && skinnableAnimal.count <= 0)
		{
			summon.DestroyMarker();
		}
		return resourcePile;
	}

	public void ProduceLogs(ActualGoapNode p_node)
	{
		string value = ResourcePile.GetResourceQuantityString(p_node.ticksPerformingCurrentState * m_amountProducedPerTick) + " " + (p_node.target as Summon).produceableMaterial.LocalizedName();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", p_node.action.goapName + " produced_resources", LOG_TAG.Work, null);
		log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		p_node.LogAction(log, ignoreShouldAddLog: true);
	}
}
