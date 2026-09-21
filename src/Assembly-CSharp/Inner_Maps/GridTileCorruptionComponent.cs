using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps;

public class GridTileCorruptionComponent : LocationGridTileComponent
{
	private const int corruptionDurationInTicks = 5;

	private const int buildOrDestroyWallDurationInTicks = 5;

	private GameObject _buildSmokeEffect;

	private string _corruptionScheduleID;

	public bool isCorrupted => base.owner.groundType == LocationGridTile.Ground_Type.Corrupted;

	public bool isCurrentlyBeingCorrupted { get; private set; }

	public bool willGenerateDemonicDecorOnCorruptionFinish { get; private set; }

	public bool wallIsBeingBuilt { get; private set; }

	public bool wallIsBeingDestroyed { get; private set; }

	public bool isAlreadyVisitedForIslandCounting { get; private set; }

	public GameDate corruptDate { get; private set; }

	public GameDate wallBuildOrDestroyDate { get; private set; }

	public GridTileCorruptionComponent()
	{
	}

	public GridTileCorruptionComponent(SaveDataGridTileCorruptionComponent data)
	{
		isCurrentlyBeingCorrupted = data.isCurrentlyBeingCorrupted;
		corruptDate = data.corruptDate;
		wallIsBeingBuilt = data.wallIsBeingBuilt;
		wallIsBeingDestroyed = data.wallIsBeingDestroyed;
		wallBuildOrDestroyDate = data.wallBuildOrDestroyDate;
		willGenerateDemonicDecorOnCorruptionFinish = data.willGenerateDemonicDecorOnCorruptionFinish;
	}

