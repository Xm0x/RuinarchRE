using UnityEngine;

public class FollowClick : MonoBehaviour
{
	public AnimationCurve LeftClick;

	public AnimationCurve RightClick;

	public EasyTween TweenToControl;

	public Transform RootCanvas;

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			MoveToMouseClick(LeftClick);
		}
		else if (Input.GetMouseButtonDown(1))
		{
			MoveToMouseClick(RightClick);
		}
	}

	private void MoveToMouseClick(AnimationCurve animationCurve)
	{
		Vector3 vector = Camera.main.ScreenToViewportPoint(Input.mousePosition);
		vector = new Vector3(vector.x * (float)Screen.width / RootCanvas.localScale.x, vector.y * (float)Screen.height / RootCanvas.localScale.y, 0f);
		if (!TweenToControl.IsObjectOpened())
		{
			TweenToControl.SetAnimationPosition(TweenToControl.rectTransform.anchoredPosition, vector, animationCurve, animationCurve);
		}
		else
		{
			TweenToControl.SetAnimationPosition(vector, TweenToControl.rectTransform.anchoredPosition, animationCurve, animationCurve);
		}
		TweenToControl.OpenCloseObjectAnimation();
	}
}
