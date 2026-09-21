using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class ManifestFoodData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.MANIFEST_FOOD;

	public override string name => "Manifest Food";

	public override string description => "This Spell produces a pile of food out of thin air. Use it to lure characters.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public ManifestFoodData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		TILE_OBJECT_TYPE tileObjectType = TILE_OBJECT_TYPE.FISH_PILE;
		if (base.currentLevel == 1)
		{
			tileObjectType = (GameUtilities.RollChance(50) ? TILE_OBJECT_TYPE.FISH_PILE : TILE_OBJECT_TYPE.ANIMAL_MEAT);
		}
		else if (base.currentLevel == 2)
		{
			List<TILE_OBJECT_TYPE> list = RuinarchListPool<TILE_OBJECT_TYPE>.Claim();
			list.Add(TILE_OBJECT_TYPE.FISH_PILE);
			list.Add(TILE_OBJECT_TYPE.ANIMAL_MEAT);
			list.Add(TILE_OBJECT_TYPE.CORN);
			list.Add(TILE_OBJECT_TYPE.POTATO);
			tileObjectType = CollectionUtilities.GetRandomElement(list);
			RuinarchListPool<TILE_OBJECT_TYPE>.Release(list);
		}
		else if (base.currentLevel == 3)
		{
			List<TILE_OBJECT_TYPE> list2 = RuinarchListPool<TILE_OBJECT_TYPE>.Claim();
			list2.Add(TILE_OBJECT_TYPE.FISH_PILE);
			list2.Add(TILE_OBJECT_TYPE.ANIMAL_MEAT);
			list2.Add(TILE_OBJECT_TYPE.CORN);
			list2.Add(TILE_OBJECT_TYPE.POTATO);
			list2.Add(TILE_OBJECT_TYPE.ICEBERRY);
			list2.Add(TILE_OBJECT_TYPE.PINEAPPLE);
			tileObjectType = CollectionUtilities.GetRandomElement(list2);
			RuinarchListPool<TILE_OBJECT_TYPE>.Release(list2);
		}
		FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(tileObjectType);
		foodPile.SetResourceInPile(20);
		foodPile.SetFoodAsFromManifestFood();
		targetTile.structure.AddPOI(foodPile, targetTile);
		GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Smoke_Effect);
		AkSoundEngine.PostEvent("Play_Manifest_Food", foodPile.mapObjectVisual.gameObject);
		base.ActivateAbility(targetTile);
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		bool flag = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		if (flag)
		{
			if (targetTile.elevationType == ELEVATION.WATER)
			{
				return false;
			}
			return targetTile.tileObjectComponent.objHere == null;
		}
		return flag;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(0, tile);
	}
}
