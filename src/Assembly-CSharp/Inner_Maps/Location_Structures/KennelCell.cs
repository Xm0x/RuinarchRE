using System.Collections.Generic;
using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class KennelCell : StructureRoom
{
	public KennelCell(List<LocationGridTile> tilesInRoom)
		: base("Kennel Cell", tilesInRoom)
	{
		Vector3 centeredWorldLocation = GetCenterTile().centeredWorldLocation;
		centeredWorldLocation.x += 0.5f;
		base.worldPosition = centeredWorldLocation;
	}

	public override bool CanUnseizeCharacterInRoom(Character character)
	{
		if (HasAnyAliveCharacterInRoom())
		{
			return false;
		}
		return IsValidOccupant(character);
	}

	public void OnHarpyDroppedCharacterHere(Character character)
	{
		if (character is Summon summon && base.parentStructure is Kennel kennel)
		{
			summon.traitContainer.RestrainAndImprison(summon, null, PlayerManager.Instance.player.playerFaction);
			if (kennel.occupyingSummon == null && IsValidOccupant(summon))
			{
				kennel.OccupyKennel(summon);
			}
		}
	}

	private bool IsValidOccupant(Character p_character)
	{
		if (p_character is Summon summon)
		{
			if (summon.isDead)
			{
				return false;
			}
			if (summon.faction != null && summon.faction.isPlayerFaction)
			{
				return false;
			}
			if (!summon.traitContainer.HasTrait("Restrained"))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void PopulateOccupants(List<Character> p_characters)
	{
		for (int i = 0; i < base.parentStructure.charactersHere.Count; i++)
		{
			Character character = base.parentStructure.charactersHere[i];
			if (character.gridTileLocation != null && character.gridTileLocation.structure.IsTilePartOfARoom(character.gridTileLocation, out var room) && room == this)
			{
				p_characters.Add(character);
			}
		}
	}

	public override bool CanBeSelected()
	{
		return false;
	}
}
