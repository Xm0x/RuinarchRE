using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps;

public class GridTileTileObjectComponent : LocationGridTileComponent
{
	private GameObject _landmineEffect;

	private GameObject _freezingTrapEffect;

	private GameObject _snareTrapEffect;

	public TileObject objHere { get; private set; }

	public TileObject hiddenObjHere { get; private set; }

	public GenericTileObject genericTileObject { get; private set; }

	public List<ThinWall> walls { get; private set; }

	public bool hasLandmine { get; private set; }

	public bool hasFreezingTrap { get; private set; }

	public bool hasSnareTrap { get; private set; }

	public bool isSeenByEyeWard { get; private set; }

	public bool isFreezingTrapPlayerSource { get; private set; }

	public bool isSnareTrapPlayerSource { get; private set; }

	public RACE[] freezingTrapExclusions { get; private set; }

	public RACE snareTrapExclusion { get; private set; }

	public Dictionary<TILE_OBJECT_TYPE, int> affectedByAOETileObjects { get; private set; }

	public GridTileTileObjectComponent()
	{
		walls = new List<ThinWall>();
	}

	public GridTileTileObjectComponent(SaveDataGridTileTileObjectComponent data)
	{
		walls = new List<ThinWall>();
		hasLandmine = data.hasLandmine;
		hasFreezingTrap = data.hasFreezingTrap;
		hasSnareTrap = data.hasSnareTrap;
		freezingTrapExclusions = data.freezingTrapExclusions;
		snareTrapExclusion = data.snareTrapExclusion;
		isSeenByEyeWard = data.isSeenByEyeWard;
		isFreezingTrapPlayerSource = data.isFreezingTrapPlayerSource;
		isSnareTrapPlayerSource = data.isSnareTrapPlayerSource;
		affectedByAOETileObjects = new Dictionary<TILE_OBJECT_TYPE, int>();
	}

	public void SetOccupyingObject(TileObject p_object)
	{
		objHere = p_object;
	}

	public void SetObjectHere(TileObject poi)
	{
		if (poi.isHidden)
		{
			if (poi.OccupiesTile())
			{
				hiddenObjHere = poi;
			}
			poi.SetGridTileLocation(base.owner);
			poi.OnPlacePOI();
			return;
		}
		bool flag = base.owner.IsPassable();
		if (poi.OccupiesTile())
		{
			SetOccupyingObject(poi);
		}
		poi.SetGridTileLocation(base.owner);
		poi.OnPlacePOI();
		base.owner.SetTileState(LocationGridTile.Tile_State.Occupied);
		if (!base.owner.IsPassable())
		{
			base.owner.structure.RemovePassableTile(base.owner);
			base.owner.area.gridTileComponent.RemovePassableTile(base.owner);
		}
		else if (base.owner.IsPassable() && !flag)
		{
			base.owner.structure.AddPassableTile(base.owner);
			base.owner.area.gridTileComponent.AddPassableTile(base.owner);
		}
	}

	public void LoadObjectHere(TileObject poi)
	{
		if (poi.isHidden)
		{
			if (poi.OccupiesTile())
			{
				hiddenObjHere = poi;
			}
			poi.SetGridTileLocation(base.owner);
			poi.OnLoadPlacePOI();
			return;
		}
		bool flag = base.owner.IsPassable();
		if (poi != null)
		{
			if (poi.OccupiesTile())
			{
				SetOccupyingObject(poi);
			}
		}
		else
		{
			SetOccupyingObject(poi);
		}
		poi.SetGridTileLocation(base.owner);
		poi.OnLoadPlacePOI();
		base.owner.SetTileState(LocationGridTile.Tile_State.Occupied);
		if (!base.owner.IsPassable())
		{
			base.owner.structure.RemovePassableTile(base.owner);
		}
		else if (base.owner.IsPassable() && !flag)
		{
			base.owner.structure.AddPassableTile(base.owner);
		}
	}

