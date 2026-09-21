using Inner_Maps;
using UnityEngine;

public class ShearAnimal : GoapAction
{
	public int m_amountProducedPerTick = 1;

	private const float _coinGainMultiplier = 1.375f;

	public ShearAnimal()
		: base(INTERACTION_TYPE.SHEAR_ANIMAL)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Work };
		base.shouldAddLogs = false;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Shear Animal Success", goapNode);
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

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		return base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		if (node.ticksPerformingCurrentState > 0)
		{
			ProduceMatsPile(node, pickUpItem: false);
		}
	}

	public void AfterShearAnimalSuccess(ActualGoapNode p_node)
	{
		ResourcePile resourcePile = ProduceMatsPile(p_node, pickUpItem: true);
		if (resourcePile != null && resourcePile.resourceInPile > 0)
		{
			p_node.actor.jobComponent.TryCreateHaulToWorkplaceJob(resourcePile);
		}
	}

	private ResourcePile ProduceMatsPile(ActualGoapNode p_node, bool pickUpItem)
	{
		Summon obj = p_node.target as Summon;
		ShearableAnimal shearableAnimal = obj as ShearableAnimal;
		int num = p_node.ticksPerformingCurrentState * m_amountProducedPerTick;
		if (shearableAnimal.count - num < 0)
		{
			num = shearableAnimal.count;
		}
		if (obj.gridTileLocation != null && shearableAnimal.count <= 0)
		{
			shearableAnimal.isAvailableForShearing = false;
		}
		if (num <= 0)
		{
			return null;
		}
		shearableAnimal.count = (int)Mathf.Clamp(shearableAnimal.count - num, 0f, 1000f);
		ResourcePile resourcePile = InnerMapManager.Instance.CreateNewTileObject<ResourcePile>(shearableAnimal.produceableMaterial);
		p_node.actor.moneyComponent.AdjustCoins(Mathf.CeilToInt((float)num * 1.375f));
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
		p_node.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Resources).AdjustExperience(12, p_node.actor);
		return resourcePile;
	}

	public void ProduceLogs(ActualGoapNode p_node)
	{
		string value = ResourcePile.GetResourceQuantityString(p_node.ticksPerformingCurrentState * m_amountProducedPerTick) + " " + (p_node.target as Animal).produceableMaterial.LocalizedName();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", p_node.action.goapName + " produced_resources", LOG_TAG.Work, null);
		log.AddToFillers(p_node.actor, p_node.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, value, LOG_IDENTIFIER.STRING_1);
		p_node.LogAction(log, ignoreShouldAddLog: true);
	}
}
