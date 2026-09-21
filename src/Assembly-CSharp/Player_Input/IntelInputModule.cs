using Inner_Maps;
using Ruinarch;

namespace Player_Input;

public class IntelInputModule : PlayerInputModule
{
	public override void OnUpdate()
	{
		IPointOfInterest currentlyHoveredPoi = InnerMapManager.Instance.currentlyHoveredPoi;
		if (currentlyHoveredPoi != null)
		{
			string hoverText = string.Empty;
			InputManager.Instance.SetCursorTo(PlayerManager.Instance.player.CanShareIntelTo(currentlyHoveredPoi, ref hoverText, PlayerManager.Instance.player.currentActiveIntel) ? Cursor_Type.Check : Cursor_Type.Cross);
			if (hoverText != string.Empty)
			{
				UIManager.Instance.ShowSmallInfo(hoverText);
			}
		}
		else
		{
			UIManager.Instance.HideSmallInfo();
			InputManager.Instance.SetCursorTo(Cursor_Type.Cross);
		}
	}
}
