using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;

public abstract class BaseBed : TileObject
{
	private Character[] bedUsers;

	public override Character[] users => bedUsers;

	public BaseBed(int slots)
	{
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		bedUsers = new Character[slots];
	}

	public BaseBed(SaveDataTileObject data, int slots)
		: base(data)
	{
		AddAdvertisedAction(INTERACTION_TYPE.DROP_ITEM);
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
		bedUsers = new Character[slots];
	}

	public override void SetPOIState(POI_STATE state)
	{
		base.SetPOIState(state);
		if (IsSlotAvailable())
		{
			if (GetActiveUserCount() > 0)
			{
				UpdateUsedBedAsset();
			}
			else if (gridTileLocation != null && mapVisual != null)
			{
				mapVisual.UpdateTileObjectVisual(this);
			}
		}
	}

	public override void OnTileObjectGainedTrait(Trait trait)
	{
		base.OnTileObjectGainedTrait(trait);
		if (trait.name == "Burning")
		{
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_JOBS_TARGETING_POI, (IPointOfInterest)this, "Bed_Burning");
			Messenger.Broadcast(CharacterSignals.FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI, (IPointOfInterest)this, "Bed_Burning");
		}
	}

	public bool IsSlotAvailable()
	{
		for (int i = 0; i < bedUsers.Length; i++)
		{
			if (bedUsers[i] == null)
			{
				return true;
			}
		}
		return false;
	}

	protected override bool AddUser(Character character)
	{
		if (gridTileLocation == null)
		{
			return false;
		}
		for (int i = 0; i < bedUsers.Length; i++)
		{
			if (bedUsers[i] == null)
			{
				bedUsers[i] = character;
				character.SetTileObjectLocation(this);
				UpdateUsedBedAsset();
				Vector3 positionWithinTileThatIsOnAWalkableNode = gridTileLocation.GetPositionWithinTileThatIsOnAWalkableNode();
				character.marker.pathfindingAI.Teleport(positionWithinTileThatIsOnAWalkableNode);
				character.marker.SetVisualState(state: false);
				character.tileObjectComponent.SetBedBeingUsed(this);
				Messenger.Broadcast(TileObjectSignals.ADD_TILE_OBJECT_USER, GetBase(), character);
				if (Selector.Instance.IsSelected(character) && base.mapObjectVisual != null)
				{
					Selector.Instance.Select(this, base.mapObjectVisual.transform);
				}
				return true;
			}
		}
		return false;
	}

	public override bool RemoveUser(Character character)
	{
		for (int i = 0; i < bedUsers.Length; i++)
		{
			if (bedUsers[i] != character)
			{
				continue;
			}
			bedUsers[i] = null;
			character.SetTileObjectLocation(null);
			UpdateUsedBedAsset();
			character.marker.SetVisualState(state: true);
			character.tileObjectComponent.SetBedBeingUsed(null);
			if (character.gridTileLocation != null && character.traitContainer.HasTrait("Paralyzed"))
			{
				LocationGridTile firstNeighborThatIsPassableAndSameStructureAs = character.gridTileLocation.GetFirstNeighborThatIsPassableAndSameStructureAs(character.gridTileLocation.structure);
				if (firstNeighborThatIsPassableAndSameStructureAs != null)
				{
					character.marker.PlaceMarkerAt(firstNeighborThatIsPassableAndSameStructureAs);
				}
				else
				{
					character.marker.PlaceMarkerAt(character.gridTileLocation);
				}
			}
			else
			{
				character.marker.UpdateAnimation();
			}
			Messenger.Broadcast(TileObjectSignals.REMOVE_TILE_OBJECT_USER, GetBase(), character);
			if (UIManager.Instance.characterInfoUI.isShowing && UIManager.Instance.characterInfoUI.activeCharacter == character && character.hasMarker)
			{
				Selector.Instance.Select(character, character.marker.transform);
				character.CenterOnCharacter();
			}
			character.visuals?.UpdateAllVisuals(character);
			return true;
		}
		return false;
	}

	public int GetActiveUserCount()
	{
		int num = 0;
		for (int i = 0; i < bedUsers.Length; i++)
		{
			if (bedUsers[i] != null)
			{
				num++;
			}
		}
		return num;
	}

	protected void LoadUser(Character character)
	{
		for (int i = 0; i < bedUsers.Length; i++)
		{
			if (bedUsers[i] == null)
			{
				bedUsers[i] = character;
				character.SetTileObjectLocation(this);
				UpdateUsedBedAsset();
				character.marker.SetVisualState(state: false);
				break;
			}
		}
	}

	public virtual bool CanUseBed(Character character)
	{
		return IsSlotAvailable();
	}

	public void UpdateUsedBedAsset()
	{
		if (gridTileLocation != null)
		{
			mapVisual.UpdateTileObjectVisual(this);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		bedUsers.Contains(p_character);
	}
}