	public TileObject RemoveObjectHere(Character removedBy, bool isPlayerSource)
	{
		if (objHere != null)
		{
			TileObject tileObject = objHere;
			SetOccupyingObject(null);
			tileObject.RemoveTileObject(removedBy);
			base.owner.eventDispatcher.ExecuteTileObjectRemovedEvent(tileObject, base.owner);
			base.owner.SetTileState(LocationGridTile.Tile_State.Empty);
			if (removedBy != null && removedBy.faction != null && removedBy.faction.isPlayerFaction)
			{
				isPlayerSource = true;
			}
			if (isPlayerSource && tileObject is ResourcePile)
			{
				PlayerManager.Instance?.player?.retaliationComponent.ResourcePileRetaliation(tileObject, base.owner);
				PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_DESTROY_RESOURCE);
			}
			Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI, tileObject);
			Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)tileObject);
			tileObject.DestroyPermanently();
			return tileObject;
		}
		return null;
	}

	public TileObject RemoveHiddenObjectHere(Character removedBy)
	{
		if (hiddenObjHere != null)
		{
			TileObject tileObject = hiddenObjHere;
			hiddenObjHere = null;
			tileObject.RemoveTileObject(removedBy);
			base.owner.eventDispatcher.ExecuteTileObjectRemovedEvent(tileObject, base.owner);
			Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI, tileObject);
			Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)tileObject);
			tileObject.DestroyPermanently();
			return tileObject;
		}
		return null;
	}

	public void RemoveHiddenObjectThatDoesntOccupyTile(TileObject p_tileObject)
	{
		p_tileObject.RemoveTileObject(null);
		base.owner.eventDispatcher.ExecuteTileObjectRemovedEvent(p_tileObject, base.owner);
		Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI, p_tileObject);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)p_tileObject);
		p_tileObject.DestroyPermanently();
	}

	public TileObject RemoveObjectHereWithoutDestroying()
	{
		if (objHere != null)
		{
			TileObject tileObject = objHere;
			objHere.SetGridTileLocation(null);
			SetOccupyingObject(null);
			tileObject.previousTile?.area.OnRemovePOIInHex(tileObject);
			base.owner.eventDispatcher.ExecuteTileObjectRemovedEvent(tileObject, base.owner);
			base.owner.SetTileState(LocationGridTile.Tile_State.Empty);
			tileObject.OnRemoveTileObject(null, base.owner, removeTraits: false, destroyTileSlots: false);
			tileObject.SetPOIState(POI_STATE.INACTIVE);
			Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)tileObject);
			return tileObject;
		}
		return null;
	}

	public TileObject RemoveObjectHereDestroyVisualOnly(Character remover = null)
	{
		if (objHere != null)
		{
			TileObject tileObject = objHere;
			objHere.SetGridTileLocation(null);
			SetOccupyingObject(null);
			tileObject.previousTile?.area.OnRemovePOIInHex(tileObject);
			base.owner.eventDispatcher.ExecuteTileObjectRemovedEvent(tileObject, base.owner);
			base.owner.SetTileState(LocationGridTile.Tile_State.Empty);
			tileObject.OnRemoveTileObject(null, base.owner, removeTraits: false, destroyTileSlots: false);
			tileObject.DestroyMapVisualGameObject();
			tileObject.SetPOIState(POI_STATE.INACTIVE);
			bool flag = false;
			if (remover != null && remover.faction != null && remover.faction.isPlayerFaction)
			{
				flag = true;
			}
			if (flag && tileObject is ResourcePile)
			{
				PlayerManager.Instance?.player?.retaliationComponent.ResourcePileRetaliation(tileObject, base.owner);
				PlayerManager.Instance?.player?.goalComponent.CompleteSubGoal(SUB_GOAL.GOAL_DESTROY_RESOURCE);
			}
			Messenger.Broadcast(CharacterSignals.STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR, tileObject, remover);
			Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)tileObject);
			return tileObject;
		}
		return null;
	}

	public void CreateGenericTileObject()
	{
		genericTileObject = new GenericTileObject(base.owner);
	}

	public void LoadGenericTileObject(GenericTileObject genericTileObject)
	{
		this.genericTileObject = genericTileObject;
	}

	public void AddWallObject(ThinWall structureWallObject)
	{
		walls.Add(structureWallObject);
	}

	public void ClearWallObjects()
	{
		walls.Clear();
	}

	public bool HasWalls()
	{
		if (objHere is BlockWall || objHere is IceBlockWall || objHere is OreVein)
		{
			return true;
		}
		if (walls.Count > 0)
		{
			for (int i = 0; i < walls.Count; i++)
			{
				if (walls[i].currentHP > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasThinWall()
	{
		if (walls.Count > 0)
		{
			for (int i = 0; i < walls.Count; i++)
			{
				if (walls[i].currentHP > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public TileObject GetFirstWall()
	{
		if (objHere is BlockWall || objHere is IceBlockWall || objHere is OreVein)
		{
			return objHere;
		}
		if (walls.Count > 0)
		{
			for (int i = 0; i < walls.Count; i++)
			{
				if (walls[i].currentHP > 0)
				{
					return walls[i];
				}
			}
		}
		return null;
	}

	public void SetHasLandmine(bool state)
	{
		if (hasLandmine != state)
		{
			base.owner.SetIsDefault(state: false);
			hasLandmine = state;
			if (hasLandmine)
			{
				_landmineEffect = GameManager.Instance.CreateParticleEffectAt(base.owner, PARTICLE_EFFECT.Landmine, 39);
				return;
			}
			base.owner.tileObjectComponent.genericTileObject.traitContainer.RemoveStatusAndStacks(base.owner.tileObjectComponent.genericTileObject, "Landmined");
			ObjectPoolManager.Instance.DestroyObject(_landmineEffect);
			_landmineEffect = null;
		}
	}

	public bool IsImmuneToLandmine(Character p_character, Landmined p_status)
	{
		if (!(p_character.characterClass.className == "Archer"))
		{
			return p_status.awareCharacters.Contains(p_character);
		}
		return true;
	}

	public IEnumerator TriggerLandmine(Character triggeredBy)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "InterventionAbility", "PlayerPowerAlerts_Table", "Landmine trap_activated", LOG_TAG.Player);
		log.AddToFillers(triggeredBy, triggeredBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFrom(triggeredBy, log, releaseLogAfter: true);
		GameManager.Instance.CreateParticleEffectAt(base.owner, PARTICLE_EFFECT.Landmine_Explosion);
		CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, triggeredBy, genericTileObject, REACTION_STATUS.WITNESSED);
		SetHasLandmine(state: false);
		if (triggeredBy.isNormalAndNotAlliedWithPlayer)
		{
			Messenger.Broadcast(PlayerSkillSignals.ON_TRAP_ACTIVATED_ON_VILLAGER, triggeredBy, PLAYER_SKILL_TYPE.LANDMINE, arg3: true);
		}
		yield return GameUtilities.waitForHalfSecond;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		base.owner.PopulateTilesInRadius(list, 1, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.LANDMINE);
		int damageBaseOnLevel = PlayerSkillManager.Instance.GetDamageBaseOnLevel(spellData);
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			List<ITraitable> list2 = RuinarchListPool<ITraitable>.Claim();
			locationGridTile.PopulateAliveTraitablesOnTile(list2);
			for (int j = 0; j < list2.Count; j++)
			{
				ITraitable traitable = list2[j];
				if (traitable.gridTileLocation == null)
				{
					continue;
				}
				if (traitable is TileObject tileObject)
				{
					if (tileObject.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT)
					{
						int amount = -damageBaseOnLevel;
						float piercingPower = pierceBasedOnCurrentLevel;
						tileObject.AdjustHP(amount, ELEMENTAL_TYPE.Normal, triggerDeath: true, spellData, null, showHPBar: true, piercingPower, isPlayerSource: true);
					}
					else
					{
						CombatManager.Instance.ApplyElementalDamage(0, ELEMENTAL_TYPE.Normal, tileObject, null, null, createHitEffect: true, setAsPlayerSource: true, pierceBasedOnCurrentLevel);
					}
				}
				else if (traitable is Character character)
				{
					int amount2 = -damageBaseOnLevel;
					float piercingPower = pierceBasedOnCurrentLevel;
					character.AdjustHP(amount2, ELEMENTAL_TYPE.Normal, triggerDeath: true, spellData, null, showHPBar: true, piercingPower, isPlayerSource: true);
					character.OnCharacterHitByPlayerSpell(-damageBaseOnLevel);
					if (character.isDead && character.skillCauseOfDeath == PLAYER_SKILL_TYPE.NONE)
					{
						character.skillCauseOfDeath = PLAYER_SKILL_TYPE.LANDMINE;
					}
				}
				else
				{
					int amount3 = -damageBaseOnLevel;
					float piercingPower = pierceBasedOnCurrentLevel;
					traitable.AdjustHP(amount3, ELEMENTAL_TYPE.Normal, triggerDeath: true, spellData, null, showHPBar: true, piercingPower, isPlayerSource: true);
				}
			}
			RuinarchListPool<ITraitable>.Release(list2);
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	public void SetHasFreezingTrap(bool state, bool isPlayerSource, params RACE[] freezingTrapExclusions)
	{
		isFreezingTrapPlayerSource = isPlayerSource;
		if (hasFreezingTrap != state)
		{
			base.owner.SetIsDefault(state: false);
			hasFreezingTrap = state;
			if (hasFreezingTrap)
			{
				base.owner.area.AddFreezingTrapInArea();
				this.freezingTrapExclusions = freezingTrapExclusions;
				_freezingTrapEffect = GameManager.Instance.CreateParticleEffectAt(base.owner, PARTICLE_EFFECT.Freezing_Trap, 39);
			}
			else
			{
				base.owner.tileObjectComponent.genericTileObject.traitContainer.RemoveStatusAndStacks(base.owner.tileObjectComponent.genericTileObject, "Freezing Trapped");
				base.owner.area.RemoveFreezingTrapInArea();
				ObjectPoolManager.Instance.DestroyObject(_freezingTrapEffect);
				_freezingTrapEffect = null;
				this.freezingTrapExclusions = null;
			}
		}
	}

	public bool IsImmuneToFreezingTrap(Character p_character, FreezingTrapped p_status)
	{
		if (!(p_character.characterClass.className == "Archer") && (freezingTrapExclusions == null || !freezingTrapExclusions.Contains(p_character.race)))
		{
			return p_status.awareCharacters.Contains(p_character);
		}
		return true;
	}

	public void TriggerFreezingTrap(Character triggeredBy)
	{
		bool flag = false;
		int p_value = 100;
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.FREEZING_TRAP);
		RESISTANCE resistanceType = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<FreezingTrapSkillData>(PLAYER_SKILL_TYPE.FREEZING_TRAP).resistanceType;
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		float resistanceValue = triggeredBy.piercingAndResistancesComponent.GetResistanceValue(resistanceType);
		CombatManager.ModifyValueByPiercingAndResistance(ref p_value, pierceBasedOnCurrentLevel, resistanceValue);
		string log = string.Empty;
		if (GameUtilities.RollChance(p_value, ref log))
		{
			flag = true;
		}
		bool flag2 = isFreezingTrapPlayerSource;
		int durationBonusPerLevel = PlayerSkillManager.Instance.GetDurationBonusPerLevel(spellData);
		if (flag)
		{
			if (triggeredBy is Summon summon)
			{
				if (summon.summonType == SUMMON_TYPE.Kobold)
				{
					durationBonusPerLevel = PlayerSkillManager.Instance.GetDurationBonusPerLevel(spellData, 0);
				}
			}
			else if (triggeredBy.isNormalAndNotAlliedWithPlayer)
			{
				Messenger.Broadcast(PlayerSkillSignals.ON_TRAP_ACTIVATED_ON_VILLAGER, triggeredBy, PLAYER_SKILL_TYPE.FREEZING_TRAP, flag2);
			}
			GameObject in_gameObjectID = GameManager.Instance.CreateParticleEffectAt(triggeredBy, PARTICLE_EFFECT.Freezing_Trap_Explosion);
			AkSoundEngine.PostEvent("Play_Freezing_Trap_Explosion", in_gameObjectID);
		}
		SetHasFreezingTrap(false, false);
		if (flag)
		{
			triggeredBy.traitContainer.RemoveStatusAndStacks(triggeredBy, "Freezing");
			triggeredBy.traitContainer.AddTrait(triggeredBy, "Frozen", null, bypassElementalChance: true, durationBonusPerLevel);
			triggeredBy.traitContainer.GetTraitOrStatus<Frozen>("Frozen")?.SetIsPlayerSource(flag2);
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "InterventionAbility", "PlayerPowerAlerts_Table", "Freezing Trap trap_activated", LOG_TAG.Player);
			log2.AddToFillers(triggeredBy, triggeredBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log2.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(triggeredBy, log2, releaseLogAfter: true);
		}
		else if (flag2)
		{
			triggeredBy.reactionComponent.ResistRuinarchPower();
		}
		else
		{
			triggeredBy.reactionComponent.PlayResistVFXandSFX();
		}
	}

	public void SetHasSnareTrap(bool state, bool isPlayerSource, RACE snareTrapExclusion = RACE.NONE)
	{
		isSnareTrapPlayerSource = isPlayerSource;
		if (hasSnareTrap != state)
		{
			base.owner.SetIsDefault(state: false);
			hasSnareTrap = state;
			if (hasSnareTrap)
			{
				base.owner.area.AddSnareTrapInArea();
				this.snareTrapExclusion = snareTrapExclusion;
				_snareTrapEffect = GameManager.Instance.CreateParticleEffectAt(base.owner, PARTICLE_EFFECT.Snare_Trap, 39);
			}
			else
			{
				base.owner.tileObjectComponent.genericTileObject.traitContainer.RemoveStatusAndStacks(base.owner.tileObjectComponent.genericTileObject, "Snare Trapped");
				base.owner.area.RemoveSnareTrapInArea();
				ObjectPoolManager.Instance.DestroyObject(_snareTrapEffect);
				_snareTrapEffect = null;
				this.snareTrapExclusion = RACE.NONE;
			}
		}
	}

	public bool IsImmuneToSnareTrap(Character p_character, SnareTrapped p_status)
	{
		if (!(p_character.characterClass.className == "Archer") && p_character.race != snareTrapExclusion)
		{
			return p_status.awareCharacters.Contains(p_character);
		}
		return true;
	}

	public void TriggerSnareTrap(Character triggeredBy)
	{
		bool flag = false;
		int p_value = 100;
		SkillData spellData = PlayerSkillManager.Instance.GetSpellData(PLAYER_SKILL_TYPE.SNARE_TRAP);
		RESISTANCE resistanceType = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(PLAYER_SKILL_TYPE.SNARE_TRAP).resistanceType;
		float pierceBasedOnCurrentLevel = PlayerSkillManager.Instance.GetPierceBasedOnCurrentLevel(spellData);
		float resistanceValue = triggeredBy.piercingAndResistancesComponent.GetResistanceValue(resistanceType);
		CombatManager.ModifyValueByPiercingAndResistance(ref p_value, pierceBasedOnCurrentLevel, resistanceValue);
		string log = string.Empty;
		if (GameUtilities.RollChance(p_value, ref log))
		{
			flag = true;
		}
		bool flag2 = false;
		if (flag)
		{
			GameManager.Instance.CreateParticleEffectAt(triggeredBy, PARTICLE_EFFECT.Snare_Trap_Explosion);
			flag2 = isSnareTrapPlayerSource;
		}
		SetHasSnareTrap(state: false, isPlayerSource: false);
		if (flag)
		{
			int durationBonusPerLevel = PlayerSkillManager.Instance.GetDurationBonusPerLevel(spellData);
			triggeredBy.traitContainer.AddTrait(triggeredBy, "Ensnared", null, bypassElementalChance: false, durationBonusPerLevel);
			triggeredBy.traitContainer.GetTraitOrStatus<Ensnared>("Ensnared")?.SetIsPlayerSource(flag2);
			if (triggeredBy.isNormalAndNotAlliedWithPlayer)
			{
				Messenger.Broadcast(PlayerSkillSignals.ON_TRAP_ACTIVATED_ON_VILLAGER, triggeredBy, PLAYER_SKILL_TYPE.SNARE_TRAP, flag2);
			}
			Log log2 = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "InterventionAbility", "PlayerPowerAlerts_Table", "Snare Trap trap_activated", LOG_TAG.Player);
			log2.AddToFillers(triggeredBy, triggeredBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log2.AddLogToDatabase();
			PlayerManager.Instance.player.ShowNotificationFrom(triggeredBy, log2, releaseLogAfter: true);
		}
		else if (flag2)
		{
			triggeredBy.reactionComponent.ResistRuinarchPower();
		}
		else
		{
			triggeredBy.reactionComponent.PlayResistVFXandSFX();
		}
	}

	public void EnableStampede(Vector3 p_direction, float p_angle, int p_width)
	{
		Stampede stampede = new Stampede();
		stampede.targetDirection = p_direction;
		stampede.SetGridTileLocation(base.owner);
		stampede.OnPlacePOI();
		stampede.SetIsPlayerSource(p_state: true);
		stampede.SetAngle(p_angle);
		stampede.SetWidth(p_width);
		stampede.StartMovement();
	}

	public void AddAffectedByAOETileObject(AOESpellTileObject p_aoeSpellTileObject)
	{
		TILE_OBJECT_TYPE tileObjectType = p_aoeSpellTileObject.tileObjectType;
		if (affectedByAOETileObjects == null)
		{
			affectedByAOETileObjects = new Dictionary<TILE_OBJECT_TYPE, int>();
		}
		if (!affectedByAOETileObjects.ContainsKey(tileObjectType))
		{
			affectedByAOETileObjects.Add(tileObjectType, 0);
		}
		affectedByAOETileObjects[tileObjectType]++;
	}

	public void RemoveAffectedByAOETileObject(AOESpellTileObject p_aoeSpellTileObject)
	{
		if (affectedByAOETileObjects == null)
		{
			return;
		}
		TILE_OBJECT_TYPE tileObjectType = p_aoeSpellTileObject.tileObjectType;
		if (!affectedByAOETileObjects.ContainsKey(tileObjectType))
		{
			return;
		}
		affectedByAOETileObjects[tileObjectType]--;
		if (affectedByAOETileObjects[tileObjectType] <= 0)
		{
			affectedByAOETileObjects.Remove(tileObjectType);
			if (affectedByAOETileObjects.Count == 0)
			{
				affectedByAOETileObjects = null;
			}
		}
	}

	public bool IsAffectedByAOESpell(TILE_OBJECT_TYPE p_tileObjectType)
	{
		if (affectedByAOETileObjects == null)
		{
			return false;
		}
		if (affectedByAOETileObjects.ContainsKey(p_tileObjectType))
		{
			return affectedByAOETileObjects[p_tileObjectType] > 0;
		}
		return false;
	}

	public void LoadSecondWave()
	{
		if (hasLandmine)
		{
			hasLandmine = false;
			SetHasLandmine(state: true);
		}
		if (hasFreezingTrap)
		{
			hasFreezingTrap = false;
			SetHasFreezingTrap(state: true, isFreezingTrapPlayerSource, freezingTrapExclusions);
		}
		if (hasSnareTrap)
		{
			hasSnareTrap = false;
			SetHasSnareTrap(state: true, isSnareTrapPlayerSource, snareTrapExclusion);
		}
	}

	public void SetIsSeenByEyeWard(bool state)
	{
		isSeenByEyeWard = state;
	}

	public void CleanUp()
	{
		objHere = null;
		genericTileObject = null;
		walls?.Clear();
		walls = null;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
