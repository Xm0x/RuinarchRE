using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class Sleep : GoapAction
{
	private const int SleepAtTavernCost = 33;

	public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;

	public Sleep()
		: base(INTERACTION_TYPE.SLEEP)
	{
		base.actionIconString = GoapActionStateDB.Sleep_Icon;
		base.logTags = new LOG_TAG[1] { LOG_TAG.Needs };
	}

	protected override void ConstructBasePreconditionsAndEffects()
	{
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
		AddExpectedEffect(InteractionManager.Instance.GetGoapEffectData(GOAP_EFFECT_CONDITION.STAMINA_RECOVERY, string.Empty, p_isKeyANumber: false, GOAP_EFFECT_TARGET.ACTOR));
	}

	public override void Perform(ActualGoapNode goapNode)
	{
		base.Perform(goapNode);
		SetState("Rest Success", goapNode);
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		if (actor.traitContainer.HasTrait("Enslaved") && (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)))
		{
			return 2000;
		}
		if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.isActiveMember && target.gridTileLocation != null && actor.gridTileLocation != null)
		{
			LocationGridTile centerGridTile = target.gridTileLocation.area.gridTileComponent.centerGridTile;
			float distanceTo = actor.gridTileLocation.area.gridTileComponent.centerGridTile.GetDistanceTo(centerGridTile);
			int num = InnerMapManager.AreaLocationGridTileSize.x * 3;
			if (distanceTo > (float)num)
			{
				return 2000;
			}
		}
		int num2 = 0;
		if (actor.movementComponent.ShouldAvoidStructureLocationOfTarget(target) && !actor.partyComponent.hasParty)
		{
			return 2000;
		}
		num2 = 0;
		if (target is BaseBed)
		{
			BaseBed baseBed = target as BaseBed;
			if (!baseBed.IsSlotAvailable())
			{
				num2 = ((actor == null || !baseBed.users.Contains(actor)) ? (num2 + 2000) : 10);
			}
			else if (actor.traitContainer.HasTrait("Travelling"))
			{
				num2 += 100;
			}
			else if (baseBed.traitContainer.HasTrait("Wet"))
			{
				num2 += 2000;
			}
			else
			{
				if (baseBed.IsOwnedBy(actor) || baseBed.structureLocation == actor.homeStructure)
				{
					num2 = ((!actor.needsComponent.isExhausted && !actor.traitContainer.HasTrait("Drunk")) ? (num2 + Utilities.Rng.Next(5, 16)) : (num2 + Utilities.Rng.Next(30, 51)));
				}
				else if (baseBed.structureLocation != null && baseBed.structureLocation.structureType == STRUCTURE_TYPE.TAVERN)
				{
					if (actor.homeStructure != null && actor.currentSettlement != null && actor.currentSettlement == actor.homeSettlement)
					{
						num2 += 2000;
					}
					else
					{
						BaseSettlement currentSettlement = baseBed.currentSettlement;
						num2 = ((currentSettlement != null && currentSettlement.owner != null && currentSettlement.owner.IsHostileWith(actor.faction)) ? (num2 + 2000) : ((!actor.moneyComponent.CanAfford(33)) ? (num2 + 2000) : (num2 + Utilities.Rng.Next(20, 26))));
					}
				}
				else if (actor.needsComponent.isExhausted)
				{
					BaseSettlement settlement = null;
					num2 = (baseBed.IsInHomeStructureOfCharacterWithOpinion(actor, "Close Friend", "Friend") ? (num2 + Utilities.Rng.Next(130, 151)) : (baseBed.IsInHomeStructureOfCharacterWithOpinion(actor, "Rival", "Enemy") ? (num2 + 2000) : ((baseBed.gridTileLocation == null || !baseBed.gridTileLocation.IsPartOfSettlement(out settlement) || settlement.owner == null || settlement.owner == actor.faction) ? Utilities.Rng.Next(80, 101) : (num2 + 200))));
				}
				else
				{
					num2 += 2000;
				}
				Character character = null;
				for (int i = 0; i < baseBed.users.Length; i++)
				{
					if (baseBed.users[i] != null)
					{
						character = baseBed.users[i];
						break;
					}
				}
				if (character != null)
				{
					string opinionLabel = actor.relationshipContainer.GetOpinionLabel(character);
					switch (opinionLabel)
					{
					case "Friend":
						num2 += 20;
						break;
					case "Acquaintance":
						num2 += 25;
						break;
					default:
						if (!(opinionLabel == string.Empty))
						{
							break;
						}
						goto case "Enemy";
					case "Enemy":
					case "Rival":
						num2 += 100;
						break;
					}
				}
			}
		}
		return num2;
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		actor.traitContainer.RemoveTrait(actor, "Resting");
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		_ = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			if (!poiTarget.IsAvailable())
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "no_space_bed";
			}
			else if (poiTarget is Bed bed && bed.traitContainer.HasTrait("Wet"))
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "wet_bed";
			}
		}
		return goapActionInvalidity;
	}

	protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job)
	{
		if (base.AreRequirementsSatisfied(actor, poiTarget, otherData, job))
		{
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure))
			{
				return false;
			}
			if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area))
			{
				return false;
			}
			if (poiTarget.IsAvailable())
			{
				return poiTarget.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	public void PreRestSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
		goapNode.actor.CancelAllJobsExceptForCurrent();
		LocationStructure locationStructure = goapNode.poiTarget.gridTileLocation?.structure;
		if (locationStructure != null && locationStructure.structureType == STRUCTURE_TYPE.TAVERN && locationStructure is ManMadeStructure manMadeStructure)
		{
			goapNode.actor.moneyComponent.AdjustCoins(-33);
			if (manMadeStructure.HasAssignedWorker())
			{
				string id = manMadeStructure.assignedWorkerIDs[0];
				DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(id).moneyComponent.AdjustCoins(33);
			}
		}
	}

	public void PerTickRestSuccess(ActualGoapNode goapNode)
	{
		CharacterNeedsComponent needsComponent = goapNode.actor.needsComponent;
		if (needsComponent.HasNeeds())
		{
			needsComponent.AdjustTiredness(0.417f);
		}
	}

	public void AfterRestSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
	}
}
