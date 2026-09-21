using System;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class DemonicStructure : LocationStructure
{
	private InnerMapLight m_innerMapLight;

	private bool _hasCheckedLight;

	public LocationStructureObject structureObj { get; private set; }

	public List<Character> currentAttackers { get; private set; }

	public Character preOccupiedBy { get; private set; }

	public string templateName { get; private set; }

	public Vector3 structureObjectWorldPos { get; private set; }

	public InnerMapLight structureObjectLight
	{
		get
		{
			if (!_hasCheckedLight && m_innerMapLight == null)
			{
				_hasCheckedLight = true;
				m_innerMapLight = structureObj.GetComponentInChildren<InnerMapLight>(includeInactive: true);
			}
			return m_innerMapLight;
		}
	}

	public List<string> connectedMonsterIDs { get; private set; }

	public override Vector2 selectableSize => structureObj.size;

	public override Type serializedData => typeof(SaveDataDemonicStructure);

	public virtual SUMMON_TYPE housedMonsterType => SUMMON_TYPE.None;

	protected virtual int maximumConnectedMonsters => 0;

	protected DemonicStructure(STRUCTURE_TYPE structureType, Region location)
		: base(structureType, location)
	{
		SetMaxHPAndReset(3000);
		currentAttackers = new List<Character>();
	}

	public DemonicStructure(Region location, SaveDataDemonicStructure data)
		: base(location, data)
	{
		currentAttackers = new List<Character>();
	}

	protected override void AfterStructureHPAdjusted()
	{
		base.AfterStructureHPAdjusted();
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
	}

	protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false, bool shouldBeCleanedUp = true)
	{
		if (!base.hasBeenDestroyed)
		{
			InnerMapManager.Instance.RemoveWorldKnownDemonicStructure(this);
			BaseParticleEffect component = GameManager.Instance.CreateParticleEffectAt(GetCenterTile(), PARTICLE_EFFECT.Destroy_Structure).GetComponent<BaseParticleEffect>();
			if (structureObj != null)
			{
				component.SetSize(structureObj.size);
			}
			base.DestroyStructure(p_responsibleCharacter, isPlayerSource, shouldBeCleanedUp: false);
			if (shouldBeCleanedUp)
			{
				MarkForCleanup();
			}
		}
	}

	protected override void AfterStructureDestruction(Character p_responsibleCharacter = null)
	{
		structureObj.OnOwnerStructureDestroyed(base.region.innerMap, this);
		Area area = base.occupiedArea;
		base.AfterStructureDestruction(p_responsibleCharacter);
		area?.TryRemoveAreaFromSettlementIfItIsNoLongerPartOfIt();
		for (int i = 0; i < currentAttackers.Count; i++)
		{
			Character p_character = currentAttackers[i];
			if (PlayerManager.Instance.player.retaliationComponent.HasRetaliator(p_character))
			{
				PlayerManager.Instance.player.retaliationComponent.AddDestroyedStructureByRetaliators();
				break;
			}
		}
		currentAttackers.Clear();
		if (base.structureType != STRUCTURE_TYPE.THE_PORTAL)
		{
			DemonicStructurePlayerSkill demonicStructureSkillData = PlayerSkillManager.Instance.GetDemonicStructureSkillData(base.structureType);
			if (demonicStructureSkillData.isInUse && PlayerManager.Instance.player.playerSettlement.GetNumberOfStructures(base.structureType) < demonicStructureSkillData.maxCharges)
			{
				demonicStructureSkillData.AdjustCharges(1);
			}
		}
		if (connectedMonsterIDs != null)
		{
			for (int j = 0; j < connectedMonsterIDs.Count; j++)
			{
				string text = connectedMonsterIDs[j];
				Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(text);
				if (!characterByPersistentID.partyComponent.hasParty || characterByPersistentID.partyComponent.currentParty == PlayerManager.Instance.player.underlingsComponent.persistentDefendParty)
				{
					characterByPersistentID.Death();
				}
			}
		}
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)this);
	}

	public override LocationGridTile GetCenterTile()
	{
		if (structureObj != null)
		{
			return GridMap.Instance.mainRegion.innerMap.GetTileFromWorldPosition(structureObj.worldPosition);
		}
		return CollectionUtilities.GetRandomElement(base.tiles);
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		if (structureObjectLight != null)
		{
			structureObjectLight.gameObject.SetActive(value: true);
		}
	}

	public override void OnDoneLoadStructure()
	{
		base.OnDoneLoadStructure();
		if (structureObjectLight != null)
		{
			structureObjectLight.gameObject.SetActive(value: true);
		}
	}

	public override void LoadReferences(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadReferences(saveDataLocationStructure);
		SaveDataDemonicStructure saveDataDemonicStructure = saveDataLocationStructure as SaveDataDemonicStructure;
		if (!string.IsNullOrEmpty(saveDataDemonicStructure.preOccupiedBy))
		{
			preOccupiedBy = CharacterManager.Instance.GetCharacterByPersistentID(saveDataDemonicStructure.preOccupiedBy);
		}
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		base.SubscribeListeners(shouldLock);
		Messenger.AddListener<TileObject, int, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED, base.OnObjectDamaged, shouldLock);
		Messenger.AddListener<TileObject, int, Character, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED_BY, base.OnObjectDamagedBy, shouldLock);
		Messenger.AddListener<TileObject, int>(TileObjectSignals.TILE_OBJECT_REPAIRED, base.OnObjectRepaired, shouldLock);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied, shouldLock);
	}

	protected override void UnsubscribeListeners()
	{
		base.UnsubscribeListeners();
		Messenger.RemoveListener<TileObject, int, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED, base.OnObjectDamaged);
		Messenger.RemoveListener<TileObject, int, Character, bool>(TileObjectSignals.TILE_OBJECT_DAMAGED_BY, base.OnObjectDamagedBy);
		Messenger.RemoveListener<TileObject, int>(TileObjectSignals.TILE_OBJECT_REPAIRED, base.OnObjectRepaired);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
	}

	private bool DoesSnatchJobTargetThisStructure(JobQueueItem job)
	{
		if (job is GoapPlanJob goapPlanJob)
		{
			OtherData[] otherDataSpecific = goapPlanJob.GetOtherDataSpecific(INTERACTION_TYPE.DROP);
			if (otherDataSpecific != null)
			{
				for (int i = 0; i < otherDataSpecific.Length; i++)
				{
					if (otherDataSpecific[i] is LocationStructureOtherData locationStructureOtherData && locationStructureOtherData.locationStructure == this)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void OnCharacterDied(Character p_character)
	{
		RemoveConnectedMonster(p_character);
	}

	public override void CenterOnStructure()
	{
		if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != base.region.innerMap)
		{
			InnerMapManager.Instance.HideAreaMap();
		}
		if (!base.region.innerMap.isShowing)
		{
			InnerMapManager.Instance.ShowInnerMap(base.region);
		}
		if (structureObj != null)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(structureObj.gameObject);
		}
	}

	public override void ShowSelectorOnStructure()
	{
		Selector.Instance.Select(this);
	}

	public void RepairStructure()
	{
		ResetHP();
		for (int i = 0; i < base.objectsThatContributeToDamage.Count; i++)
		{
			IDamageable damageable = base.objectsThatContributeToDamage.ElementAt(i);
			damageable.AdjustHP(damageable.maxHP, ELEMENTAL_TYPE.Normal);
			if (damageable is ITraitable traitable)
			{
				traitable.traitContainer.RemoveTrait(traitable, "Burnt");
			}
		}
		Messenger.Broadcast(StructureSignals.DEMONIC_STRUCTURE_REPAIRED, this);
	}

	public bool HasUnoccupiedRoom()
	{
		if (base.rooms == null)
		{
			return false;
		}
		for (int i = 0; i < base.rooms.Length; i++)
		{
			if (!base.rooms[i].HasAnyAliveCharacterInRoom())
			{
				return true;
			}
		}
		return false;
	}

	public int GetUnoccupiedRoomCount()
	{
		if (base.rooms == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < base.rooms.Length; i++)
		{
			if (!base.rooms[i].HasAnyAliveCharacterInRoom())
			{
				num++;
			}
		}
		return num;
	}

	public void SetPreOccupiedBy(Character p_character)
	{
		preOccupiedBy = p_character;
	}

	public override void OnTileRepaired(LocationGridTile tile, int amount)
	{
		if (!base.hasBeenDestroyed && tile.tileObjectComponent.genericTileObject.currentHP >= tile.tileObjectComponent.genericTileObject.maxHP)
		{
			structureObj?.ApplyGroundTileAssetForTile(tile);
			tile.CreateSeamlessEdgesForSelfAndNeighbours();
		}
	}

	public virtual void SetStructureObject(LocationStructureObject structureObj)
	{
		this.structureObj = structureObj;
		templateName = structureObj.name;
		structureObjectWorldPos = structureObj.transform.position;
		Vector3 position = structureObj.transform.position;
		position.x -= 0.5f;
		position.y -= 0.5f;
		worldPosition = position;
	}

	public void AddAttacker(Character p_attacker)
	{
		if (!currentAttackers.Contains(p_attacker))
		{
			currentAttackers.Add(p_attacker);
			Messenger.Broadcast(CharacterSignals.CHARACTER_HIT_DEMONIC_STRUCTURE, p_attacker, this);
		}
	}

	public void RemoveAttacker(Character p_attacker)
	{
		currentAttackers.Remove(p_attacker);
	}

	public override bool IsAtTargetDestination(Character character)
	{
		if (!base.IsAtTargetDestination(character))
		{
			return CanSeeObjectLocatedHere(character);
		}
		return true;
	}

	public bool CanSeeObjectLocatedHere(Character p_character)
	{
		if (p_character.hasMarker)
		{
			for (int i = 0; i < p_character.marker.inVisionTileObjects.Count; i++)
			{
				TileObject tileObject = p_character.marker.inVisionTileObjects[i];
				if (tileObject.structureLocation != null && tileObject.structureLocation == this)
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.persistentID))
		{
			structureObj = null;
			currentAttackers.Clear();
			SetPreOccupiedBy(null);
			base.CleanUp();
		}
	}

	public void AddConnectedMonster(Character p_character)
	{
		if (connectedMonsterIDs == null)
		{
			connectedMonsterIDs = RuinarchListPool<string>.Claim(10);
		}
		if (!connectedMonsterIDs.Contains(p_character.persistentID))
		{
			connectedMonsterIDs.Add(p_character.persistentID);
		}
	}

	private void RemoveConnectedMonster(Character p_character)
	{
		if (connectedMonsterIDs != null && connectedMonsterIDs.Remove(p_character.persistentID) && connectedMonsterIDs.Count <= 0)
		{
			connectedMonsterIDs = null;
		}
	}

	public bool HasMaximumConnectedMonsters()
	{
		if (connectedMonsterIDs != null)
		{
			return connectedMonsterIDs.Count >= maximumConnectedMonsters;
		}
		return false;
	}
}
