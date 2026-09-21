using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;

namespace Traits;

public class Edible : Trait
{
	private IPointOfInterest owner;

	public Edible()
	{
		name = "Edible";
		description = "Yummy.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.EAT,
			INTERACTION_TYPE.POISON
		};
		AddTraitOverrideFunctionIdentifier("Execute_Pre_Effect_Trait");
		AddTraitOverrideFunctionIdentifier("Execute_Pre_Effect_Trait");
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is IPointOfInterest pointOfInterest)
		{
			owner = pointOfInterest;
		}
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is IPointOfInterest pointOfInterest)
		{
			owner = pointOfInterest;
		}
	}

	public override void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode)
	{
		base.ExecuteActionPerTickEffects(action, goapNode);
		if (action != INTERACTION_TYPE.EAT)
		{
			return;
		}
		if (goapNode.actor.needsComponent.HasNeeds())
		{
			goapNode.actor.needsComponent.AdjustFullness(5f, 0.08f);
		}
		else if (goapNode.actor != null && !goapNode.actor.IsHealthFull())
		{
			int num = Mathf.CeilToInt((float)goapNode.actor.maxHP * 0.08f);
			if (num > 0)
			{
				goapNode.actor.AdjustHP(num, ELEMENTAL_TYPE.Normal);
			}
		}
		if (goapNode.hasBeenReset)
		{
			return;
		}
		if (owner is Table table)
		{
			goapNode.actor.needsComponent.AdjustHappiness(0.67f);
			owner.resourceStorageComponent.ReduceMainResourceUsingRandomSpecificResources(RESOURCE.FOOD, 1);
			if (table.gridTileLocation != null && table.gridTileLocation.structure is Dwelling)
			{
				Messenger.Broadcast(StructureSignals.FOOD_IN_DWELLING_CHANGED, table);
			}
		}
		else if (owner is FoodPile foodPile)
		{
			goapNode.actor.needsComponent.AdjustHappiness(-0.83f);
			foodPile.AdjustResourceInPile(-1, shouldBeDestroyed: false);
		}
	}

	public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, OtherData[] otherData, ref int cost)
	{
		base.ExecuteCostModification(action, actor, poiTarget, otherData, ref cost);
		if (action != INTERACTION_TYPE.EAT)
		{
			return;
		}
		switch (GetEdibleType())
		{
		case "Meat":
			if (actor.traitContainer.HasTrait("Carnivore"))
			{
				cost = 25;
			}
			else
			{
				cost = 50;
			}
			break;
		case "Plant":
			if (actor.traitContainer.HasTrait("Herbivore"))
			{
				cost = 25;
			}
			else
			{
				cost = 50;
			}
			break;
		case "Table":
		{
			Table table = owner as Table;
			if (table.structureLocation.isDwelling)
			{
				if (table.structureLocation == actor.homeStructure)
				{
					cost = 12;
				}
				else if (table.structureLocation.HasPositiveRelationshipWithAnyResident(actor))
				{
					cost = 18;
				}
				else if (!table.structureLocation.IsOccupied())
				{
					cost = 28;
				}
			}
			else
			{
				cost = 28;
			}
			break;
		}
		}
	}

	public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, Character actor, IPointOfInterest target, ACTION_CATEGORY category, ref bool isRemoved)
	{
		base.ExecuteActionAfterEffects(action, actor, target, category, ref isRemoved);
		if (action == INTERACTION_TYPE.EAT)
		{
			if (owner is Crops crops)
			{
				crops.SetGrowthState(Crops.Growth_State.Growing);
				isRemoved = true;
			}
			else if (owner is FoodPile { resourceInPile: <=0 } foodPile)
			{
				foodPile.DestroyResourcePile();
			}
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		owner = null;
	}

	private string GetEdibleType()
	{
		if (owner is Crops)
		{
			return "Plant";
		}
		if (owner is Table)
		{
			return "Table";
		}
		return "Meat";
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
