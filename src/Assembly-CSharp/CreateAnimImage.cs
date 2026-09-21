using System.Collections.Generic;
using UnityEngine;

public class CreateAnimImage : MonoBehaviour
{
	public CreateAnimImage[] createImageOtherReference;

	public GameObject CreateInstance;

	public int HowManyButtons;

	public Vector3 StartAnim;

	public Vector3 EndAnim;

	public float Offset;

	public AnimationCurve EnterAnim;

	public AnimationCurve ExitAnim;

	public RectTransform RootRect;

	public RectTransform RootCanvas;

	private List<EasyTween> Created = new List<EasyTween>();

	private Vector2 InitialCanvasScrollSize;

	private float totalWidth;

	private void Start()
	{
		InitialCanvasScrollSize = new Vector2(RootRect.rect.height, RootRect.rect.width);
	}

	public void CallBack()
	{
		if (Created.Count == 0)
		{
			for (int i = 0; i < createImageOtherReference.Length; i++)
			{
				createImageOtherReference[i].DestroyButtons();
			}
			CreateButtons();
		}
	}

	public void DestroyButtons()
	{
		for (int i = 0; i < Created.Count; i++)
		{
			Created[i].OpenCloseObjectAnimation();
		}
		Created.Clear();
	}

	public void CreateButtons()
	{
		CreatePanels();
		AdaptCanvas();
	}

	private void CreatePanels()
	{
		Vector3 endAnim = EndAnim;
		totalWidth = 0f;
		for (int i = 0; i < HowManyButtons; i++)
		{
			GameObject obj = Object.Instantiate(CreateInstance);
			obj.transform.SetParent(RootRect, worldPositionStays: false);
			EasyTween component = obj.GetComponent<EasyTween>();
			Created.Add(component);
			StartAnim.y = endAnim.y;
			component.SetAnimationPosition(StartAnim, endAnim, EnterAnim, ExitAnim);
			component.SetFade();
			component.OpenCloseObjectAnimation();
			endAnim.y += Offset;
			totalWidth += Offset;
		}
	}

	private void AdaptCanvas()
	{
		if (InitialCanvasScrollSize.x < Mathf.Abs(totalWidth))
		{
			RootRect.offsetMin = new Vector2(RootRect.offsetMin.x, totalWidth + InitialCanvasScrollSize.x + RootRect.offsetMax.y);
		}
	}
}
