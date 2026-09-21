using System;
using Ruinarch.MVCFramework;
using UnityEngine;

public class SaveWindowUIView : MVCUIView
{
	public interface IListener
	{
		void OnClickClose();

		void OnClickSaveLoad();

		void OnClickCreateNewSave();

		void OnConfirmNewSaveFileName();

		void OnNewSaveFileNameValueChanged(string p_value);

		void OnSubmitNewSaveFileName(string p_value);

		void OnClickConfirmRenameSaveFile();

		void OnSubmitRenameSaveFile();
	}

	public SaveWindowUIModel UIModel => _baseAssetModel as SaveWindowUIModel;

	public static void Create(Canvas p_canvas, SaveWindowUIModel p_assets, Action<SaveWindowUIView> p_onCreate)
	{
		SaveWindowUIView saveWindowUIView = new GameObject(typeof(SaveWindowUIView).ToString()).AddComponent<SaveWindowUIView>();
		SaveWindowUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		saveWindowUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(saveWindowUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		SaveWindowUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Combine(uIModel.onClickClose, new Action(p_listener.OnClickClose));
		SaveWindowUIModel uIModel2 = UIModel;
		uIModel2.OnClickSaveLoad = (Action)Delegate.Combine(uIModel2.OnClickSaveLoad, new Action(p_listener.OnClickSaveLoad));
		SaveWindowUIModel uIModel3 = UIModel;
		uIModel3.onClickCreateNewSave = (Action)Delegate.Combine(uIModel3.onClickCreateNewSave, new Action(p_listener.OnClickCreateNewSave));
		SaveWindowUIModel uIModel4 = UIModel;
		uIModel4.onClickConfirmCreateNewSave = (Action)Delegate.Combine(uIModel4.onClickConfirmCreateNewSave, new Action(p_listener.OnConfirmNewSaveFileName));
		SaveWindowUIModel uIModel5 = UIModel;
		uIModel5.onValueChangedCreateNewSave = (Action<string>)Delegate.Combine(uIModel5.onValueChangedCreateNewSave, new Action<string>(p_listener.OnNewSaveFileNameValueChanged));
		SaveWindowUIModel uIModel6 = UIModel;
		uIModel6.onSubmitCreateNewSave = (Action<string>)Delegate.Combine(uIModel6.onSubmitCreateNewSave, new Action<string>(p_listener.OnSubmitNewSaveFileName));
		SaveWindowUIModel uIModel7 = UIModel;
		uIModel7.onClickConfirmRenameSave = (Action)Delegate.Combine(uIModel7.onClickConfirmRenameSave, new Action(p_listener.OnClickConfirmRenameSaveFile));
		SaveWindowUIModel uIModel8 = UIModel;
		uIModel8.onSubmitRenameSave = (Action)Delegate.Combine(uIModel8.onSubmitRenameSave, new Action(p_listener.OnSubmitRenameSaveFile));
	}

	public void Unsubscribe(IListener p_listener)
	{
		SaveWindowUIModel uIModel = UIModel;
		uIModel.onClickClose = (Action)Delegate.Remove(uIModel.onClickClose, new Action(p_listener.OnClickClose));
		SaveWindowUIModel uIModel2 = UIModel;
		uIModel2.OnClickSaveLoad = (Action)Delegate.Remove(uIModel2.OnClickSaveLoad, new Action(p_listener.OnClickSaveLoad));
		SaveWindowUIModel uIModel3 = UIModel;
		uIModel3.onClickCreateNewSave = (Action)Delegate.Remove(uIModel3.onClickCreateNewSave, new Action(p_listener.OnClickCreateNewSave));
		SaveWindowUIModel uIModel4 = UIModel;
		uIModel4.onClickConfirmCreateNewSave = (Action)Delegate.Remove(uIModel4.onClickConfirmCreateNewSave, new Action(p_listener.OnConfirmNewSaveFileName));
		SaveWindowUIModel uIModel5 = UIModel;
		uIModel5.onValueChangedCreateNewSave = (Action<string>)Delegate.Remove(uIModel5.onValueChangedCreateNewSave, new Action<string>(p_listener.OnNewSaveFileNameValueChanged));
		SaveWindowUIModel uIModel6 = UIModel;
		uIModel6.onSubmitCreateNewSave = (Action<string>)Delegate.Remove(uIModel6.onSubmitCreateNewSave, new Action<string>(p_listener.OnSubmitNewSaveFileName));
		SaveWindowUIModel uIModel7 = UIModel;
		uIModel7.onClickConfirmRenameSave = (Action)Delegate.Remove(uIModel7.onClickConfirmRenameSave, new Action(p_listener.OnClickConfirmRenameSaveFile));
		SaveWindowUIModel uIModel8 = UIModel;
		uIModel8.onSubmitRenameSave = (Action)Delegate.Remove(uIModel8.onSubmitRenameSave, new Action(p_listener.OnSubmitRenameSaveFile));
	}

	public void SetTitle(string p_title)
	{
		UIModel.titleLbl.text = p_title;
	}

	public void SetSaveLoadBtnText(string p_title)
	{
		UIModel.saveLoadLbl.text = p_title;
	}

	public void SetSaveNameWithoutNotify(string p_name)
	{
		UIModel.saveNameInputField.SetTextWithoutNotify(p_name);
	}

	public void SetSaveInfo(string p_info)
	{
		UIModel.saveInfoLbl.text = p_info;
	}

	public void SetSaveScreenshot(Texture2D p_texture2D)
	{
		UIModel.saveScreenshotImage.texture = p_texture2D;
		UIModel.saveScreenshotImage.SetNativeSize();
	}

	public void SetScreenshotState(bool p_state)
	{
		UIModel.saveScreenshotImage.gameObject.SetActive(p_state);
	}

	public void SetSaveErrorMessage(string p_message)
	{
		UIModel.saveErrorLbl.text = p_message;
		UIModel.saveErrorGO.SetActive(!string.IsNullOrEmpty(p_message));
	}
}