	public void StartCorruption(bool randomGenerateDemonicDecor = false)
	{
		if (!isCurrentlyBeingCorrupted && !isCorrupted)
		{
			if (!_buildSmokeEffect)
			{
				_buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(base.owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
			}
			isCurrentlyBeingCorrupted = true;
			willGenerateDemonicDecorOnCorruptionFinish = randomGenerateDemonicDecor;
			corruptDate = GameManager.Instance.Today().AddTicks(5);
			_corruptionScheduleID = (randomGenerateDemonicDecor ? SchedulingManager.Instance.AddEntry(corruptDate, CorruptTileAndRandomlyGenerateDemonicObject, null) : SchedulingManager.Instance.AddEntry(corruptDate, CorruptTile, null));
			base.owner.mouseEventsComponent.OnHoverExit();
			base.owner.SetIsDefault(state: false);
		}
	}

	public void DisruptCorruption()
	{
		if (isCurrentlyBeingCorrupted && !isCorrupted)
		{
			if ((bool)_buildSmokeEffect)
			{
				ObjectPoolManager.Instance.DestroyObject(_buildSmokeEffect);
				_buildSmokeEffect = null;
			}
			isCurrentlyBeingCorrupted = false;
			SchedulingManager.Instance.RemoveSpecificEntry(_corruptionScheduleID);
			base.owner.mouseEventsComponent.OnHoverExit();
		}
	}

	public void CorruptTile()
	{
		isCurrentlyBeingCorrupted = false;
		if ((bool)_buildSmokeEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(_buildSmokeEffect);
			_buildSmokeEffect = null;
		}
		if (isCorrupted)
		{
			return;
		}
		PlayerManager.Instance.player.playerSettlement.AddCorruptedTile(base.owner);
		base.owner.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
		base.owner.CreateSeamlessEdgesForSelfAndNeighbours();
		base.owner.UpdateMinimapVisual(base.owner.structure);
		TileObject objHere = base.owner.tileObjectComponent.objHere;
		if (objHere != null)
		{
			if (objHere is TreeObject { mapObjectVisual: TileObjectGameObject mapObjectVisual } treeObject)
			{
				if (treeObject is BigTreeObject)
				{
					base.owner.structure.RemovePOI(objHere);
				}
				else
				{
					mapObjectVisual.UpdateTileObjectVisual(treeObject);
				}
			}
			else if (objHere is BlockWall blockWall)
			{
				blockWall.SetWallType(WALL_TYPE.Demon_Stone);
				blockWall.UpdateVisual(base.owner);
			}
			else
			{
				if (objHere is Tombstone tombstone)
				{
					tombstone.SetRespawnCorpseOnDestroy(state: false);
				}
				if (!objHere.tileObjectType.IsTileObjectImportant() && !objHere.traitContainer.HasTrait("Indestructible"))
				{
					base.owner.structure.RemovePOI(objHere);
				}
			}
		}
		base.owner.mouseEventsComponent.OnHoverExit();
	}

	private void CorruptTileAndRandomlyGenerateDemonicObject()
	{
		isCurrentlyBeingCorrupted = false;
		if ((bool)_buildSmokeEffect)
		{
			ObjectPoolManager.Instance.DestroyObject(_buildSmokeEffect);
			_buildSmokeEffect = null;
		}
		if (isCorrupted)
		{
			return;
		}
		PlayerManager.Instance.player.playerSettlement.AddCorruptedTile(base.owner);
		base.owner.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
		base.owner.CreateSeamlessEdgesForSelfAndNeighbours();
		base.owner.UpdateMinimapVisual(base.owner.structure);
		TileObject objHere = base.owner.tileObjectComponent.objHere;
		if (objHere != null)
		{
			if (objHere is TreeObject { mapObjectVisual: TileObjectGameObject mapObjectVisual } treeObject)
			{
				if (treeObject is BigTreeObject)
				{
					base.owner.structure.RemovePOI(objHere);
				}
				else
				{
					mapObjectVisual.UpdateTileObjectVisual(treeObject);
				}
			}
			else if (objHere is BlockWall blockWall)
			{
				blockWall.SetWallType(WALL_TYPE.Demon_Stone);
				blockWall.UpdateVisual(base.owner);
			}
			else
			{
				if (objHere is Tombstone tombstone)
				{
					tombstone.SetRespawnCorpseOnDestroy(state: false);
				}
				if (!objHere.tileObjectType.IsTileObjectImportant() && !objHere.traitContainer.HasTrait("Indestructible"))
				{
					base.owner.structure.RemovePOI(objHere);
				}
			}
		}
		if (base.owner.tileObjectComponent.objHere == null && ChanceData.RollChance(CHANCE_TYPE.Demonic_Decor_On_Corrupt))
		{
			TILE_OBJECT_TYPE randomElement = CollectionUtilities.GetRandomElement(GameUtilities.corruptionTileObjectChoices);
			TileObject poi = InnerMapManager.Instance.CreateNewTileObject<TileObject>(randomElement);
			base.owner.structure.AddPOI(poi, base.owner);
		}
		base.owner.mouseEventsComponent.OnHoverExit();
	}

	public void CorruptTileAndDestroyDestructibleObject()
	{
		if (!isCorrupted)
		{
			PlayerManager.Instance.player.playerSettlement.AddCorruptedTile(base.owner);
			base.owner.SetGroundTilemapVisual(InnerMapManager.Instance.assetManager.corruptedTile);
			base.owner.CreateSeamlessEdgesForSelfAndNeighbours();
			base.owner.UpdateMinimapVisual(base.owner.structure);
			TileObject objHere = base.owner.tileObjectComponent.objHere;
			if (objHere != null && !objHere.traitContainer.HasTrait("Indestructible"))
			{
				base.owner.structure.RemovePOI(objHere);
			}
		}
	}

	public void UncorruptTile()
	{
		if (isCorrupted)
		{
			base.owner.RevertTileToOriginalPerlin();
			base.owner.CreateSeamlessEdgesForSelfAndNeighbours();
		}
		else
		{
			DisruptCorruption();
		}
	}

	public bool HasCorruptedNeighbour()
	{
		for (int i = 0; i < base.owner.neighbourList.Count; i++)
		{
			if (base.owner.neighbourList[i].corruptionComponent.isCorrupted)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanCorruptTile()
	{
		if (HasCorruptedNeighbour() && !base.owner.hasBlueprint && base.owner.structure.structureType == STRUCTURE_TYPE.WILDERNESS && !isCorrupted && !isCurrentlyBeingCorrupted)
		{
			return PlayerManager.Instance.player.currentActivePlayerSpell == null;
		}
		return false;
	}

	public bool CanDisruptCorruptionOfTile()
	{
		if (!isCorrupted && isCurrentlyBeingCorrupted)
		{
			return PlayerManager.Instance.player.currentActivePlayerSpell == null;
		}
		return false;
	}

	public void SetIsAlreadyVisitedForIslandCounting(bool p_state)
	{
		isAlreadyVisitedForIslandCounting = p_state;
	}

	public void BaseBuildingTileDemolition(bool p_includeStructures, bool p_includeDemonicWalls, bool p_includeDecorations, bool p_includeCorruptedTiles, bool canDemolishTiles = true)
	{
		SkillData buildSkillData = PlayerSkillManager.Instance.GetBuildSkillData(PLAYER_SKILL_TYPE.CORRUPT_TILE);
		if (p_includeStructures && base.owner.structure.structureType != STRUCTURE_TYPE.THE_PORTAL && base.owner.structure is DemonicStructure demonicStructure)
		{
			demonicStructure.AdjustHP(-demonicStructure.currentHP, null, isPlayerSource: true);
		}
		TileObject objHere = base.owner.tileObjectComponent.objHere;
		if (objHere != null && objHere.isBuiltByPlayerBaseBuilding)
		{
			if (objHere.tileObjectType == TILE_OBJECT_TYPE.BLOCK_WALL)
			{
				if (p_includeDemonicWalls)
				{
					base.owner.structure.RemovePOI(objHere);
				}
			}
			else if (p_includeDecorations)
			{
				base.owner.structure.RemovePOI(objHere);
			}
		}
		if (p_includeCorruptedTiles && canDemolishTiles && base.owner.corruptionComponent.isCorrupted && !base.owner.HasNeighbourStructure(STRUCTURE_TYPE.THE_PORTAL) && base.owner.structure.structureType != STRUCTURE_TYPE.THE_PORTAL)
		{
			base.owner.corruptionComponent.UncorruptTile();
			if (!PlayerSkillManager.Instance.unlimitedCast && !WorldSettings.Instance.worldSettingsData.playerSkillSettings.PowerHasUnlimitedCharges(buildSkillData.type))
			{
				buildSkillData.AdjustCharges(1);
			}
		}
	}

	public void StartBuildDemonicWall()
	{
		if (CanBuildDemonicWall())
		{
			if (!_buildSmokeEffect)
			{
				_buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(base.owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
			}
			wallIsBeingBuilt = true;
			wallBuildOrDestroyDate = GameManager.Instance.Today().AddTicks(5);
			SchedulingManager.Instance.AddEntry(wallBuildOrDestroyDate, BuildDemonicWall, null);
			base.owner.mouseEventsComponent.OnHoverExit();
		}
	}

	public void BuildDemonicWall()
	{
		wallIsBeingBuilt = false;
		if (CanBuildDemonicWall())
		{
			if ((bool)_buildSmokeEffect)
			{
				ObjectPoolManager.Instance.DestroyObject(_buildSmokeEffect);
				_buildSmokeEffect = null;
			}
			BuildDemonicWallBase();
			base.owner.mouseEventsComponent.OnHoverExit();
		}
	}

	public BlockWall BuildDemonicWallBase()
	{
		if (base.owner.tileObjectComponent.objHere != null)
		{
			base.owner.structure.RemovePOI(base.owner.tileObjectComponent.objHere);
		}
		BlockWall blockWall = InnerMapManager.Instance.CreateNewTileObject<BlockWall>(TILE_OBJECT_TYPE.BLOCK_WALL);
		blockWall.SetWallType(WALL_TYPE.Demon_Stone);
		base.owner.structure.AddPOI(blockWall, base.owner);
		return blockWall;
	}

	public void StartDestroyDemonicWall()
	{
		if (CanDestroyDemonicWall())
		{
			if (!_buildSmokeEffect)
			{
				_buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(base.owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
			}
			wallIsBeingDestroyed = true;
			wallBuildOrDestroyDate = GameManager.Instance.Today().AddTicks(5);
			SchedulingManager.Instance.AddEntry(wallBuildOrDestroyDate, DestroyDemonicWall, null);
			base.owner.mouseEventsComponent.OnHoverExit();
		}
	}

	public void DestroyDemonicWall()
	{
		wallIsBeingDestroyed = false;
		if (CanDestroyDemonicWall())
		{
			if ((bool)_buildSmokeEffect)
			{
				ObjectPoolManager.Instance.DestroyObject(_buildSmokeEffect);
				_buildSmokeEffect = null;
			}
			base.owner.tileObjectComponent.objHere.AdjustHP(-base.owner.tileObjectComponent.objHere.currentHP, ELEMENTAL_TYPE.Normal, triggerDeath: true);
			base.owner.mouseEventsComponent.OnHoverExit();
		}
	}

	public bool CanBuildDemonicWall()
	{
		if (isCorrupted && !wallIsBeingBuilt && !wallIsBeingDestroyed && base.owner.structure is Wilderness && (base.owner.tileObjectComponent.objHere == null || (!base.owner.tileObjectComponent.objHere.IsUnpassable() && !base.owner.tileObjectComponent.objHere.traitContainer.HasTrait("Indestructible"))))
		{
			return PlayerManager.Instance.player.currentActivePlayerSpell == null;
		}
		return false;
	}

	public bool CanDestroyDemonicWall()
	{
		if (isCorrupted && !wallIsBeingDestroyed && !wallIsBeingBuilt && base.owner.tileObjectComponent.objHere is BlockWall blockWall)
		{
			return blockWall.wallType == WALL_TYPE.Demon_Stone;
		}
		return false;
	}

	public void LoadSecondWave()
	{
		if (isCurrentlyBeingCorrupted)
		{
			if (!_buildSmokeEffect)
			{
				_buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(base.owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
			}
			_corruptionScheduleID = (willGenerateDemonicDecorOnCorruptionFinish ? SchedulingManager.Instance.AddEntry(corruptDate, CorruptTileAndRandomlyGenerateDemonicObject, null) : SchedulingManager.Instance.AddEntry(corruptDate, CorruptTile, null));
		}
		else if (wallIsBeingBuilt)
		{
			if (!_buildSmokeEffect)
			{
				_buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(base.owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
			}
			SchedulingManager.Instance.AddEntry(wallBuildOrDestroyDate, BuildDemonicWall, null);
		}
		else if (wallIsBeingDestroyed)
		{
			if (!_buildSmokeEffect)
			{
				_buildSmokeEffect = GameManager.Instance.CreateParticleEffectAt(base.owner, PARTICLE_EFFECT.Build_Grid_Tile_Smoke);
			}
			SchedulingManager.Instance.AddEntry(wallBuildOrDestroyDate, DestroyDemonicWall, null);
		}
		if (isCorrupted)
		{
			PlayerManager.Instance.player.playerSettlement.AddCorruptedTile(base.owner);
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
