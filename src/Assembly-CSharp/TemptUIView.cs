using System;
using System.Collections.Generic;
using Ruinarch.Custom_UI;
using Ruinarch.MVCFramework;
using UnityEngine;
using UtilityScripts;

public class TemptUIView : MVCUIView
{
	public interface IListener
	{
		void OnToggleDarkBlessing(bool p_isOn);

		void OnToggleEmpower(bool p_isOn);

		void OnToggleCleanseFlaws(bool p_isOn);

		void OnHoverDarkBlessing();

		void OnHoverEmpower();

		void OnHoverCleanseFlaws();

		void OnHoverOutTemptation();

		void OnClickClose();

		void OnClickConfirm();
	}

	[NonSerialized]
	public TEMPTATION[] allTemptationTypes;

	public TemptUIModel UIModel => _baseAssetModel as TemptUIModel;

	public static void Create(Canvas p_canvas, TemptUIModel p_assets, Action<TemptUIView> p_onCreate)
	{
		TemptUIView temptUIView = new GameObject(typeof(TemptUIView).ToString()).AddComponent<TemptUIView>();
		TemptUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		temptUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(temptUIView);
	}

	private void Awake()
	{
		allTemptationTypes = CollectionUtilities.GetEnumValues<TEMPTATION>();
	}

	public void Subscribe(IListener p_listener)
	{
		TemptUIModel uIModel = UIModel;
		uIModel.onToggleDarkBlessing = (Action<bool>)Delegate.Combine(uIModel.onToggleDarkBlessing, new Action<bool>(p_listener.OnToggleDarkBlessing));
		TemptUIModel uIModel2 = UIModel;
		uIModel2.onToggleEmpower = (Action<bool>)Delegate.Combine(uIModel2.onToggleEmpower, new Action<bool>(p_listener.OnToggleEmpower));
		TemptUIModel uIModel3 = UIModel;
		uIModel3.onToggleCleanseFlaws = (Action<bool>)Delegate.Combine(uIModel3.onToggleCleanseFlaws, new Action<bool>(p_listener.OnToggleCleanseFlaws));
		TemptUIModel uIModel4 = UIModel;
		uIModel4.onHoverDarkBlessing = (Action)Delegate.Combine(uIModel4.onHoverDarkBlessing, new Action(p_listener.OnHoverDarkBlessing));
		TemptUIModel uIModel5 = UIModel;
		uIModel5.onHoverEmpower = (Action)Delegate.Combine(uIModel5.onHoverEmpower, new Action(p_listener.OnHoverEmpower));
		TemptUIModel uIModel6 = UIModel;
		uIModel6.onHoverCleanseFlaws = (Action)Delegate.Combine(uIModel6.onHoverCleanseFlaws, new Action(p_listener.OnHoverCleanseFlaws));
		TemptUIModel uIModel7 = UIModel;
		uIModel7.onHoverOutTemptation = (Action)Delegate.Combine(uIModel7.onHoverOutTemptation, new Action(p_listener.OnHoverOutTemptation));
		TemptUIModel uIModel8 = UIModel;
		uIModel8.onClickClose = (Action)Delegate.Combine(uIModel8.onClickClose, new Action(p_listener.OnClickClose));
		TemptUIModel uIModel9 = UIModel;
		uIModel9.onClickConfirm = (Action)Delegate.Combine(uIModel9.onClickConfirm, new Action(p_listener.OnClickConfirm));
	}

	public void Unsubscribe(IListener p_listener)
	{
		TemptUIModel uIModel = UIModel;
		uIModel.onToggleDarkBlessing = (Action<bool>)Delegate.Remove(uIModel.onToggleDarkBlessing, new Action<bool>(p_listener.OnToggleDarkBlessing));
		TemptUIModel uIModel2 = UIModel;
		uIModel2.onToggleEmpower = (Action<bool>)Delegate.Remove(uIModel2.onToggleEmpower, new Action<bool>(p_listener.OnToggleEmpower));
		TemptUIModel uIModel3 = UIModel;
		uIModel3.onToggleCleanseFlaws = (Action<bool>)Delegate.Remove(uIModel3.onToggleCleanseFlaws, new Action<bool>(p_listener.OnToggleCleanseFlaws));
		TemptUIModel uIModel4 = UIModel;
		uIModel4.onHoverDarkBlessing = (Action)Delegate.Remove(uIModel4.onHoverDarkBlessing, new Action(p_listener.OnHoverDarkBlessing));
		TemptUIModel uIModel5 = UIModel;
		uIModel5.onHoverEmpower = (Action)Delegate.Remove(uIModel5.onHoverEmpower, new Action(p_listener.OnHoverEmpower));
		TemptUIModel uIModel6 = UIModel;
		uIModel6.onHoverCleanseFlaws = (Action)Delegate.Remove(uIModel6.onHoverCleanseFlaws, new Action(p_listener.OnHoverCleanseFlaws));
		TemptUIModel uIModel7 = UIModel;
		uIModel7.onHoverOutTemptation = (Action)Delegate.Remove(uIModel7.onHoverOutTemptation, new Action(p_listener.OnHoverOutTemptation));
		TemptUIModel uIModel8 = UIModel;
		uIModel8.onClickClose = (Action)Delegate.Remove(uIModel8.onClickClose, new Action(p_listener.OnClickClose));
		TemptUIModel uIModel9 = UIModel;
		uIModel9.onClickConfirm = (Action)Delegate.Remove(uIModel9.onClickConfirm, new Action(p_listener.OnClickConfirm));
	}

	public void UpdateShownItems(Character p_target, List<TEMPTATION> p_alreadyChosenTemptations)
	{
		for (int i = 0; i < allTemptationTypes.Length; i++)
		{
			TEMPTATION tEMPTATION = allTemptationTypes[i];
			RuinarchToggle temptationToggle = GetTemptationToggle(tEMPTATION);
			GameObject temptationCover = GetTemptationCover(tEMPTATION);
			bool num = tEMPTATION.CanTemptCharacter(p_target);
			temptationToggle.SetIsOnWithoutNotify(value: false);
			if (num)
			{
				temptationToggle.gameObject.SetActive(value: true);
				temptationToggle.interactable = !p_alreadyChosenTemptations.Contains(tEMPTATION);
				temptationCover.SetActive(!temptationToggle.interactable);
			}
			else
			{
				temptationToggle.gameObject.SetActive(value: false);
			}
		}
	}

	private RuinarchToggle GetTemptationToggle(TEMPTATION p_temptationType)
	{
		return p_temptationType switch
		{
			TEMPTATION.Dark_Blessing => UIModel.tglDarkBlessing, 
			TEMPTATION.Empower => UIModel.tglEmpower, 
			TEMPTATION.Cleanse_Flaws => UIModel.tglCleanseFlaws, 
			_ => throw new ArgumentOutOfRangeException("p_temptationType", p_temptationType, null), 
		};
	}

	private GameObject GetTemptationCover(TEMPTATION p_temptationType)
	{
		return p_temptationType switch
		{
			TEMPTATION.Dark_Blessing => UIModel.coverDarkBlessing, 
			TEMPTATION.Empower => UIModel.coverEmpower, 
			TEMPTATION.Cleanse_Flaws => UIModel.coverCleanseFlaws, 
			_ => throw new ArgumentOutOfRangeException("p_temptationType", p_temptationType, null), 
		};
	}
}
