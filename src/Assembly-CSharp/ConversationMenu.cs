using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class ConversationMenu : PopupMenuBase
{
	[Header("Main")]
	[SerializeField]
	private ScrollRect dialogScrollView;

	[SerializeField]
	private GameObject dialogItemPrefab;

	[SerializeField]
	private Button closeBtn;

	[SerializeField]
	private TextMeshProUGUI instructionLbl;

	[SerializeField]
	private TextMeshProUGUI endOfConversationLbl;

	private bool wasPausedOnOpen;

	public void Open(List<ConversationData> conversationList, string titleText)
	{
		base.Open();
		wasPausedOnOpen = GameManager.Instance.isPaused;
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		Messenger.Broadcast(UISignals.ON_OPEN_CONVERSATION_MENU);
		instructionLbl.text = titleText;
		Utilities.DestroyChildren(dialogScrollView.content);
		if (conversationList != null)
		{
			for (int i = 0; i < conversationList.Count; i++)
			{
				ConversationData conversationData = conversationList[i];
				CreateDialogItem(conversationData.character, conversationData.text, conversationData.position);
			}
		}
		closeBtn.interactable = true;
		dialogScrollView.verticalNormalizedPosition = 1f;
	}

	public override void Close()
	{
		base.Close();
		UIManager.Instance.SetSpeedTogglesState(state: true);
		GameManager.Instance.SetPausedState(wasPausedOnOpen);
		Messenger.Broadcast(UISignals.ON_CLOSE_CONVERSATION_MENU);
	}

	private void CreateDialogItem(Character character, string reaction, DialogItem.Position position)
	{
		ObjectPoolManager.Instance.InstantiateObjectFromPool(dialogItemPrefab.name, Vector3.zero, Quaternion.identity, dialogScrollView.content).GetComponent<DialogItem>().SetData(character, reaction, position);
	}
}
