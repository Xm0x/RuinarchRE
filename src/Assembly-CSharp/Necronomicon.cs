using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UtilityScripts;

public class Necronomicon : Artifact
{
	public Necronomicon()
		: base(ARTIFACT_TYPE.Necronomicon)
	{
		base.maxHP = 700;
		base.currentHP = base.maxHP;
		base.traitContainer.AddTrait(this, "Treasure");
	}

	public Necronomicon(SaveDataArtifact data)
		: base(data)
	{
	}

	public override void SetInventoryOwner(Character p_newOwner)
	{
		if (base.isBeingCarriedBy != p_newOwner)
		{
			base.SetInventoryOwner(p_newOwner);
			if (base.isBeingCarriedBy != null)
			{
				base.isBeingCarriedBy.interruptComponent.NecromanticTransform();
			}
		}
	}

	public override void ActivateTileObject()
	{
		if (gridTileLocation != null)
		{
			base.ActivateTileObject();
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			gridTileLocation.PopulateTilesInRadius(list, 1);
			LocationGridTile locationGridTile = null;
			LocationGridTile locationGridTile2 = null;
			LocationGridTile locationGridTile3 = null;
			int index = Random.Range(0, list.Count);
			locationGridTile = list[index];
			list.RemoveAt(index);
			locationGridTile2 = locationGridTile;
			locationGridTile3 = locationGridTile;
			if (list.Count > 0)
			{
				int index2 = Random.Range(0, list.Count);
				locationGridTile2 = list[index2];
				list.RemoveAt(index2);
			}
			if (list.Count > 0)
			{
				int index3 = Random.Range(0, list.Count);
				locationGridTile3 = list[index3];
			}
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.undeadFaction, null, gridTileLocation.parentMap.region, null, "Marauder");
			Summon summon2 = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.undeadFaction, null, gridTileLocation.parentMap.region, null, "Archer");
			Summon summon3 = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Skeleton, FactionManager.Instance.undeadFaction, null, gridTileLocation.parentMap.region, null, "Mage");
			CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile);
			CharacterManager.Instance.PlaceSummonInitially(summon2, locationGridTile2);
			CharacterManager.Instance.PlaceSummonInitially(summon3, locationGridTile3);
			GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Necronomicon_Activate);
			RuinarchListPool<LocationGridTile>.Release(list);
		}
	}
}
