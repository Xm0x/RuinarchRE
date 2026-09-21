public class EyeWardObjectGameObject : TileObjectGameObject
{
	public override void Initialize(TileObject tileObject)
	{
		base.Initialize(tileObject);
		visionTrigger.SetVisionTriggerCollidersState(state: false);
	}
}
