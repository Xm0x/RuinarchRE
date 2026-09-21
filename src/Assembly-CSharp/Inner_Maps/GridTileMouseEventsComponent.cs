using Inner_Maps.Location_Structures;
using UnityEngine;

namespace Inner_Maps;

public class GridTileMouseEventsComponent : LocationGridTileComponent
{
	public bool isHovering { get; private set; }

	public GridTileMouseEventsComponent()
	{
	}

	public GridTileMouseEventsComponent(SaveDataGridTileMouseEventsComponent data)
	{
	}

	public void OnTileEvents()
	{
		if (!GameManager.Instance.gameHasStarted)
		{
			return;
		}
		if (base.owner.corruptionComponent.CanCorruptTile())
		{
			Cost corruptTileCost = EditableValuesManager.Instance.GetCorruptTileCost();
			if (PlayerManager.Instance.player.currenciesComponent.CanAfford(corruptTileCost))
			{
				PlayerManager.Instance.player.currenciesComponent.ReduceCurrency(corruptTileCost);
				base.owner.corruptionComponent.StartCorruption(randomGenerateDemonicDecor: true);
			}
			else
			{
				InnerMapManager.Instance.ShowAreaMapTextPopup(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Not_Enough_" + corruptTileCost.currency.ToStringEnum()), base.owner.centeredWorldLocation, Color.white);
				AudioManager.Instance.OnErrorSoundPlay();
			}
		}
		else if (base.owner.corruptionComponent.CanDisruptCorruptionOfTile())
		{
			Cost corruptTileCost2 = EditableValuesManager.Instance.GetCorruptTileCost();
			if (corruptTileCost2.currency == CURRENCY.Chaotic_Energy)
			{
				PlayerManager.Instance.player.currenciesComponent.AdjustChaoticEnergyWithoutAffectingSpiritEnergy(corruptTileCost2.processedAmount);
			}
			else
			{
				PlayerManager.Instance.player.currenciesComponent.AddCurrency(corruptTileCost2);
			}
			base.owner.corruptionComponent.DisruptCorruption();
		}
		else if (base.owner.corruptionComponent.CanBuildDemonicWall())
		{
			Cost buildWallCost = EditableValuesManager.Instance.GetBuildWallCost();
			if (PlayerManager.Instance.player.currenciesComponent.CanAfford(buildWallCost))
			{
				PlayerManager.Instance.player.currenciesComponent.ReduceCurrency(buildWallCost);
				base.owner.corruptionComponent.StartBuildDemonicWall();
			}
			else
			{
				InnerMapManager.Instance.ShowAreaMapTextPopup(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Not_Enough_" + buildWallCost.currency.ToStringEnum()), base.owner.centeredWorldLocation, Color.white);
				AudioManager.Instance.OnErrorSoundPlay();
			}
		}
	}

	public void OnHoverEnter()
	{
		if (GameManager.Instance.gameHasStarted && !isHovering)
		{
			isHovering = true;
			if (base.owner.corruptionComponent.CanCorruptTile())
			{
				OnHoverEnterTileAdjacentToCorruption();
			}
			else if (base.owner.corruptionComponent.CanDisruptCorruptionOfTile())
			{
				OnHoverEnterBeingCorrupted();
			}
			else if (base.owner.corruptionComponent.CanBuildDemonicWall())
			{
				OnHoverEnterCorrupted();
			}
		}
	}

	public void OnHoverExit()
	{
		if (isHovering)
		{
			isHovering = false;
			UIManager.Instance.HideSmallInfo();
		}
	}

	private void OnHoverEnterTileAdjacentToCorruption()
	{
		Cost corruptTileCost = EditableValuesManager.Instance.GetCorruptTileCost();
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Corrupt_Tile") + " " + corruptTileCost.GetCostStringWithIcon());
	}

	private void OnHoverEnterBeingCorrupted()
	{
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Uncorrupt_Tile"));
	}

	private void OnHoverEnterCorrupted()
	{
		Cost buildWallCost = EditableValuesManager.Instance.GetBuildWallCost();
		UIManager.Instance.ShowSmallInfo(LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Build_Wall") + " " + buildWallCost.GetCostStringWithIcon());
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
