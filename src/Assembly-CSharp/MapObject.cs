using System.Collections.Generic;
using Inner_Maps;
using Traits;

public abstract class MapObject<T> : BaseMapObject where T : IDamageable
{
	public BaseVisionTrigger visionTrigger => mapVisual.visionTrigger;

	public virtual MapObjectVisual<T> mapVisual { get; protected set; }

	public override BaseMapObjectVisual baseMapObjectVisual => mapVisual;

	protected abstract void CreateMapObjectVisual();

	public virtual void InitializeMapObject(T obj)
	{
		CreateMapObjectVisual();
		mapVisual.Initialize(obj);
		InitializeVisionTrigger(obj);
		if (!(obj is TileObject tileObject))
		{
			return;
		}
		tileObject.hiddenComponent.OnSetHiddenState(tileObject);
		List<Trait> traitOverrideFunctions = tileObject.traitContainer.GetTraitOverrideFunctions("Initiate_Map_Visual_Trait");
		if (traitOverrideFunctions != null)
		{
			for (int i = 0; i < traitOverrideFunctions.Count; i++)
			{
				traitOverrideFunctions[i].OnInitiateMapObjectVisual(tileObject);
			}
		}
	}

	protected void PlaceMapObjectAt(LocationGridTile tile)
	{
		mapVisual.PlaceObjectAt(tile);
		visionTrigger.gameObject.SetActive(value: true);
	}

	public override void DestroyMapVisualGameObject()
	{
		base.DestroyMapVisualGameObject();
		mapVisual = null;
	}

	private void InitializeVisionTrigger(T obj)
	{
		visionTrigger.Initialize(obj);
	}
}
