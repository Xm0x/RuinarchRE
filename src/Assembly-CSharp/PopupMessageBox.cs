using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UtilityScripts;

public class PopupMessageBox : InfoUIBase
{
	public static PopupMessageBox Instance;

	[SerializeField]
	private EnvelopContentUnityUI envelopContent;

	[SerializeField]
	private TextMeshProUGUI messageLbl;

	[SerializeField]
	private EasyTween tweener;

	private void Awake()
	{
		Instance = this;
	}

	internal override void Initialize()
	{
		Messenger.AddListener<string, bool>(UISignals.SHOW_POPUP_MESSAGE, ShowMessage);
		Messenger.AddListener(UISignals.HIDE_POPUP_MESSAGE, HideMessage);
	}

	private void ShowMessage(string message, bool autoHide)
	{
		messageLbl.text = message;
		envelopContent.Execute();
		if (!isShowing)
		{
			isShowing = true;
			tweener.TriggerOpenClose();
			if (autoHide)
			{
				StartCoroutine(AutoHideAfterSeconds());
			}
		}
	}

	private IEnumerator AutoHideAfterSeconds()
	{
		yield return GameUtilities.waitFor2Seconds;
		HideMessage();
	}

	public void SetYesAction(UnityAction yesAction)
	{
		AddYesAction(yesAction);
	}

	public void AddYesAction(UnityAction yesAction)
	{
	}

	public void SetNoAction(UnityAction noAction)
	{
		AddNoAction(noAction);
	}

	public void AddNoAction(UnityAction noAction)
	{
	}

	private void HideMessage()
	{
		isShowing = false;
		tweener.TriggerOpenClose();
	}
}
