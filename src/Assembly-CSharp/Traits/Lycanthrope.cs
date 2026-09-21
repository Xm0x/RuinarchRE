using System.Collections.Generic;
using Locations.Settlements;
using Object_Pools;
using UnityEngine;
using UtilityScripts;

namespace Traits;

public class Lycanthrope : Trait
{
	private Collider2D[] _triggerFlawNearbyTargets;

	public Character owner { get; private set; }

	public override bool isPersistent => true;

	public Lycanthrope()
	{
		name = "Lycanthrope";
		description = "Sometimes turns to plain wolf when it sleeps";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		canBeTriggered = true;
		AddTraitOverrideFunctionIdentifier("Per_Tick_While_Stationary_Unoccupied");
		AddTraitOverrideFunctionIdentifier("See_Poi_Cannot_Witness_Trait");
		advertisedInteractions = new List<INTERACTION_TYPE> { INTERACTION_TYPE.DISPEL };
		_triggerFlawNearbyTargets = new Collider2D[100];
	}

	public override void OnAddTrait(ITraitable sourceCharacter)
	{
		if (sourceCharacter is Character character)
		{
			owner = character;
		}
		base.OnAddTrait(sourceCharacter);
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
		}
	}

	public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy)
	{
		base.OnRemoveTrait(sourceCharacter, removedBy);
		owner.RevertFromWerewolfForm();
		owner.lycanData.EraseThisDataWhenTraitIsRemoved(owner);
		owner = null;
	}

	public override string GetTestingData(ITraitable traitable = null)
	{
		string text = base.GetTestingData(traitable);
		if (traitable is Character character)
		{
			text = $"{text}Is Master: {character.lycanData.isMaster}";
		}
		return text;
	}

	protected override string GetDescriptionInUI()
	{
		string localizedValue = base.GetDescriptionInUI();
		if (owner.lycanData.isMaster)
		{
			localizedValue = LocalizationManager.Instance.GetLocalizedValue("Traits_Table", "Lycanthrope_Master");
		}
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Traits", "Traits_Table", owner.lycanData.dislikesBeingLycan ? "Loathes_Trait" : "Enjoys_Trait");
		log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(null, base.localizedName, LOG_IDENTIFIER.STRING_1);
		localizedValue = localizedValue + "\n" + log.logText;
		LogPool.Release(log);
		if (owner.lycanData.awareCharacters.Count > 0)
		{
			localizedValue = localizedValue + "\n" + LocalizationManager.Instance.GetLocalizedValue("Traits_Table", "Aware") + ": ";
			for (int i = 0; i < owner.lycanData.awareCharacters.Count; i++)
			{
				Character character = owner.lycanData.awareCharacters[i];
				localizedValue += character.visuals.GetCharacterNameWithIconAndColor();
				if (!CollectionUtilities.IsLastIndex(owner.lycanData.awareCharacters, i))
				{
					localizedValue += ", ";
				}
			}
		}
		return localizedValue;
	}

	public override void OnSeePOIEvenCannotWitness(IPointOfInterest targetPOI, Character character)
	{
		base.OnSeePOIEvenCannotWitness(targetPOI, character);
		if (IsHuntingForPrey() && targetPOI is Character { isDead: false } character2 && !owner.combatComponent.IsInActualCombatWith(character2) && owner.relationshipContainer.IsFriendsWith(character2) && CrimeManager.Instance.GetCrimeSeverity(character2, owner, owner, CRIME_TYPE.Werewolf).IsConsideredACrime())
		{
			owner.jobQueue.GetJob(JOB_TYPE.LYCAN_HUNT_PREY)?.ForceCancelJob("Avoiding_Discovery");
			owner.crimeComponent.FleeToAllVillagerInRangeThatConsidersCrimeTypeACrime(owner, CRIME_TYPE.Werewolf, LocalizationManager.Instance.GetLocalizedValue("CancelReasons_Table", "Avoiding_Discovery"));
		}
	}

	public override bool PerTickWhileStationaryOrUnoccupied(Character p_character)
	{
		if (p_character.lycanData != null)
		{
			if (p_character.hasMarker && p_character.marker.isMoving && (p_character.lycanData.activeForm == p_character.lycanData.lycanthropeForm || p_character.lycanData.isInWerewolfForm))
			{
				float chance = 0.85f;
				if (p_character.currentRegion.tileObjectsComponent.GetTileObjectInRegionCount(TILE_OBJECT_TYPE.WEREWOLF_PELT) >= 3)
				{
					chance = 0.5f;
				}
				if (GameUtilities.RollChance(chance) && p_character.gridTileLocation.tileObjectComponent.objHere == null)
				{
					Messenger.Broadcast(CharacterSignals.LYCANTHROPE_SHED_WOLF_PELT, p_character);
					p_character.interruptComponent.TriggerInterrupt(INTERRUPT.Shed_Pelt, p_character);
				}
			}
			if (p_character.needsComponent.isStarving && p_character.lycanData.isMaster && !IsHuntingForPrey() && GameUtilities.RollChance(1))
			{
				Character huntPreyTarget = GetHuntPreyTarget();
				if (huntPreyTarget != null)
				{
					p_character.jobComponent.TriggerHuntPreyJob(huntPreyTarget);
				}
			}
			if (p_character.lycanData.dislikesBeingLycan && GameUtilities.RollChance(1) && IsHuntingForPrey())
			{
				ResistHunger();
			}
		}
		return false;
	}

	private void ResistHunger()
	{
		owner.jobQueue.GetJob(JOB_TYPE.LYCAN_HUNT_PREY)?.ForceCancelJob("Resisted_Hunger");
		owner.traitContainer.AddTrait(owner, "Ashamed");
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", "Lycanthrope resist_hunger", LOG_TAG.Needs);
		log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddLogToDatabase(releaseLogAfter: true);
		if (owner.lycanData.isInWerewolfForm)
		{
			owner.interruptComponent.TriggerInterrupt(INTERRUPT.Revert_From_Werewolf, owner);
		}
	}

	private bool IsHuntingForPrey()
	{
		if (owner.currentJob is GoapPlanJob && owner.currentJob.jobType == JOB_TYPE.LYCAN_HUNT_PREY)
		{
			return true;
		}
		if (owner.currentJob is CharacterStateJob && owner.stateComponent.currentState is CombatState { currentClosestHostile: not null } combatState)
		{
			CombatData combatData = owner.combatComponent.GetCombatData(combatState.currentClosestHostile);
			if (combatData != null && combatData.connectedAction != null && combatData.connectedAction.associatedJobType == JOB_TYPE.LYCAN_HUNT_PREY)
			{
				return true;
			}
		}
		return false;
	}

	public override string TriggerFlaw(Character character, bool isTriggeredByPlayer = true)
	{
		if (!TryDoLycanBehaviour(p_isFlawTriggered: true))
		{
			return "fail_no_target";
		}
		return base.TriggerFlaw(character);
	}

	public void CheckIfAlone()
	{
		TryDoLycanBehaviour();
	}

	private bool TryDoLycanBehaviour(bool p_isFlawTriggered = false)
	{
		if (owner.lycanData.isMaster)
		{
			return TryMasterLycanHuntPrey(p_isFlawTriggered);
		}
		if (IsAlone())
		{
			DoTriggerFlawTransform();
		}
		else
		{
			GoOutsideForStealthTransform();
		}
		return true;
	}

	private bool IsAlone()
	{
		return !owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Werewolf);
	}

	private void DoTriggerFlawTransform()
	{
		owner.lycanData.Transform(owner);
	}

	private void GoOutsideForStealthTransform()
	{
		OtherData[] array = null;
		BaseSettlement currentSettlement = owner.currentSettlement;
		if (currentSettlement != null && currentSettlement.locationType == LOCATION_TYPE.VILLAGE)
		{
			array = new OtherData[1]
			{
				new SettlementOtherData(currentSettlement)
			};
		}
		else
		{
			Area area = owner.areaLocation.neighbourComponent.GetRandomAdjacentNoSettlementHextileWithinRegion();
			if (area == null)
			{
				area = owner.areaLocation;
			}
			array = new OtherData[1]
			{
				new AreaOtherData(area)
			};
		}
		owner.PlanFixedJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEALTH_TRANSFORM, owner, array);
	}

	private bool TryMasterLycanHuntPrey(bool isTriggeredByPlayer, bool p_isFlawTriggered = false)
	{
		Character huntPreyTarget = GetHuntPreyTarget();
		if (huntPreyTarget != null)
		{
			owner.jobQueue.CancelAllJobs();
			return owner.jobComponent.TriggerHuntPreyJob(huntPreyTarget, p_isFlawTriggered);
		}
		return false;
	}

	private Character GetHuntPreyTarget()
	{
		WeightedDictionary<Character> weightedDictionary = new WeightedDictionary<Character>();
		int num = 0;
		int num2 = Physics2D.OverlapCircleNonAlloc(owner.worldPosition, 20f, _triggerFlawNearbyTargets, GameUtilities.Filtered_Layer_Mask);
		for (int i = 0; i < num2; i++)
		{
			POIVisionTrigger component = _triggerFlawNearbyTargets[i].gameObject.GetComponent<POIVisionTrigger>();
			if (!(component != null) || !(component.poi is Character character) || character == owner || character.isDead)
			{
				continue;
			}
			int num3 = 0;
			if (character is Animal)
			{
				if (num >= 3)
				{
					continue;
				}
				num3 = 10;
				num++;
			}
			else if (character.race.IsSapient() && character.faction != owner.faction && !owner.isDead && !owner.relationshipContainer.IsFriendsWith(character) && owner.movementComponent.HasPathToEvenIfDiffRegion(character.gridTileLocation))
			{
				num3 = 10;
			}
			if (num3 > 0)
			{
				weightedDictionary.AddElement(character, num3);
			}
		}
		if (weightedDictionary.GetTotalOfWeights() > 0)
		{
			return weightedDictionary.PickRandomElementGivenWeights();
		}
		return null;
	}

	public override string GetTriggerFlawEffectDescription(Character character, string key)
	{
		if (owner.lycanData.isMaster)
		{
			key = "flaw_effect_master";
		}
		return base.GetTriggerFlawEffectDescription(character, key);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
