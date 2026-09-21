using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class CreateCultistKit : GoapAction
{
	private Precondition _stonePrecondition;

	private Precondition _woodPrecondition;

	public CreateCultistKit()
		: base(INTERACTION_TYPE.CREATE_CULTIST_KIT)
	{
		base.actionIconString = GoapActionStateDB.Work_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Work,
			LOG_TAG.Crimes
		};
		_stonePrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasStone);
		_woodPrecondition = new Precondition(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR), HasWood);
	}

	public override bool ShouldActionBeAnIntel(ActualGoapNode node)
	{
		return true;
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.HAS_POI, "Cultist Kit", p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden)
	{
		Precondition precondition = null;
		precondition = ((actor.homeSettlement == null) ? ((actor.race == RACE.HUMANS) ? _stonePrecondition : _woodPrecondition) : (actor.homeSettlement.HasStructure(STRUCTURE_TYPE.MINE) ? _stonePrecondition : _woodPrecondition));
		isOverridden = true;
		return precondition;
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Create Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		return 0;
	}

	public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness)
	{
		if (CrimeManager.Instance.GetCrimeSeverity(witness, node.actor, node.target, CRIME_TYPE.Demon_Worship).IsConsideredACrime())
		{
			return REACTABLE_EFFECT.Negative;
		}
		return base.GetReactableEffect(node, witness);
	}

	public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
		bool flag = false;
		if (CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Demon_Worship).IsConsideredACrime())
		{
			flag = true;
		}
		if (!witness.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship) && flag)
		{
			reactions.Add(EMOTION.Repulsed);
			if (witness.traitContainer.HasTrait("Coward"))
			{
				reactions.Add(EMOTION.Fear);
			}
			else
			{
				if (witness.traitContainer.HasTrait("Psychopath"))
				{
					return;
				}
				if (witness.classComponent.IsTargetRecognizedAsCultistByStalker(actor))
				{
					reactions.Add(EMOTION.Disapproval);
					return;
				}
				reactions.Add(EMOTION.Threatened);
				string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
				if (opinionLabel == "Close Friend")
				{
					reactions.Add(EMOTION.Despair);
				}
				else if (opinionLabel == "Friend")
				{
					reactions.Add(EMOTION.Shock);
				}
			}
		}
		else
		{
			reactions.Add(EMOTION.Approval);
			if (RelationshipManager.IsSexuallyCompatibleOneSided(witness, actor) && GameUtilities.RollChance(10 * witness.relationshipContainer.GetCompatibility(actor)))
			{
				reactions.Add(EMOTION.Arousal);
			}
		}
	}

	public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status)
	{
		string result = base.ReactionToActor(actor, target, witness, node, status);
		TileObject ownedItemOnTheGround = actor.GetOwnedItemOnTheGround(TILE_OBJECT_TYPE.CULTIST_KIT);
		if (ownedItemOnTheGround != null && ownedItemOnTheGround.gridTileLocation.structure.settlementLocation != null && ownedItemOnTheGround.gridTileLocation.structure.settlementLocation == witness.homeSettlement)
		{
			witness.jobComponent.TriggerDestroy(ownedItemOnTheGround, "Destroy_Cultist_Kit");
		}
		return result;
	}

	public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime)
	{
		return CRIME_TYPE.Demon_Worship;
	}

	public override CRIME_TYPE GetRawCrimeType(Character actor)
	{
		return CRIME_TYPE.Demon_Worship;
	}

	private bool HasWood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) is ResourcePile { resourceInPile: var resourceInPile })
		{
			return resourceInPile >= TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.CULTIST_KIT).mainRecipe.GetNeededAmountForIngredient(TILE_OBJECT_TYPE.WOOD_PILE);
		}
		return false;
	}

	private bool HasStone(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType)
	{
		if (actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) is ResourcePile { resourceInPile: var resourceInPile })
		{
			return resourceInPile >= TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.CULTIST_KIT).mainRecipe.GetNeededAmountForIngredient(TILE_OBJECT_TYPE.STONE_PILE);
		}
		return false;
	}

	public void AfterCreateSuccess(ActualGoapNode goapNode)
	{
		Character actor = goapNode.actor;
		if (actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) is StonePile stonePile)
		{
			actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.CULTIST_KIT));
			stonePile.AdjustResourceInPile(-10);
		}
		else if (actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) is WoodPile woodPile)
		{
			actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.CULTIST_KIT));
			woodPile.AdjustResourceInPile(-10);
		}
	}
}
