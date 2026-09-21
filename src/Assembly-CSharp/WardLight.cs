using System;
using System.Collections.Generic;
using Characters.Components;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class WardLight : TileObject, CharacterEventDispatcher.ILocationListener
{
	private WardLightGameObject _wardLightGameObject;

	private InnerMapLight _innerMapLight;

	public NPCSettlement settlementOwner { get; private set; }

	public List<Character> charactersInRange { get; private set; }

	public override Type serializedData => typeof(SaveDataWardLight);

	public WardLight()
	{
		Initialize(TILE_OBJECT_TYPE.WARD_LIGHT);
		base.traitContainer.AddTrait(this, "Immovable");
		charactersInRange = new List<Character>();
	}

	public WardLight(SaveDataTileObject data)
		: base(data)
	{
		charactersInRange = new List<Character>();
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		SaveDataWardLight saveDataWardLight = data as SaveDataWardLight;
		if (!string.IsNullOrEmpty(saveDataWardLight.settlementOwner))
		{
			settlementOwner = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentIDSafe(saveDataWardLight.settlementOwner) as NPCSettlement;
		}
		if (saveDataWardLight.charactersInRange != null)
		{
			SaveUtilities.ConvertIDArrayToCharacters(saveDataWardLight.charactersInRange, charactersInRange);
		}
	}

	protected override void SubscribeListeners(bool shouldLock)
	{
		if (!base.hasSubscribedToSignals)
		{
			base.hasSubscribedToSignals = true;
			base.SubscribeListeners(shouldLock);
			Messenger.AddListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
		}
	}

	protected override void UnsubscribeListeners()
	{
		if (base.hasSubscribedToSignals)
		{
			base.hasSubscribedToSignals = false;
			base.UnsubscribeListeners();
			Messenger.RemoveListener<NPCSettlement>(SettlementSignals.DISCONNECT_FROM_SETTLEMENT, DisconnectFromSettlement);
		}
	}

	protected override void CreateMapObjectVisual()
	{
		base.CreateMapObjectVisual();
		_wardLightGameObject = mapVisual.gameObject.GetComponent<WardLightGameObject>();
		_wardLightGameObject.SetInRangeActions(AddCharacterInRange, RemoveCharacterInRange);
		_innerMapLight = mapVisual.gameObject.GetComponentInChildren<InnerMapLight>();
	}

	public override void DestroyMapVisualGameObject()
	{
		base.DestroyMapVisualGameObject();
		_wardLightGameObject = null;
		_innerMapLight = null;
	}

	public override void ProcessOnSetAsActiveInTileObjectInfo()
	{
		base.ProcessOnSetAsActiveInTileObjectInfo();
		if (_wardLightGameObject != null)
		{
			_wardLightGameObject.SetRangeHighlightState(p_state: true);
		}
	}

	public override void ProcessOnSetAsInactiveInTileObjectInfo()
	{
		base.ProcessOnSetAsInactiveInTileObjectInfo();
		if (_wardLightGameObject != null)
		{
			_wardLightGameObject.SetRangeHighlightState(p_state: false);
		}
	}

	protected override void OnSetObjectAsUnbuilt()
	{
		base.OnSetObjectAsUnbuilt();
		if (_innerMapLight != null)
		{
			_innerMapLight.gameObject.SetActive(value: false);
			_wardLightGameObject.SetRangeColliderState(p_state: false);
		}
	}

	protected override void OnSetObjectAsBuilding()
	{
		base.OnSetObjectAsBuilding();
		if (_innerMapLight != null)
		{
			_innerMapLight.gameObject.SetActive(value: false);
		}
	}

	protected override void OnSetObjectAsBuilt()
	{
		base.OnSetObjectAsBuilt();
		if (_innerMapLight != null)
		{
			_innerMapLight.gameObject.SetActive(value: true);
			_wardLightGameObject.SetRangeColliderState(p_state: true);
		}
	}

	protected override void OnSetGridTileLocation()
	{
		base.OnSetGridTileLocation();
		BaseSettlement settlement;
		if (gridTileLocation == null)
		{
			SetSettlementOwner(null);
		}
		else if (gridTileLocation.IsPartOfSettlement(out settlement) && settlement is NPCSettlement nPCSettlement)
		{
			SetSettlementOwner(nPCSettlement);
		}
	}

	public override string GetAdditionalTestingData()
	{
		return string.Concat(base.GetAdditionalTestingData() + "\nSettlement Owner: " + settlementOwner?.name, "\nCharacters In Range: ", charactersInRange?.ComafyList());
	}

	public override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom, bool removeTraits = true, bool destroyTileSlots = true)
	{
		base.OnRemoveTileObject(removedBy, removedFrom, removeTraits, destroyTileSlots);
		_wardLightGameObject?.SetRangeColliderState(p_state: false);
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		_wardLightGameObject?.SetRangeColliderState(p_state: true);
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		charactersInRange?.Remove(p_character);
	}

	public override void DestroyPermanently()
	{
		base.DestroyPermanently();
		settlementOwner = null;
		charactersInRange?.Clear();
		charactersInRange = null;
	}

	public void SetSettlementOwner(NPCSettlement p_settlement)
	{
		if (settlementOwner != p_settlement)
		{
			settlementOwner?.tileObjectComponent.RemoveWardLight(this);
			settlementOwner = p_settlement;
			settlementOwner?.tileObjectComponent.AddWardLight(this);
		}
	}

	private void AddCharacterInRange(Character target)
	{
		if (!charactersInRange.Contains(target))
		{
			charactersInRange.Add(target);
			Character p_character = target;
			if (target.reactionComponent.disguisedCharacter != null)
			{
				p_character = target.reactionComponent.disguisedCharacter;
			}
			if (settlementOwner != null && settlementOwner.ShouldBeUnderSiegeIfCharacterEntersSettlement(p_character) && !TryTriggerUnderSiege(target))
			{
				target.eventDispatcher.SubscribeToCharacterArrivedAtSettlement(this);
			}
		}
	}

	private void RemoveCharacterInRange(Character p_character)
	{
		if (charactersInRange.Remove(p_character))
		{
			p_character.eventDispatcher.UnsubscribeToCharacterArrivedAtSettlement(this);
		}
	}

	private bool TryTriggerUnderSiege(Character target)
	{
		if (settlementOwner?.owner != null && target.gridTileLocation != null && target.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(settlementOwner) && (target.gridTileLocation.structure.structureType == STRUCTURE_TYPE.WILDERNESS || target.gridTileLocation.structure.settlementLocation == settlementOwner) && (settlementOwner.owner.isMajorFaction || settlementOwner.owner.factionType.type == FACTION_TYPE.Bandits))
		{
			if (settlementOwner.SetIsUnderSiege(state: true))
			{
				PlayerManager.Instance.player.retaliationComponent.UnderSiegeRetaliation(settlementOwner, target);
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Tile Objects", "TileObjectAlerts_Table", "Ward_Light_Hostile", LOG_TAG.Major, LOG_TAG.Combat);
				log.AddToFillers(settlementOwner, settlementOwner.name, LOG_IDENTIFIER.LANDMARK_1);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			}
			settlementOwner.AlertSleepingCombatantResidents();
			return true;
		}
		return false;
	}

	private void DisconnectFromSettlement(NPCSettlement p_settlement)
	{
		if (settlementOwner == p_settlement)
		{
			SetSettlementOwner(null);
		}
	}

	public void OnCharacterLeftStructure(Character p_character, LocationStructure p_leftStructure)
	{
	}

	public void OnCharacterArrivedAtStructure(Character p_character, LocationStructure p_leftStructure)
	{
	}

	public void OnCharacterArrivedAtSettlement(Character p_character, NPCSettlement p_settlement)
	{
		if (charactersInRange.Contains(p_character))
		{
			Character p_character2 = p_character;
			if (p_character.reactionComponent.disguisedCharacter != null)
			{
				p_character2 = p_character.reactionComponent.disguisedCharacter;
			}
			if (settlementOwner.ShouldBeUnderSiegeIfCharacterEntersSettlement(p_character2))
			{
				TryTriggerUnderSiege(p_character);
			}
		}
	}
}
