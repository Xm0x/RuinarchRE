using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;

namespace Traits;

public class Necromancer : Trait
{
	public const int MaxSkeletonFollowers = 25;

	public Character owner { get; private set; }

	public LocationStructure lairStructure { get; private set; }

	public NPCSettlement attackVillageTarget { get; private set; }

	public string prevClassName { get; private set; }

	public int lifeAbsorbed { get; private set; }

	public int energy { get; private set; }

	public bool doNotSpawnLair { get; private set; }

	public GameDate spawnLairDate { get; private set; }

	public int numOfSkeletonFollowers => GetNumOfSkeletonFollowersInSameRegion();

	public override Type serializedData => typeof(SaveDataNecromancer);

	public override bool shouldBeLoadedInMainThread => true;

	public override bool affectsNameIcon => true;

	public Necromancer()
	{
		name = "Necromancer";
		description = "This is a necromancer.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.BUILD_LAIR,
			INTERACTION_TYPE.SPAWN_SKELETON,
			INTERACTION_TYPE.READ_NECRONOMICON,
			INTERACTION_TYPE.MEDITATE,
			INTERACTION_TYPE.REGAIN_ENERGY
		};
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataNecromancer saveDataNecromancer = saveDataTrait as SaveDataNecromancer;
		if (!string.IsNullOrEmpty(saveDataNecromancer.lairStructureID))
		{
			lairStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentIDSafe(saveDataNecromancer.lairStructureID);
		}
		if (!string.IsNullOrEmpty(saveDataNecromancer.attackVillageID))
		{
			attackVillageTarget = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(saveDataNecromancer.attackVillageID) as NPCSettlement;
		}
		prevClassName = saveDataNecromancer.prevClassName;
		lifeAbsorbed = saveDataNecromancer.lifeAbsorbed;
		energy = saveDataNecromancer.energy;
		doNotSpawnLair = saveDataNecromancer.doNotSpawnLair;
		spawnLairDate = saveDataNecromancer.spawnLairDate;
	}

	public override void LoadTraitSecondWaveInMainThread(SaveDataTrait p_saveDataTrait)
	{
		base.LoadTraitSecondWaveInMainThread(p_saveDataTrait);
		if (doNotSpawnLair)
		{
			SchedulingManager.Instance.AddEntry(spawnLairDate, delegate
			{
				SetDoNotSpawnLair(p_state: false);
			}, null);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		owner = addedTo as Character;
		prevClassName = owner.characterClass.className;
		owner.behaviourComponent.AddBehaviourComponent(typeof(NecromancerBehaviour));
		owner.SetNecromancerTrait(this);
		AdjustEnergy(4);
		owner.jobQueue.CancelAllJobs();
		owner.movementComponent.SetEnableDigging(state: true);
		owner.movementComponent.SetAvoidSettlements(state: true);
		owner.traitContainer.RemoveTrait(owner, "Malnourished");
		owner.needsComponent.ResetFullnessMeter();
		owner.needsComponent.ResetTirednessMeter();
		owner.needsComponent.ResetHappinessMeter();
		owner.needsComponent.ResetStaminaMeter();
		owner.needsComponent.ResetHopeMeter();
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)owner);
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character character)
		{
			owner = character;
			CharacterManager.Instance.SetNecromancerInTheWorld(owner);
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		owner.behaviourComponent.RemoveBehaviourComponent(typeof(NecromancerBehaviour));
		owner.SetNecromancerTrait(null);
		if (FactionManager.Instance.undeadFaction.leader == owner)
		{
			FactionManager.Instance.undeadFaction.OnlySetLeader(null);
		}
		CharacterManager.Instance.SetNecromancerInTheWorld(null);
		owner.movementComponent.SetEnableDigging(state: false);
		owner.movementComponent.SetAvoidSettlements(state: false);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)owner);
		owner = null;
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character || targetPOI is Tombstone)
		{
			Character character = null;
			if (targetPOI is Character character2)
			{
				if (character2 is Summon summon)
				{
					if (!summon.hasBeenRaisedFromDead)
					{
						character = character2;
					}
				}
				else
				{
					character = character2;
				}
			}
			else if (targetPOI is Tombstone tombstone)
			{
				character = tombstone.character;
			}
			if (character != null && character.isDead && character.hasMarker && numOfSkeletonFollowers < 25)
			{
				return characterThatWillDoJob.jobComponent.TriggerRaiseCorpse(character);
			}
		}
		return false;
	}

	public void SetLairStructure(LocationStructure structure)
	{
		lairStructure = structure;
	}

	public void AdjustLifeAbsorbed(int amount)
	{
		lifeAbsorbed += amount;
	}

	public void AdjustEnergy(int amount)
	{
		if (amount > 0)
		{
			int num = 10 - energy;
			if (amount > num)
			{
				amount = num;
			}
		}
		else if (amount < 0 && amount * -1 > energy)
		{
			amount = -energy;
		}
		energy += amount;
		owner.combatComponent.AdjustIntelligenceModifier(amount * 2);
		owner.piercingAndResistancesComponent.AdjustBasePiercing(amount * 5);
		owner.piercingAndResistancesComponent.AdjustAllResistances(amount * 5);
	}

	private int GetNumOfSkeletonFollowersInSameRegion()
	{
		int num = 0;
		for (int i = 0; i < owner.faction.characters.Count; i++)
		{
			Character character = owner.faction.characters[i];
			if (!character.isDead && character.race == RACE.SKELETON && character.currentRegion == owner.currentRegion)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumOfSkeletonFollowersThatAreNotAttackingAndIsAlive()
	{
		int num = 0;
		for (int i = 0; i < owner.faction.characters.Count; i++)
		{
			Character character = owner.faction.characters[i];
			if (character.race == RACE.SKELETON && !character.isDead && !character.behaviourComponent.HasBehaviour(typeof(AttackVillageBehaviour)))
			{
				num++;
			}
		}
		return num;
	}

	public void SetAttackVillageTarget(NPCSettlement npcSettlement)
	{
		attackVillageTarget = npcSettlement;
	}

	public void SetDoNotSpawnLair(bool p_state)
	{
		if (doNotSpawnLair == p_state)
		{
			return;
		}
		doNotSpawnLair = p_state;
		if (!owner.isDead && doNotSpawnLair)
		{
			spawnLairDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
			SchedulingManager.Instance.AddEntry(spawnLairDate, delegate
			{
				SetDoNotSpawnLair(p_state: false);
			}, null);
		}
	}

	public void DisconnectFromStructure(LocationStructure p_structure)
	{
		if (lairStructure == p_structure)
		{
			SetLairStructure(null);
		}
	}

	public override void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		base.CheckIfStructureIsStillReferenced(p_structure);
		_ = lairStructure;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
