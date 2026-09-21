using System.Collections;
using DG.Tweening;
using Ruinarch.Custom_UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
[ExecuteInEditMode]
public class CustomVerticalScrollSnap : MonoBehaviour, IScrollHandler, IEventSystemHandler
{
	[SerializeField]
	private ScrollRect _scrollRect;

	[SerializeField]
	private float _distancePerScroll;

	[SerializeField]
	private float _durationPerScroll;

	[SerializeField]
	private float _offsetPadding;

	[SerializeField]
	private Ease _scrollMovementType;

	[SerializeField]
	private bool _loopScroll;

	[SerializeField]
	private RuinarchButton _nextButton;

	[SerializeField]
	private RuinarchButton _prevButton;

	private float _supposedPositionY;

	private float maxScrollHeight => _scrollRect.content.sizeDelta.y - _scrollRect.viewport.rect.height - _offsetPadding;

	private void Start()
	{
		InitializeButtons();
		UpdateButtonsInteractability();
	}

	private void OnEnable()
	{
		ForceUpdateButtonsInteractability();
	}

	public void OnScroll(PointerEventData p_data)
	{
		if (p_data.scrollDelta.y < 0f)
		{
			OnScrollDown();
		}
		else if (p_data.scrollDelta.y > 0f)
		{
			OnScrollUp();
		}
	}

	private void OnScrollUp()
	{
		if (_scrollRect.content.anchoredPosition.y <= 0f)
		{
			if (_loopScroll)
			{
				ScrollToBottom();
			}
		}
		else if (_supposedPositionY <= 0f)
		{
			if (_loopScroll)
			{
				ScrollToBottom();
			}
		}
		else
		{
			_supposedPositionY -= _distancePerScroll;
			_supposedPositionY = Mathf.Max(0f, _supposedPositionY);
			_scrollRect.content.DOAnchorPosY(_supposedPositionY, _durationPerScroll).SetEase(_scrollMovementType).OnComplete(UpdateButtonsInteractability);
		}
	}

	private void OnScrollDown()
	{
		float num = maxScrollHeight;
		if (_scrollRect.content.anchoredPosition.y >= num)
		{
			if (_loopScroll)
			{
				ScrollToTop();
			}
		}
		else if (_supposedPositionY >= num)
		{
			if (_loopScroll)
			{
				ScrollToTop();
			}
		}
		else
		{
			_supposedPositionY += _distancePerScroll;
			_supposedPositionY = Mathf.Min(num, _supposedPositionY);
			_scrollRect.content.DOAnchorPosY(_supposedPositionY, _durationPerScroll).SetEase(_scrollMovementType).OnComplete(UpdateButtonsInteractability);
		}
	}

	private void ScrollToTop()
	{
		_supposedPositionY = 0f;
		_scrollRect.content.DOAnchorPosY(_supposedPositionY, _durationPerScroll).SetEase(_scrollMovementType).OnComplete(UpdateButtonsInteractability);
	}

	private void ScrollToBottom()
	{
		_supposedPositionY = maxScrollHeight;
		_scrollRect.content.DOAnchorPosY(_supposedPositionY, _durationPerScroll).SetEase(_scrollMovementType).OnComplete(UpdateButtonsInteractability);
	}

	private void InitializeButtons()
	{
		if (_nextButton != null && _nextButton.onClick != null)
		{
			_nextButton.onClick.AddListener(OnScrollDown);
		}
		if (_prevButton != null && _prevButton.onClick != null)
		{
			_prevButton.onClick.AddListener(OnScrollUp);
		}
	}

	private void UpdateButtonsInteractability()
	{
		int num = Mathf.RoundToInt((_scrollRect.content.sizeDelta.y - _offsetPadding) / _scrollRect.viewport.rect.height);
		if (_nextButton != null)
		{
			_nextButton.interactable = (_loopScroll || _scrollRect.content.anchoredPosition.y < maxScrollHeight) && num > 1;
		}
		if (_prevButton != null)
		{
			_prevButton.interactable = (_loopScroll || _scrollRect.content.anchoredPosition.y > 0f) && num > 1;
		}
	}

	public void ForceUpdateButtonsInteractability()
	{
		StartCoroutine(IUpdateButtonsInteractability());
	}

	private IEnumerator IUpdateButtonsInteractability()
	{
		yield return null;
		UpdateButtonsInteractability();
	}
}
