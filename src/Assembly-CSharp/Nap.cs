using Inner_Maps;
using UtilityScripts;

public class Nap : GoapAction
{
	public Nap()
		: base(INTERACTION_TYPE.NAP)
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
		SetState("Nap Success", goapNode);
	}

	public override GoapActionInvalidity IsInvalid(ActualGoapNode node)
	{
		GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
		Character actor = node.actor;
		IPointOfInterest poiTarget = node.poiTarget;
		if (!goapActionInvalidity.isInvalid)
		{
			if (!CanSleepInBed(actor, poiTarget as TileObject))
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "no_space_bed";
			}
			else if (!poiTarget.IsAvailable())
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "target_unavailable";
			}
			else if (poiTarget is Bed bed && bed.traitContainer.HasTrait("Wet"))
			{
				goapActionInvalidity.isInvalid = true;
				goapActionInvalidity.reason = "wet_bed";
			}
		}
		return goapActionInvalidity;
	}

	protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData)
	{
		int num = 0;
		if (actor.traitContainer.HasTrait("Enslaved") && (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)))
		{
			return 2000;
		}
		if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.isActiveMember && target.gridTileLocation != null && actor.gridTileLocation != null)
		{
			LocationGridTile centerGridTile = target.gridTileLocation.area.gridTileComponent.centerGridTile;
			float distanceTo = actor.gridTileLocation.area.gridTileComponent.centerGridTile.GetDistanceTo(centerGridTile);
			int num2 = InnerMapManager.AreaLocationGridTileSize.x * 3;
			if (distanceTo > (float)num2)
			{
				return 2000;
			}
		}
		if (target is BaseBed baseBed)
		{
			if (!baseBed.IsSlotAvailable())
			{
				num += 2000;
			}
			else if (actor.traitContainer.HasTrait("Travelling"))
			{
				num += 100;
			}
			else if (baseBed.traitContainer.HasTrait("Wet"))
			{
				num += 2000;
			}
			else
			{
				num = ((baseBed.IsOwnedBy(actor) || baseBed.structureLocation == actor.homeStructure) ? (num + Utilities.Rng.Next(30, 51)) : ((!actor.needsComponent.isExhausted) ? (num + 2000) : (baseBed.IsInHomeStructureOfCharacterWithOpinion(actor, "Close Friend", "Friend") ? (num + Utilities.Rng.Next(130, 151)) : ((!baseBed.IsInHomeStructureOfCharacterWithOpinion(actor, "Rival", "Enemy")) ? Utilities.Rng.Next(80, 101) : (num + 2000)))));
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
						num += 20;
						break;
					case "Acquaintance":
						num += 25;
						break;
					default:
						if (!(opinionLabel == string.Empty))
						{
							break;
						}
						goto case "Enemy";
					case "Enemy":
					case "Rival":
						num += 100;
						break;
					}
				}
			}
		}
		return num;
	}

	public override void OnStopWhilePerforming(ActualGoapNode node)
	{
		base.OnStopWhilePerforming(node);
		Character actor = node.actor;
		actor.traitContainer.RemoveTrait(actor, "Resting");
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
			if (poiTarget.IsAvailable() && CanSleepInBed(actor, poiTarget as TileObject))
			{
				return poiTarget.gridTileLocation != null;
			}
			return false;
		}
		return false;
	}

	public void PreNapSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
	}

	public void PerTickNapSuccess(ActualGoapNode goapNode)
	{
		CharacterNeedsComponent needsComponent = goapNode.actor.needsComponent;
		if (needsComponent.HasNeeds())
		{
			needsComponent.AdjustTiredness(1f);
		}
	}

	public void AfterNapSuccess(ActualGoapNode goapNode)
	{
		goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
	}

	private bool CanSleepInBed(Character character, TileObject tileObject)
	{
		return (tileObject as BaseBed).CanUseBed(character);
	}
}
