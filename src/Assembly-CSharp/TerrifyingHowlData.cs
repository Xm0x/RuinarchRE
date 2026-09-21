using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;
using UtilityScripts;

public class TerrifyingHowlData : SkillData
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.TERRIFYING_HOWL;

	public override string name => "Terrifying Howl";

	public override string description => "This Spell releases a bunch of screaming skulls. Their spine-tingling wails will cause all nearby Villager to flee.";

	public override PLAYER_SKILL_CATEGORY category => PLAYER_SKILL_CATEGORY.SPELL;

	public override int radius => 1;

	public TerrifyingHowlData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.TILE };
	}

	public override void ActivateAbility(LocationGridTile targetTile)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		targetTile.PopulateTilesInRadius(list, 1, 0, includeCenterTile: true);
		targetTile.tileObjectComponent.genericTileObject.GetOrCreateMapVisual().visionTrigger.VoteToMakeVisibleToCharacters();
		SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(3), delegate
		{
			targetTile.tileObjectComponent.genericTileObject.TryDestroyMapVisual();
		}, this);
		GameObject in_gameObjectID = GameManager.Instance.CreateParticleEffectAt(targetTile, PARTICLE_EFFECT.Terrifying_Howl);
		AkSoundEngine.PostEvent("Play_Terrifying_Howl", in_gameObjectID);
		RuinarchListPool<LocationGridTile>.Release(list);
		base.ActivateAbility(targetTile);
	}

	private void SpookCharacter(ITraitable traitable, LocationGridTile targetTile)
	{
		if (traitable is Character character)
		{
			character.marker.AddPOIAsInVisionRange(targetTile.tileObjectComponent.genericTileObject);
			character.combatComponent.Flight(targetTile.tileObjectComponent.genericTileObject, "Terrifying_Howl");
		}
	}

	public override bool CanPerformAbilityTowards(LocationGridTile targetTile, out string o_cannotPerformReason)
	{
		bool flag = base.CanPerformAbilityTowards(targetTile, out o_cannotPerformReason);
		if (flag)
		{
			return targetTile.structure != null;
		}
		return flag;
	}

	public override void ShowValidHighlight(LocationGridTile tile)
	{
		TileHighlighter.Instance.PositionHighlight(radius, tile);
	}
}
