using DG.Tweening;
using TMPro;
using UnityEngine;

public class AchievementNotification : MonoBehaviour
{
	[SerializeField]
	private int expirySeconds;

	[SerializeField]
	private RectTransform thisRect;

	[SerializeField]
	private GameObject achievementContainerGO;

	[SerializeField]
	private TextMeshProUGUI titleLbl;

	[SerializeField]
	private TextMeshProUGUI descriptionLbl;

	private bool _isShowing;

	private bool _hasStartedExpiry;

	private float _currentExpirySeconds;

	private Achievement _achievement;

	private void Update()
	{
		if (!_hasStartedExpiry)
		{
			return;
		}
		_currentExpirySeconds += Time.deltaTime;
		if (_currentExpirySeconds >= (float)expirySeconds)
		{
			StopExpiry();
			if (_isShowing)
			{
				HideNotification();
			}
		}
	}

	public void ShowNotification(Achievement p_achievement)
	{
		_isShowing = true;
		_achievement = p_achievement;
		thisRect.DOKill();
		ResetPositionAndSize();
		ShowHideAchievementContainer(p_state: false);
		AchievementNotificationShowTween();
	}

	public void HideNotification()
	{
		_isShowing = false;
		thisRect.DOKill();
		ShowHideAchievementContainer(p_state: false);
		AchievementNotificationHideTween();
	}

	private void ResetPositionAndSize()
	{
		thisRect.anchoredPosition = new Vector2(thisRect.anchoredPosition.x, 150f);
		thisRect.sizeDelta = new Vector2(0f, thisRect.sizeDelta.y);
	}

	private void ShowHideAchievementContainer(bool p_state)
	{
		achievementContainerGO.SetActive(p_state);
	}

	private void AchievementNotificationShowTween()
	{
		AchievementNotificationEntranceTween();
		thisRect.DOSizeDelta(new Vector2(500f, thisRect.sizeDelta.y), 0.5f).SetEase(Ease.OutExpo).OnComplete(ShowAchievementMessage);
	}

	private void AchievementNotificationHideTween()
	{
		AchievementNotificationExitTween();
		thisRect.DOSizeDelta(new Vector2(0f, thisRect.sizeDelta.y), 0.5f).SetEase(Ease.InExpo);
	}

	private void AchievementNotificationEntranceTween()
	{
		thisRect.DOAnchorPosY(-160f, 0.5f).SetEase(Ease.OutFlash);
	}

	private void AchievementNotificationExitTween()
	{
		thisRect.DOAnchorPosY(150f, 0.5f).SetEase(Ease.InBack);
	}

	private void ShowAchievementMessage()
	{
		string localizedValue = LocalizationManager.Instance.GetLocalizedValue("Achievements_Table", _achievement.nameKey);
		string localizedValue2 = LocalizationManager.Instance.GetLocalizedValue("Achievements_Table", _achievement.descriptionKey);
		titleLbl.text = localizedValue;
		descriptionLbl.text = localizedValue2;
		ShowHideAchievementContainer(p_state: true);
		StartExpiry();
	}

	private void StartExpiry()
	{
		_currentExpirySeconds = 0f;
		_hasStartedExpiry = true;
	}

	private void StopExpiry()
	{
		_hasStartedExpiry = false;
	}

	public void OnClickClose()
	{
		if (_isShowing)
		{
			HideNotification();
		}
	}
}
