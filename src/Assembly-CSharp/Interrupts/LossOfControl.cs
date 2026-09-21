using System.Collections.Generic;
using Inner_Maps;
using Locations.Area_Features;

namespace Interrupts;

public class LossOfControl : Interrupt
{
	public LossOfControl()
		: base(INTERRUPT.Loss_Of_Control)
	{
		base.duration = 20;
		base.doesDropCurrentJob = true;
		base.doesStopCurrentAction = true;
		base.interruptIconString = GoapActionStateDB.Shock_Icon;
		base.logTags = new LOG_TAG[2]
		{
			LOG_TAG.Needs,
			LOG_TAG.Life_Changes
		};
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", "Mental Break break", base.logTags);
		overrideEffectLog.AddToFillers(interruptHolder.actor, interruptHolder.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		overrideEffectLog.AddToFillers(null, LocalizationManager.Instance.GetLocalizedValue("Interrupts_Table", "Loss of Control"), LOG_IDENTIFIER.STRING_1);
		Area areaLocation = interruptHolder.actor.areaLocation;
		LocationGridTile gridTileLocation = interruptHolder.actor.gridTileLocation;
		if (interruptHolder.actor.characterClass.className.Equals("Druid"))
		{
			List<AOESpellTileObject> activeSpellsOnTile = gridTileLocation.parentMap.region.regionSpellsComponent.GetActiveSpellsOnTile(gridTileLocation);
			ElectricStormTileObject electricStormTileObject = null;
			if (activeSpellsOnTile != null)
			{
				for (int i = 0; i < activeSpellsOnTile.Count; i++)
				{
					if (activeSpellsOnTile[i] is ElectricStormTileObject electricStormTileObject2)
					{
						electricStormTileObject = electricStormTileObject2;
						break;
					}
				}
			}
			if (electricStormTileObject != null)
			{
				electricStormTileObject.ResetElectricStormDuration();
			}
			else
			{
				ElectricStormTileObject poi = InnerMapManager.Instance.CreateNewTileObject<ElectricStormTileObject>(TILE_OBJECT_TYPE.ELECTRIC_STORM_TILE_OBJECT);
				gridTileLocation.structure.AddPOI(poi, gridTileLocation);
			}
		}
		else if (interruptHolder.actor.characterClass.className.Equals("Shaman"))
		{
			PoisonBloomFeature feature = areaLocation.featureComponent.GetFeature<PoisonBloomFeature>();
			if (feature != null)
			{
				feature.ResetDuration();
			}
			else
			{
				areaLocation.featureComponent.AddFeature(AreaFeatureDB.Poison_Bloom_Feature, areaLocation);
			}
		}
		else
		{
			List<AOESpellTileObject> activeSpellsOnTile2 = gridTileLocation.parentMap.region.regionSpellsComponent.GetActiveSpellsOnTile(gridTileLocation);
			BrimstonesTileObject brimstonesTileObject = null;
			if (activeSpellsOnTile2 != null)
			{
				for (int j = 0; j < activeSpellsOnTile2.Count; j++)
				{
					if (activeSpellsOnTile2[j] is BrimstonesTileObject brimstonesTileObject2)
					{
						brimstonesTileObject = brimstonesTileObject2;
						break;
					}
				}
			}
			if (brimstonesTileObject != null)
			{
				brimstonesTileObject.ResetDuration();
			}
			else
			{
				BrimstonesTileObject poi2 = InnerMapManager.Instance.CreateNewTileObject<BrimstonesTileObject>(TILE_OBJECT_TYPE.BRIMSTONES_TILE_OBJECT);
				gridTileLocation.structure.AddPOI(poi2, gridTileLocation);
			}
		}
		return base.ExecuteInterruptStartEffect(interruptHolder, ref overrideEffectLog, goapNode);
	}

	public override bool ExecuteInterruptEndEffect(InterruptHolder interruptHolder)
	{
		interruptHolder.actor.traitContainer.AddTrait(interruptHolder.actor, "Catharsis");
		return base.ExecuteInterruptEndEffect(interruptHolder);
	}
}
