using System.Collections;
using UtilityScripts;

namespace Generator.Map_Generation.Components;

public class FactionFinalization : MapGenerationComponent
{
	private readonly FACTION_IDEOLOGY[] _uniqueIdeologiesInWorldGen = new FACTION_IDEOLOGY[4]
	{
		FACTION_IDEOLOGY.Wyvern_Tamers,
		FACTION_IDEOLOGY.Lightning_Tower_Defense,
		FACTION_IDEOLOGY.Tower_Defense,
		FACTION_IDEOLOGY.Breeders
	};

	public override IEnumerator ExecuteRandomGeneration(MapGenerationData data)
	{
		GenerateFactionLeadersAndFinalizeIdeologies();
		GenerateSettlementRulers();
		yield return null;
	}

	private void GenerateSettlementRulers()
	{
		for (int i = 0; i < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; i++)
		{
			NPCSettlement nPCSettlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[i];
			if (nPCSettlement.locationType == LOCATION_TYPE.VILLAGE)
			{
				if (nPCSettlement.ruler == null)
				{
					nPCSettlement.DesignateNewRuler(willLog: false);
				}
				nPCSettlement.GenerateInitialOpinionBetweenResidents();
			}
		}
	}

	private void GenerateFactionLeadersAndFinalizeIdeologies()
	{
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++)
		{
			Faction faction = FactionManager.Instance.allFactions[i];
			if (!faction.isMajorNonPlayer)
			{
				continue;
			}
			faction.successionComponent.UpdateSuccessors();
			faction.DesignateNewLeader(willLog: false);
			if (faction.leader is Character leader)
			{
				FactionManager.Instance.RerollFactionLeaderTraitIdeology(faction, leader);
				FactionManager.Instance.RerollSpecialIdeologies(faction, leader);
				if (!faction.factionType.HasPeaceTypeIdeology())
				{
					FactionManager.Instance.RerollPeaceTypeIdeology(faction, leader);
				}
			}
			faction.GenerateInitialOpinionBetweenMembers();
			if (GameUtilities.RollChance(12))
			{
				FACTION_IDEOLOGY ideologyType = _uniqueIdeologiesInWorldGen[Utilities.Rng.Next(0, _uniqueIdeologiesInWorldGen.Length)];
				FactionIdeology ideology = FactionManager.Instance.CreateIdeology<FactionIdeology>(ideologyType);
				faction.factionType.AddIdeology(ideology, faction);
			}
		}
	}
}
