using Inner_Maps;
using Ruinarch;

namespace Player_Input;

public class SeizeInputModule : PlayerInputModule
{
	public override void OnUpdate()
	{
		if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing)
		{
			InputManager.Instance.SetCursorTo(Cursor_Type.Default);
			PlayerManager.Instance.player.seizeComponent.DisableFollowMousePosition();
			return;
		}
		PlayerManager.Instance.player.seizeComponent.EnableFollowMousePosition();
		PlayerManager.Instance.player.seizeComponent.FollowMousePosition();
		LocationGridTile tileFromMousePosition = InnerMapManager.Instance.GetTileFromMousePosition();
		IPointOfInterest currentlyHoveredPoi = InnerMapManager.Instance.currentlyHoveredPoi;
		if (tileFromMousePosition != null)
		{
			InputManager.Instance.SetCursorTo(PlayerManager.Instance.player.seizeComponent.CanUnseizeHere(tileFromMousePosition, currentlyHoveredPoi) ? Cursor_Type.Check : Cursor_Type.Cross);
		}
		else
		{
			InputManager.Instance.SetCursorTo(Cursor_Type.Cross);
		}
	}
}
