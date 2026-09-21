using System;
using System.Collections.Generic;
using Factions.Faction_Types;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

[Serializable]
public class Necromantic : FactionIdeology
{
	public Necromantic()
		: base(FACTION_IDEOLOGY.Necromantic)
	{
	}

	public override bool DoesCharacterFitIdeology(Character character)
	{
		return true;
	}

	public override bool DoesCharacterFitIdeology(PreCharacterData character)
	{
		return true;
	}

	protected override void FactionMemberDied(Character p_deadCharacter)
	{
		TrySpawningUndead(p_deadCharacter);
	}

	protected override void OnAddIdeology(FactionType factionType, Faction p_faction)
	{
		Faction undeadFaction = FactionManager.Instance.undeadFaction;
		if (undeadFaction != null)
		{
			p_faction?.SetRelationshipFor(undeadFaction, FACTION_RELATIONSHIP_STATUS.Friendly);
		}
	}

	private void TrySpawningUndead(Character p_deadCharacter)
	{
		if (p_deadCharacter.race.IsSapient())
		{
			SpawnUndead(p_deadCharacter);
		}
	}

	private void SpawnUndead(Character p_deadCharacter)
	{
		LocationGridTile locationGridTile = p_deadCharacter.gridTileLocation;
		if (locationGridTile == null)
		{
			locationGridTile = p_deadCharacter.deathTilePosition;
		}
		if (locationGridTile != null)
		{
			LocationGridTile locationGridTile2 = null;
			LocationStructure structure = locationGridTile.structure;
			if (structure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS || structure.structureType == STRUCTURE_TYPE.KENNEL)
			{
				locationGridTile = null;
				List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
				structure.PopulateEdgeTiles(list);
				for (int i = 0; i < list.Count; i++)
				{
					LocationGridTile locationGridTile3 = list[i];
					for (int j = 0; j < locationGridTile3.neighbourList.Count; j++)
					{
						LocationGridTile locationGridTile4 = locationGridTile3.neighbourList[j];
						if (locationGridTile4.structure != structure)
						{
							if (locationGridTile4.IsPassable())
							{
								locationGridTile = locationGridTile4;
								break;
							}
							if (locationGridTile2 == null)
							{
								locationGridTile2 = locationGridTile4;
							}
						}
					}
					if (locationGridTile != null)
					{
						break;
					}
				}
				if (locationGridTile == null)
				{
					locationGridTile = locationGridTile2;
				}
				RuinarchListPool<LocationGridTile>.Release(list);
			}
		}
		if (locationGridTile != null)
		{
			SUMMON_TYPE randomUndeadSummon = GetRandomUndeadSummon();
			Summon summon = CharacterManager.Instance.CreateNewSummon(randomUndeadSummon, p_deadCharacter.faction, homeRegion: locationGridTile.parentMap.region, homeLocation: p_deadCharacter.homeSettlement, homeStructure: null, className: "", bypassIdeologyChecking: true);
			summon.SetFirstName(p_deadCharacter.name);
			CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile);
			GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
		}
	}

	private SUMMON_TYPE GetRandomUndeadSummon()
	{
		return GameUtilities.RandomBetweenTwoNumbers(0, 2) switch
		{
			0 => SUMMON_TYPE.Skeleton, 
			1 => SUMMON_TYPE.Ghost, 
			_ => SUMMON_TYPE.Ghoul, 
		};
	}
}
