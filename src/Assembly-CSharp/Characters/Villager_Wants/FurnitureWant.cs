using System.Collections.Generic;
using UtilityScripts;

namespace Characters.Villager_Wants;

public abstract class FurnitureWant : ItemWant
{
	public override bool CanObjectSatisfyWant(TileObject p_tileObject, Character p_character)
	{
		return p_tileObject.tileObjectType == GetFurnitureWanted(p_character);
	}

	protected bool HasBuildingFurnitureInProgress(Character p_actor)
	{
		return p_actor.homeStructure.HasBuildingTileObjectOfType(GetFurnitureWanted(p_actor));
	}

	protected bool OwnsEnoughResource(Character p_character, int p_amount, out TileObject p_foundObject)
	{
		List<RESOURCE> validCraftResourcesForCharacter = GetValidCraftResourcesForCharacter(p_character);
		for (int i = 0; i < p_character.ownedItems.Count; i++)
		{
			TileObject tileObject = p_character.ownedItems[i];
			if (tileObject is ResourcePile { gridTileLocation: not null } resourcePile && validCraftResourcesForCharacter.Contains(resourcePile.providedResource) && resourcePile.resourceInPile >= p_amount && p_character.movementComponent.HasPathTo(resourcePile.gridTileLocation))
			{
				RuinarchListPool<RESOURCE>.Release(validCraftResourcesForCharacter);
				p_foundObject = tileObject;
				return true;
			}
		}
		p_foundObject = null;
		RuinarchListPool<RESOURCE>.Release(validCraftResourcesForCharacter);
		return false;
	}

	private List<RESOURCE> GetValidCraftResourcesForCharacter(Character p_character)
	{
		List<RESOURCE> list = RuinarchListPool<RESOURCE>.Claim();
		if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire)
		{
			list.Add(RESOURCE.STONE);
		}
		else if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom)
		{
			list.Add(RESOURCE.WOOD);
		}
		else
		{
			list.Add(RESOURCE.STONE);
			list.Add(RESOURCE.WOOD);
		}
		return list;
	}

	public abstract TILE_OBJECT_TYPE GetFurnitureWanted(Character p_character);
}
