using System;
using Inner_Maps;
using Inner_Maps.Location_Structures;

[Serializable]
public class Infested : FactionIdeology
{
	public Infested()
		: base(FACTION_IDEOLOGY.Infested)
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
		TrySpawningAbomination(p_deadCharacter);
	}

	private void TrySpawningAbomination(Character p_deadCharacter)
	{
		if (p_deadCharacter.race != RACE.ABOMINATION)
		{
			SpawnAbomination(p_deadCharacter);
		}
	}

	private void SpawnAbomination(Character p_deadCharacter)
	{
		LocationGridTile locationGridTile = p_deadCharacter.gridTileLocation;
		if (locationGridTile == null)
		{
			locationGridTile = p_deadCharacter.deathTilePosition;
		}
		if (locationGridTile != null)
		{
			LocationStructure structure = locationGridTile.structure;
			if (structure is TortureChambers tortureChambers)
			{
				locationGridTile = tortureChambers.GetRandomPassableBorderTile();
				if (locationGridTile == null)
				{
					locationGridTile = tortureChambers.GetRandomBorderTile();
				}
			}
			else if (structure is Kennel { occupyingSummon: not null } kennel)
			{
				locationGridTile = kennel.GetRandomPassableBorderTile();
				if (locationGridTile == null)
				{
					locationGridTile = kennel.GetRandomBorderTile();
				}
			}
		}
		if (locationGridTile != null)
		{
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Abomination, p_deadCharacter.faction, homeRegion: locationGridTile.parentMap.region, homeLocation: p_deadCharacter.homeSettlement, homeStructure: null, className: "", bypassIdeologyChecking: true);
			CharacterManager.Instance.PlaceSummonInitially(summon, locationGridTile);
			if (locationGridTile.structure is Kennel kennel2)
			{
				summon.traitContainer.RestrainAndImprison(summon, null, PlayerManager.Instance.player.playerFaction);
				kennel2.OccupyKennel(summon);
			}
			GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
		}
	}
}
