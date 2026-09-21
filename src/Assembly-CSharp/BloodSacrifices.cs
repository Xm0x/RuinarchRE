using System;
using Locations.Settlements;

[Serializable]
public class BloodSacrifices : FactionIdeology
{
	public BloodSacrifices()
		: base(FACTION_IDEOLOGY.Blood_Sacrifices)
	{
		base.daysIntervalSettlementEvent = 4;
	}

	public override bool DoesCharacterFitIdeology(Character character)
	{
		return true;
	}

	public override bool DoesCharacterFitIdeology(PreCharacterData character)
	{
		return true;
	}

	protected override void SettlementEvent(BaseSettlement p_settlement)
	{
		base.SettlementEvent(p_settlement);
		if (p_settlement is NPCSettlement { ruler: var ruler, owner: var owner } nPCSettlement && ruler != null && HasEnoughResidentsInsideVillage(nPCSettlement))
		{
			Character sacrificeTarget = GetSacrificeTarget(ruler, owner, nPCSettlement);
			if (sacrificeTarget != null)
			{
				nPCSettlement.settlementJobTriggerComponent.TryCreateBloodSacrificeJob(sacrificeTarget);
			}
		}
	}

	protected override bool CanCharacterDoIdeologyEventInternal(Character p_character)
	{
		return p_character.isSettlementRuler;
	}

	private bool HasEnoughResidentsInsideVillage(BaseSettlement p_settlement)
	{
		int num = 0;
		for (int i = 0; i < p_settlement.residents.Count; i++)
		{
			Character character = p_settlement.residents[i];
			if (!character.isDead && character.gridTileLocation != null && character.gridTileLocation.IsPartOfSettlement(p_settlement))
			{
				num++;
				if (num >= 5)
				{
					return true;
				}
			}
		}
		return false;
	}

	private Character GetSacrificeTarget(Character p_ruler, Faction p_faction, BaseSettlement p_settlement)
	{
		Character character = null;
		for (int i = 0; i < p_settlement.residents.Count; i++)
		{
			Character character2 = p_settlement.residents[i];
			if (!character2.isDead && character2 != p_ruler && !p_ruler.relationshipContainer.HasRelationshipWith(character2) && character2.gridTileLocation != null && character2.gridTileLocation.IsPartOfSettlement(p_settlement))
			{
				character = character2;
				break;
			}
		}
		if (character == null)
		{
			Character firstAliveCharacterWithLowestOpinionInsideSettlement = p_ruler.relationshipContainer.GetFirstAliveCharacterWithLowestOpinionInsideSettlement(p_faction, p_settlement);
			if (firstAliveCharacterWithLowestOpinionInsideSettlement != null && p_ruler.relationshipContainer.GetTotalOpinion(firstAliveCharacterWithLowestOpinionInsideSettlement) <= 0)
			{
				character = firstAliveCharacterWithLowestOpinionInsideSettlement;
			}
		}
		return character;
	}
}
