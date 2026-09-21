namespace Inner_Maps.Location_Structures;

public class BerryGarden : NaturalStructureWithStructureObject
{
	public BerryGarden(Region location)
		: base(STRUCTURE_TYPE.BERRY_GARDEN, location)
	{
		SetMaxHPAndReset(6000);
	}

	public BerryGarden(Region location, SaveDataNaturalStructureWithStructureObject data)
		: base(location, data)
	{
		SetMaxHP(6000);
	}

	public override void CenterOnStructure()
	{
		if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != base.region.innerMap)
		{
			InnerMapManager.Instance.HideAreaMap();
		}
		if (!base.region.innerMap.isShowing)
		{
			InnerMapManager.Instance.ShowInnerMap(base.region);
		}
		if (base.occupiedArea != null)
		{
			InnerMapCameraMove.Instance.CenterCameraOn(base.occupiedArea.gridTileComponent.centerGridTile.centeredWorldLocation);
		}
	}

	public override void ShowSelectorOnStructure()
	{
	}
}
