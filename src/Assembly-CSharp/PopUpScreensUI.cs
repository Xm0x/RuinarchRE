using System.Collections.Generic;
using DG.Tweening;
using Maccima_Games.Util;
using Ruinarch;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class PopUpScreensUI : MonoBehaviour
{
	[Header("Summary Screen")]
	[SerializeField]
	private CanvasGroup summaryScreen;

	[SerializeField]
	private TextMeshProUGUI summaryLbl;

	[Header("Start Screen")]
	[SerializeField]
	private GameObject startScreen;

	[SerializeField]
	private Image startMessageWindow;

	[SerializeField]
	private CanvasGroup startMessageWindowCG;

	[SerializeField]
	private Button startGameButton;

	[SerializeField]
	private TextMeshProUGUI startGameButtonLbl;

	[SerializeField]
	private TextMeshProUGUI lblStartScreen;

	[Header("End Screen")]
	[SerializeField]
	private GameObject endScreen;

	[SerializeField]
	private Image bgImage;

	[SerializeField]
	private Image ruinarchLogo;

	[SerializeField]
	private TextMeshProUGUI lblWindowMessage;

	[SerializeField]
	private RectTransform thankYouWindow;

	[SerializeField]
	private CanvasGroup endScreenCanvasGroup;

	[SerializeField]
	private Button btnJoinDiscord;

	[SerializeField]
	private Button btnSurvey;

	[SerializeField]
	private Button btnContinue;

	[SerializeField]
	private CanvasGroup topMessageCanvasGroup;

	[SerializeField]
	private CanvasGroup bottomMessageCanvasGroup;

	[SerializeField]
	private GameObject goTopMessage;

	[SerializeField]
	private GameObject goBottomMessage;

	[SerializeField]
	private TextMeshProUGUI lblTopMessage;

	[SerializeField]
	private TextMeshProUGUI lblBottomMessage;

	[SerializeField]
	private TextMeshProUGUI lblContinueBtn;

	[SerializeField]
	private TextMeshProUGUI lblMainMenuBtn;

	public void ShowStartScreen(string message)
	{
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		InnerMapCameraMove.Instance.DisableMovement();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		startScreen.gameObject.SetActive(value: true);
		startMessageWindow.gameObject.SetActive(value: true);
		RectTransform rectTransform = startMessageWindow.rectTransform;
		rectTransform.anchoredPosition = new Vector2(0f, -100f);
		startMessageWindowCG.alpha = 0f;
		lblStartScreen.SetText(message);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(rectTransform.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack));
		sequence.Join(startMessageWindowCG.DOFade(1f, 0.5f).SetEase(Ease.InSine));
		sequence.Play();
	}

	public void OnClickStartGameButton()
	{
		Sequence sequence = DOTween.Sequence();
		sequence.Append(startMessageWindowCG.DOFade(0f, 0.5f).SetEase(Ease.OutSine));
		sequence.OnComplete(HideStartDemoScreen);
		sequence.Play();
	}

	private void HideStartDemoScreen()
	{
		startScreen.gameObject.SetActive(value: false);
		UIManager.Instance.SetSpeedTogglesState(state: true);
		InnerMapCameraMove.Instance.EnableMovement();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
	}

	public bool IsShowingStartScreen()
	{
		return startScreen.gameObject.activeSelf;
	}

	public bool IsShowingEndScreen()
	{
		if (!summaryScreen.gameObject.activeInHierarchy)
		{
			return endScreen.activeInHierarchy;
		}
		return true;
	}

	public void ShowSummaryThenEndScreen(string summary, bool p_isGameWin)
	{
		GameManager.Instance.SetPausedState(isPaused: true);
		UIManager.Instance.SetSpeedTogglesState(state: false);
		UIManager.Instance.HideSmallInfo();
		summaryScreen.alpha = 0f;
		summaryScreen.gameObject.SetActive(value: true);
		summaryLbl.text = summary;
		SetEndScreenMessage(p_isGameWin);
		RectTransform rectTransform = summaryLbl.rectTransform;
		rectTransform.anchoredPosition = new Vector2(0f, -100f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(summaryScreen.DOFade(1f, 0.5f));
		sequence.Append(rectTransform.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutBack));
		sequence.Join(DOTween.ToAlpha(() => summaryLbl.color, delegate(Color value)
		{
			summaryLbl.color = value;
		}, 1f, 0.5f));
		sequence.AppendInterval(1.5f);
		sequence.OnComplete(ShowEndScreen);
		sequence.Play();
	}

	private void SetEndScreenMessage(bool p_isWin)
	{
		string text = string.Empty;
		string empty = string.Empty;
		if (p_isWin)
		{
			string text2 = string.Empty;
			Character randomCharacterKilledByPlayer = CharacterManager.Instance.GetRandomCharacterKilledByPlayer();
			if (randomCharacterKilledByPlayer != null)
			{
				Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
				dictionary.Add("deathLog", randomCharacterKilledByPlayer.deathLog.logText);
				text2 = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "End_Game_Win_Message_Optional", dictionary);
			}
			text = ((!string.IsNullOrEmpty(text2)) ? (LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "End_Game_Win_Message_1") + " " + text2) : (LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "End_Game_Win_Message_1") ?? ""));
			empty = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "End_Game_Win_Message_2") ?? "";
		}
		else
		{
			empty = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "End_Game_Lose_Message") ?? "";
		}
		lblTopMessage.text = text;
		lblBottomMessage.text = empty;
		goTopMessage.SetActive(!string.IsNullOrEmpty(text));
	}

	private void UpdateEndScreenButtons(bool p_isWin)
	{
		btnContinue.gameObject.SetActive(p_isWin);
		if (p_isWin)
		{
			btnContinue.transform.SetAsLastSibling();
			lblContinueBtn.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "No");
			lblMainMenuBtn.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Yes") + " (" + LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Main_Menu") + ")";
		}
		else
		{
			btnContinue.transform.SetAsFirstSibling();
			lblContinueBtn.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Continue");
			lblMainMenuBtn.text = LocalizationManager.Instance.GetLocalizedValue("UIStrings_Table", "Main_Menu");
		}
	}

	private void ShowEndScreen()
	{
		InputManager.Instance.SetCursorTo(Cursor_Type.Default);
		endScreen.SetActive(value: true);
		endScreenCanvasGroup.alpha = 0f;
		topMessageCanvasGroup.alpha = 0f;
		bottomMessageCanvasGroup.alpha = 0f;
		UpdateEndScreenButtons(PlayerManager.Instance.player.hasAlreadyWon);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(endScreenCanvasGroup.DOFade(1f, 2f).SetEase(Ease.InQuint));
		sequence.Append(topMessageCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InQuint));
		sequence.Join(bottomMessageCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InQuint).SetDelay(0.2f));
		sequence.Play();
	}

	public void OnClickContinuePlaying()
	{
		GameManager.Instance.SetPausedState(isPaused: false);
		UIManager.Instance.SetSpeedTogglesState(state: true);
		DOTween.Clear(destroy: true);
		HideScreens();
	}

	private void HideScreens()
	{
		summaryScreen.gameObject.SetActive(value: false);
		endScreen.SetActive(value: false);
	}

	public void OnClickReturnToMainMenu()
	{
		DOTween.Clear(destroy: true);
		LevelLoaderManager.Instance.UpdateLoadingInfo(string.Empty);
		LevelLoaderManager.Instance.LoadLevel("MainMenu");
	}

	public void OnClickWishList()
	{
		Application.OpenURL("https://store.steampowered.com/app/909320/Ruinarch/");
	}

	public void OnClickLeaveFeedback()
	{
		Application.OpenURL("https://forms.gle/6QYHiSmU8ySVGSXp7");
	}

	public void OnClickJoinDiscord()
	{
		Application.OpenURL(GameUtilities.Discord_Link);
	}
}
