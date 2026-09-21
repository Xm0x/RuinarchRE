using System.Collections.Generic;
using System.Linq;
using Ruinarch;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public abstract class StructureRoom : IPlayerActionTarget, ISelectable
{
	public string name { get; }

	public List<LocationGridTile> tilesInRoom { get; }

	public List<PLAYER_SKILL_TYPE> actions { get; }

	public Vector3 worldPosition { get; protected set; }

	public Vector2 selectableSize { get; protected set; }

	public LocationStructure parentStructure => tilesInRoom?[0].structure;

	protected StructureRoom(string name, List<LocationGridTile> tilesInRoom)
	{
		this.name = name;
		this.tilesInRoom = tilesInRoom;
		actions = new List<PLAYER_SKILL_TYPE>();
		worldPosition = GetCenterTile().centeredWorldLocation;
		int num = tilesInRoom.Max((LocationGridTile t) => t.localPlace.x);
		int num2 = tilesInRoom.Min((LocationGridTile t) => t.localPlace.x);
		int num3 = tilesInRoom.Max((LocationGridTile t) => t.localPlace.y);
		int num4 = tilesInRoom.Min((LocationGridTile t) => t.localPlace.y);
		selectableSize = new Vector2(num - num2 + 1, num3 - num4 + 1);
		Initialize();
	}

	public StructureRoom(SaveDataStructureRoom data)
	{
		name = data.name;
		tilesInRoom = SaveUtilities.ConvertIDListToLocationGridTiles(data.tilesInRoom);
		actions = new List<PLAYER_SKILL_TYPE>();
		worldPosition = GetCenterTile().centeredWorldLocation;
		int num = tilesInRoom.Max((LocationGridTile t) => t.localPlace.x);
		int num2 = tilesInRoom.Min((LocationGridTile t) => t.localPlace.x);
		int num3 = tilesInRoom.Max((LocationGridTile t) => t.localPlace.y);
		int num4 = tilesInRoom.Min((LocationGridTile t) => t.localPlace.y);
		selectableSize = new Vector2(num - num2 + 1, num3 - num4 + 1);
		Initialize();
	}

	public virtual void LoadReferences(SaveDataStructureRoom saveDataStructureRoom)
	{
	}

	public virtual void LoadAdditionalReferences(SaveDataStructureRoom saveDataStructureRoom)
	{
	}

	public virtual void LoadStructureRoomSecondWaveInMainThread(SaveDataStructureRoom saveDataStructureRoom)
	{
	}

	private void Initialize()
	{
		ConstructDefaultPlayerActions();
	}

	public virtual void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
	}

	public void AddPlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true)
	{
		if (!actions.Contains(action))
		{
			actions.Add(action);
		}
	}

	public void RemovePlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true)
	{
		actions.Remove(action);
	}

	public void ClearPlayerActions()
	{
		actions.Clear();
	}

	public bool IsCurrentlySelected()
	{
		return UIManager.Instance.structureRoomInfoUI.activeRoom == this;
	}

	public void LeftSelectAction()
	{
		UIManager.Instance.ShowStructureRoomInfo(this);
	}

	public void RightSelectAction()
	{
		Vector3 p_followTarget = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(InputManager.Instance.mousePosition);
		UIManager.Instance.ShowPlayerActionContextMenu(this, p_followTarget, p_isScreenPosition: false);
	}

	public void MiddleSelectAction()
	{
	}

	public virtual bool CanBeSelected()
	{
		return true;
	}

	public void PopulateCharactersInRoom(List<Character> p_characters)
	{
		for (int i = 0; i < parentStructure.charactersHere.Count; i++)
		{
			Character character = parentStructure.charactersHere[i];
			if (character.gridTileLocation != null && parentStructure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room == this)
			{
				p_characters.Add(character);
			}
		}
	}

	public Character GetFirstAliveCharacterInRoom()
	{
		for (int i = 0; i < tilesInRoom.Count; i++)
		{
			LocationGridTile locationGridTile = tilesInRoom[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				Character character = locationGridTile.charactersHere[j];
				if (!character.isDead)
				{
					return character;
				}
			}
		}
		return null;
	}

	public bool HasCharacterInRoom()
	{
		for (int i = 0; i < tilesInRoom.Count; i++)
		{
			LocationGridTile locationGridTile = tilesInRoom[i];
			for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
			{
				if (!locationGridTile.charactersHere[j].isDead)
				{
					return true;
				}
			}
		}
		return false;
	}

	public T GetTileObjectInRoom<T>() where T : TileObject
	{
		for (int i = 0; i < tilesInRoom.Count; i++)
		{
			LocationGridTile locationGridTile = tilesInRoom[i];
			if (locationGridTile.tileObjectComponent.objHere != null && locationGridTile.tileObjectComponent.objHere is T result)
			{
				return result;
			}
		}
		return null;
	}

	public LocationGridTile GetCenterTile()
	{
		return GameUtilities.GetCenterTile(tilesInRoom, tilesInRoom[0].parentMap.map);
	}

	public bool HasAnyAliveCharacterInRoom()
	{
		return GetFirstAliveCharacterInRoom() != null;
	}

	public virtual bool CanUnseizeCharacterInRoom(Character character)
	{
		return true;
	}

	public virtual void OnParentStructureDestroyed()
	{
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		_ = parentStructure;
	}

	public virtual void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}

	public void CleanUp()
	{
		tilesInRoom?.Clear();
	}
}
