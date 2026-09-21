using System.Collections.Generic;
using Traits;

public abstract class BaseMapObject
{
	public MAP_OBJECT_STATE mapObjectState { get; private set; }

	public IObjectManipulator lastManipulatedBy { get; private set; }

	public abstract BaseMapObjectVisual baseMapObjectVisual { get; }

	public void SetMapObjectState(MAP_OBJECT_STATE state)
	{
		if (mapObjectState != state)
		{
			mapObjectState = state;
			OnMapObjectStateChanged();
		}
	}

	protected abstract void OnMapObjectStateChanged();

	public void OnManipulatedBy(IObjectManipulator newManipulator)
	{
		IObjectManipulator objectManipulator = lastManipulatedBy;
		lastManipulatedBy = newManipulator;
		if (baseMapObjectVisual != null && baseMapObjectVisual.visionTrigger != null)
		{
			if (newManipulator is Player && !(objectManipulator is Player))
			{
				baseMapObjectVisual.visionTrigger.VoteToMakeVisibleToCharacters();
			}
			else if (newManipulator is Character && objectManipulator is Player)
			{
				baseMapObjectVisual.visionTrigger.VoteToMakeInvisibleToCharacters();
			}
		}
	}

	protected void DisableGameObject()
	{
		baseMapObjectVisual.SetActiveState(state: false);
	}

	protected void EnableGameObject()
	{
		baseMapObjectVisual.SetActiveState(state: true);
	}

	public virtual void DestroyMapVisualGameObject()
	{
		if (!(baseMapObjectVisual != null))
		{
			return;
		}
		if (baseMapObjectVisual.selectable is TileObject tileObject)
		{
			List<Trait> traitOverrideFunctions = tileObject.traitContainer.GetTraitOverrideFunctions("Destroy_Map_Visual_Trait");
			if (traitOverrideFunctions != null)
			{
				for (int i = 0; i < traitOverrideFunctions.Count; i++)
				{
					traitOverrideFunctions[i].OnDestroyMapObjectVisual(tileObject);
				}
			}
		}
		ObjectPoolManager.Instance.DestroyObject(baseMapObjectVisual);
	}

	public virtual string GetAdditionalTestingData()
	{
		string text = $"<b>Last Manipulated by:</b> {lastManipulatedBy}";
		if (baseMapObjectVisual != null)
		{
			text = text + "<b>Vision Votes:</b> " + baseMapObjectVisual.visionTrigger?.filterVotes.ToString();
		}
		return text + "<b>Map Object state:</b> " + mapObjectState.ToStringEnum();
	}
}
