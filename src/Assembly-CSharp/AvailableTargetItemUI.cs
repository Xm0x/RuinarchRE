using System;
using UnityEngine;
using UnityEngine.UI;

public class AvailableTargetItemUI : MonoBehaviour
{
	public Action<AvailableTargetItemUI> onClicked;

	public Action<AvailableTargetItemUI> onHoverOver;

	public Action<AvailableTargetItemUI> onHoverOut;

	public Button myButton;

	public RuinarchText txtName;

	public IStoredTarget target;

	public GameObject goCover;

	public HoverText hoverText;

	public HoverHandler hoverHandler;

	private void Awake()
	{
		hoverText = goCover.GetComponent<HoverText>();
	}

	public void InitializeItem(IStoredTarget p_target, bool showCover)
	{
		target = p_target;
		if (showCover)
		{
			ShowCover();
		}
		else
		{
			HideCover();
		}
		txtName.text = p_target.bookmarkName ?? "";
	}

	public void SetHoverText(string p_text)
	{
		hoverText.SetText(p_text);
	}

	private void OnEnable()
	{
		myButton.onClick.AddListener(Click);
		hoverHandler.AddOnHoverOverAction(OnHoverOver);
		hoverHandler.AddOnHoverOutAction(OnHoverOut);
	}

	private void OnDisable()
	{
		myButton.onClick.RemoveListener(Click);
		hoverHandler.RemoveOnHoverOverAction(OnHoverOver);
		hoverHandler.RemoveOnHoverOutAction(OnHoverOut);
	}

	public void ShowCover()
	{
		myButton.interactable = false;
		goCover.SetActive(value: true);
	}

	public void HideCover()
	{
		myButton.interactable = true;
		goCover.SetActive(value: false);
	}

	private void Click()
	{
		onClicked?.Invoke(this);
	}

	private void OnHoverOver()
	{
		onHoverOver?.Invoke(this);
	}

	private void OnHoverOut()
	{
		onHoverOut?.Invoke(this);
	}
}
