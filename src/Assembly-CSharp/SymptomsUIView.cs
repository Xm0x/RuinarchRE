using System;
using Ruinarch.MVCFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class SymptomsUIView : MVCUIView
{
	public interface IListener
	{
		void OnParalysisUpgradeClicked();

		void OnVomitingUpgradeClicked();

		void OnLethargyUpgradeClicked();

		void OnSeizuresUpgradeClicked();

		void OnInsomniaUpgradeClicked();

		void OnPoisonCloudUpgradeClicked();

		void OnMonsterScentUpgradeClicked();

		void OnSneezingUpgradeClicked();

		void OnDepressionUpgradeClicked();

		void OnHungerPangsUpgradeClicked();

		void OnHoverOverParalysis(UIHoverPosition p_hoverPosition);

		void OnHoverOverVomiting(UIHoverPosition p_hoverPosition);

		void OnHoverOverLethargy(UIHoverPosition p_hoverPosition);

		void OnHoverOverSeizures(UIHoverPosition p_hoverPosition);

		void OnHoverOverInsomnia(UIHoverPosition p_hoverPosition);

		void OnHoverOverPoisonCloud(UIHoverPosition p_hoverPosition);

		void OnHoverOverMonsterScent(UIHoverPosition p_hoverPosition);

		void OnHoverOverSneezing(UIHoverPosition p_hoverPosition);

		void OnHoverOverDepression(UIHoverPosition p_hoverPosition);

		void OnHoverOverHungerPangs(UIHoverPosition p_hoverPosition);

		void OnHoverOutParalysis();

		void OnHoverOutVomiting();

		void OnHoverOutLethargy();

		void OnHoverOutSeizures();

		void OnHoverOutInsomnia();

		void OnHoverOutPoisonCloud();

		void OnHoverOutMonsterScent();

		void OnHoverOutSneezing();

		void OnHoverOutDepression();

		void OnHoverOutHungerPangs();
	}

	public SymptomsUIModel UIModel => _baseAssetModel as SymptomsUIModel;

	public static void Create(Canvas p_canvas, SymptomsUIModel p_assets, Action<SymptomsUIView> p_onCreate)
	{
		SymptomsUIView symptomsUIView = new GameObject(typeof(SymptomsUIView).ToString()).AddComponent<SymptomsUIView>();
		SymptomsUIModel assets = UnityEngine.Object.Instantiate(p_assets);
		symptomsUIView.Init(p_canvas, assets);
		p_onCreate?.Invoke(symptomsUIView);
	}

	public void Subscribe(IListener p_listener)
	{
		SymptomsUIModel uIModel = UIModel;
		uIModel.onParalysisUpgradeClicked = (Action)Delegate.Combine(uIModel.onParalysisUpgradeClicked, new Action(p_listener.OnParalysisUpgradeClicked));
		SymptomsUIModel uIModel2 = UIModel;
		uIModel2.onVomitingUpgradeClicked = (Action)Delegate.Combine(uIModel2.onVomitingUpgradeClicked, new Action(p_listener.OnVomitingUpgradeClicked));
		SymptomsUIModel uIModel3 = UIModel;
		uIModel3.onLethargyUpgradeClicked = (Action)Delegate.Combine(uIModel3.onLethargyUpgradeClicked, new Action(p_listener.OnLethargyUpgradeClicked));
		SymptomsUIModel uIModel4 = UIModel;
		uIModel4.onSeizuresUpgradeClicked = (Action)Delegate.Combine(uIModel4.onSeizuresUpgradeClicked, new Action(p_listener.OnSeizuresUpgradeClicked));
		SymptomsUIModel uIModel5 = UIModel;
		uIModel5.onInsomniaUpgradeClicked = (Action)Delegate.Combine(uIModel5.onInsomniaUpgradeClicked, new Action(p_listener.OnInsomniaUpgradeClicked));
		SymptomsUIModel uIModel6 = UIModel;
		uIModel6.onPoisonCloudUpgradeClicked = (Action)Delegate.Combine(uIModel6.onPoisonCloudUpgradeClicked, new Action(p_listener.OnPoisonCloudUpgradeClicked));
		SymptomsUIModel uIModel7 = UIModel;
		uIModel7.onMonsterScentUpgradeClicked = (Action)Delegate.Combine(uIModel7.onMonsterScentUpgradeClicked, new Action(p_listener.OnMonsterScentUpgradeClicked));
		SymptomsUIModel uIModel8 = UIModel;
		uIModel8.onSneezingUpgradeClicked = (Action)Delegate.Combine(uIModel8.onSneezingUpgradeClicked, new Action(p_listener.OnSneezingUpgradeClicked));
		SymptomsUIModel uIModel9 = UIModel;
		uIModel9.onDepressionUpgradeClicked = (Action)Delegate.Combine(uIModel9.onDepressionUpgradeClicked, new Action(p_listener.OnDepressionUpgradeClicked));
		SymptomsUIModel uIModel10 = UIModel;
		uIModel10.onHungerPangsUpgradeClicked = (Action)Delegate.Combine(uIModel10.onHungerPangsUpgradeClicked, new Action(p_listener.OnHungerPangsUpgradeClicked));
		SymptomsUIModel uIModel11 = UIModel;
		uIModel11.onParalysisHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel11.onParalysisHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverParalysis));
		SymptomsUIModel uIModel12 = UIModel;
		uIModel12.onVomitingHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel12.onVomitingHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverVomiting));
		SymptomsUIModel uIModel13 = UIModel;
		uIModel13.onLethargyHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel13.onLethargyHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverLethargy));
		SymptomsUIModel uIModel14 = UIModel;
		uIModel14.onSeizuresHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel14.onSeizuresHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverSeizures));
		SymptomsUIModel uIModel15 = UIModel;
		uIModel15.onInsomniaHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel15.onInsomniaHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverInsomnia));
		SymptomsUIModel uIModel16 = UIModel;
		uIModel16.onPoisonCloudHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel16.onPoisonCloudHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverPoisonCloud));
		SymptomsUIModel uIModel17 = UIModel;
		uIModel17.onMonsterScentHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel17.onMonsterScentHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverMonsterScent));
		SymptomsUIModel uIModel18 = UIModel;
		uIModel18.onSneezingHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel18.onSneezingHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverSneezing));
		SymptomsUIModel uIModel19 = UIModel;
		uIModel19.onDepressionHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel19.onDepressionHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverDepression));
		SymptomsUIModel uIModel20 = UIModel;
		uIModel20.onHungerPangsHoveredOver = (Action<UIHoverPosition>)Delegate.Combine(uIModel20.onHungerPangsHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverHungerPangs));
		SymptomsUIModel uIModel21 = UIModel;
		uIModel21.onParalysisHoveredOut = (Action)Delegate.Combine(uIModel21.onParalysisHoveredOut, new Action(p_listener.OnHoverOutParalysis));
		SymptomsUIModel uIModel22 = UIModel;
		uIModel22.onVomitingHoveredOut = (Action)Delegate.Combine(uIModel22.onVomitingHoveredOut, new Action(p_listener.OnHoverOutVomiting));
		SymptomsUIModel uIModel23 = UIModel;
		uIModel23.onLethargyHoveredOut = (Action)Delegate.Combine(uIModel23.onLethargyHoveredOut, new Action(p_listener.OnHoverOutLethargy));
		SymptomsUIModel uIModel24 = UIModel;
		uIModel24.onSeizuresHoveredOut = (Action)Delegate.Combine(uIModel24.onSeizuresHoveredOut, new Action(p_listener.OnHoverOutSeizures));
		SymptomsUIModel uIModel25 = UIModel;
		uIModel25.onInsomniaHoveredOut = (Action)Delegate.Combine(uIModel25.onInsomniaHoveredOut, new Action(p_listener.OnHoverOutInsomnia));
		SymptomsUIModel uIModel26 = UIModel;
		uIModel26.onPoisonCloudHoveredOut = (Action)Delegate.Combine(uIModel26.onPoisonCloudHoveredOut, new Action(p_listener.OnHoverOutPoisonCloud));
		SymptomsUIModel uIModel27 = UIModel;
		uIModel27.onMonsterScentHoveredOut = (Action)Delegate.Combine(uIModel27.onMonsterScentHoveredOut, new Action(p_listener.OnHoverOutMonsterScent));
		SymptomsUIModel uIModel28 = UIModel;
		uIModel28.onSneezingHoveredOut = (Action)Delegate.Combine(uIModel28.onSneezingHoveredOut, new Action(p_listener.OnHoverOutSneezing));
		SymptomsUIModel uIModel29 = UIModel;
		uIModel29.onDepressionHoveredOut = (Action)Delegate.Combine(uIModel29.onDepressionHoveredOut, new Action(p_listener.OnHoverOutDepression));
		SymptomsUIModel uIModel30 = UIModel;
		uIModel30.onHungerPangsHoveredOut = (Action)Delegate.Combine(uIModel30.onHungerPangsHoveredOut, new Action(p_listener.OnHoverOutHungerPangs));
	}

	public void Unsubscribe(IListener p_listener)
	{
		SymptomsUIModel uIModel = UIModel;
		uIModel.onParalysisUpgradeClicked = (Action)Delegate.Remove(uIModel.onParalysisUpgradeClicked, new Action(p_listener.OnParalysisUpgradeClicked));
		SymptomsUIModel uIModel2 = UIModel;
		uIModel2.onVomitingUpgradeClicked = (Action)Delegate.Remove(uIModel2.onVomitingUpgradeClicked, new Action(p_listener.OnVomitingUpgradeClicked));
		SymptomsUIModel uIModel3 = UIModel;
		uIModel3.onLethargyUpgradeClicked = (Action)Delegate.Remove(uIModel3.onLethargyUpgradeClicked, new Action(p_listener.OnLethargyUpgradeClicked));
		SymptomsUIModel uIModel4 = UIModel;
		uIModel4.onSeizuresUpgradeClicked = (Action)Delegate.Remove(uIModel4.onSeizuresUpgradeClicked, new Action(p_listener.OnSeizuresUpgradeClicked));
		SymptomsUIModel uIModel5 = UIModel;
		uIModel5.onInsomniaUpgradeClicked = (Action)Delegate.Remove(uIModel5.onInsomniaUpgradeClicked, new Action(p_listener.OnInsomniaUpgradeClicked));
		SymptomsUIModel uIModel6 = UIModel;
		uIModel6.onPoisonCloudUpgradeClicked = (Action)Delegate.Remove(uIModel6.onPoisonCloudUpgradeClicked, new Action(p_listener.OnPoisonCloudUpgradeClicked));
		SymptomsUIModel uIModel7 = UIModel;
		uIModel7.onMonsterScentUpgradeClicked = (Action)Delegate.Remove(uIModel7.onMonsterScentUpgradeClicked, new Action(p_listener.OnMonsterScentUpgradeClicked));
		SymptomsUIModel uIModel8 = UIModel;
		uIModel8.onSneezingUpgradeClicked = (Action)Delegate.Remove(uIModel8.onSneezingUpgradeClicked, new Action(p_listener.OnSneezingUpgradeClicked));
		SymptomsUIModel uIModel9 = UIModel;
		uIModel9.onDepressionUpgradeClicked = (Action)Delegate.Remove(uIModel9.onDepressionUpgradeClicked, new Action(p_listener.OnDepressionUpgradeClicked));
		SymptomsUIModel uIModel10 = UIModel;
		uIModel10.onHungerPangsUpgradeClicked = (Action)Delegate.Remove(uIModel10.onHungerPangsUpgradeClicked, new Action(p_listener.OnHungerPangsUpgradeClicked));
		SymptomsUIModel uIModel11 = UIModel;
		uIModel11.onParalysisHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel11.onParalysisHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverParalysis));
		SymptomsUIModel uIModel12 = UIModel;
		uIModel12.onVomitingHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel12.onVomitingHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverVomiting));
		SymptomsUIModel uIModel13 = UIModel;
		uIModel13.onLethargyHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel13.onLethargyHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverLethargy));
		SymptomsUIModel uIModel14 = UIModel;
		uIModel14.onSeizuresHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel14.onSeizuresHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverSeizures));
		SymptomsUIModel uIModel15 = UIModel;
		uIModel15.onInsomniaHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel15.onInsomniaHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverInsomnia));
		SymptomsUIModel uIModel16 = UIModel;
		uIModel16.onPoisonCloudHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel16.onPoisonCloudHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverPoisonCloud));
		SymptomsUIModel uIModel17 = UIModel;
		uIModel17.onMonsterScentHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel17.onMonsterScentHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverMonsterScent));
		SymptomsUIModel uIModel18 = UIModel;
		uIModel18.onSneezingHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel18.onSneezingHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverSneezing));
		SymptomsUIModel uIModel19 = UIModel;
		uIModel19.onDepressionHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel19.onDepressionHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverDepression));
		SymptomsUIModel uIModel20 = UIModel;
		uIModel20.onHungerPangsHoveredOver = (Action<UIHoverPosition>)Delegate.Remove(uIModel20.onHungerPangsHoveredOver, new Action<UIHoverPosition>(p_listener.OnHoverOverHungerPangs));
		SymptomsUIModel uIModel21 = UIModel;
		uIModel21.onParalysisHoveredOut = (Action)Delegate.Remove(uIModel21.onParalysisHoveredOut, new Action(p_listener.OnHoverOutParalysis));
		SymptomsUIModel uIModel22 = UIModel;
		uIModel22.onVomitingHoveredOut = (Action)Delegate.Remove(uIModel22.onVomitingHoveredOut, new Action(p_listener.OnHoverOutVomiting));
		SymptomsUIModel uIModel23 = UIModel;
		uIModel23.onLethargyHoveredOut = (Action)Delegate.Remove(uIModel23.onLethargyHoveredOut, new Action(p_listener.OnHoverOutLethargy));
		SymptomsUIModel uIModel24 = UIModel;
		uIModel24.onSeizuresHoveredOut = (Action)Delegate.Remove(uIModel24.onSeizuresHoveredOut, new Action(p_listener.OnHoverOutSeizures));
		SymptomsUIModel uIModel25 = UIModel;
		uIModel25.onInsomniaHoveredOut = (Action)Delegate.Remove(uIModel25.onInsomniaHoveredOut, new Action(p_listener.OnHoverOutInsomnia));
		SymptomsUIModel uIModel26 = UIModel;
		uIModel26.onPoisonCloudHoveredOut = (Action)Delegate.Remove(uIModel26.onPoisonCloudHoveredOut, new Action(p_listener.OnHoverOutPoisonCloud));
		SymptomsUIModel uIModel27 = UIModel;
		uIModel27.onMonsterScentHoveredOut = (Action)Delegate.Remove(uIModel27.onMonsterScentHoveredOut, new Action(p_listener.OnHoverOutMonsterScent));
		SymptomsUIModel uIModel28 = UIModel;
		uIModel28.onSneezingHoveredOut = (Action)Delegate.Remove(uIModel28.onSneezingHoveredOut, new Action(p_listener.OnHoverOutSneezing));
		SymptomsUIModel uIModel29 = UIModel;
		uIModel29.onDepressionHoveredOut = (Action)Delegate.Remove(uIModel29.onDepressionHoveredOut, new Action(p_listener.OnHoverOutDepression));
		SymptomsUIModel uIModel30 = UIModel;
		uIModel30.onHungerPangsHoveredOut = (Action)Delegate.Remove(uIModel30.onHungerPangsHoveredOut, new Action(p_listener.OnHoverOutHungerPangs));
	}

	private RuinarchText GetCostTextToUpdate(PLAGUE_SYMPTOM p_symptomType)
	{
		return p_symptomType switch
		{
			PLAGUE_SYMPTOM.Paralysis => UIModel.txtParalysisCost, 
			PLAGUE_SYMPTOM.Vomiting => UIModel.txtVomitingCost, 
			PLAGUE_SYMPTOM.Lethargy => UIModel.txtLethargyCost, 
			PLAGUE_SYMPTOM.Seizure => UIModel.txtSeizuresCost, 
			PLAGUE_SYMPTOM.Insomnia => UIModel.txtInsomniaCost, 
			PLAGUE_SYMPTOM.Poison_Cloud => UIModel.txtPoisonCloudCost, 
			PLAGUE_SYMPTOM.Monster_Scent => UIModel.txtMonsterScentCost, 
			PLAGUE_SYMPTOM.Sneezing => UIModel.txtSneezingCost, 
			PLAGUE_SYMPTOM.Depression => UIModel.txtDepressionCost, 
			PLAGUE_SYMPTOM.Hunger_Pangs => UIModel.txtHungerCost, 
			_ => throw new ArgumentOutOfRangeException("p_symptomType", p_symptomType, null), 
		};
	}

	private Button GetSymptomUpgradeBtn(PLAGUE_SYMPTOM p_symptomType)
	{
		return p_symptomType switch
		{
			PLAGUE_SYMPTOM.Paralysis => UIModel.btnParalysisUpgrade, 
			PLAGUE_SYMPTOM.Vomiting => UIModel.btnVomitingUpgrade, 
			PLAGUE_SYMPTOM.Lethargy => UIModel.btnLethargyUpgrade, 
			PLAGUE_SYMPTOM.Seizure => UIModel.btnSeizuresUpgrade, 
			PLAGUE_SYMPTOM.Insomnia => UIModel.btnInsomniaUpgrade, 
			PLAGUE_SYMPTOM.Poison_Cloud => UIModel.btnPoisonCloudUpgrade, 
			PLAGUE_SYMPTOM.Monster_Scent => UIModel.btnMonsterScenetUpgrade, 
			PLAGUE_SYMPTOM.Sneezing => UIModel.btnSneezingUpgrade, 
			PLAGUE_SYMPTOM.Depression => UIModel.btnDepressionUpgrade, 
			PLAGUE_SYMPTOM.Hunger_Pangs => UIModel.btnHungerUpgrade, 
			_ => throw new ArgumentOutOfRangeException("p_symptomType", p_symptomType, null), 
		};
	}

	private TextMeshProUGUI GetSymptomUpgradeBtnText(PLAGUE_SYMPTOM p_symptomType)
	{
		return p_symptomType switch
		{
			PLAGUE_SYMPTOM.Paralysis => UIModel.txtParalysisUpgrade, 
			PLAGUE_SYMPTOM.Vomiting => UIModel.txtVomitingUpgrade, 
			PLAGUE_SYMPTOM.Lethargy => UIModel.txtLethargyUpgrade, 
			PLAGUE_SYMPTOM.Seizure => UIModel.txtSeizuresUpgrade, 
			PLAGUE_SYMPTOM.Insomnia => UIModel.txtInsomniaUpgrade, 
			PLAGUE_SYMPTOM.Poison_Cloud => UIModel.txtPoisonCloudUpgrade, 
			PLAGUE_SYMPTOM.Monster_Scent => UIModel.txtMonsterScenetUpgrade, 
			PLAGUE_SYMPTOM.Sneezing => UIModel.txtSneezingUpgrade, 
			PLAGUE_SYMPTOM.Depression => UIModel.txtDepressionUpgrade, 
			PLAGUE_SYMPTOM.Hunger_Pangs => UIModel.txtHungerUpgrade, 
			_ => throw new ArgumentOutOfRangeException("p_symptomType", p_symptomType, null), 
		};
	}

	private GameObject GetSymptomUpgradeCheckmark(PLAGUE_SYMPTOM p_symptomType)
	{
		return p_symptomType switch
		{
			PLAGUE_SYMPTOM.Paralysis => UIModel.checkMarkParalysisUpgrade, 
			PLAGUE_SYMPTOM.Vomiting => UIModel.checkMarkVomitingUpgrade, 
			PLAGUE_SYMPTOM.Lethargy => UIModel.checkMarkLethargyUpgrade, 
			PLAGUE_SYMPTOM.Seizure => UIModel.checkMarkSeizuresUpgrade, 
			PLAGUE_SYMPTOM.Insomnia => UIModel.checkMarkInsomniaUpgrade, 
			PLAGUE_SYMPTOM.Poison_Cloud => UIModel.checkMarkPoisonCloudUpgrade, 
			PLAGUE_SYMPTOM.Monster_Scent => UIModel.checkMarkMonsterScenetUpgrade, 
			PLAGUE_SYMPTOM.Sneezing => UIModel.checkMarkSneezingUpgrade, 
			PLAGUE_SYMPTOM.Depression => UIModel.checkMarkDepressionUpgrade, 
			PLAGUE_SYMPTOM.Hunger_Pangs => UIModel.checkMarkHungerUpgrade, 
			_ => throw new ArgumentOutOfRangeException("p_symptomType", p_symptomType, null), 
		};
	}

	public void UpdateSymptomCost(PLAGUE_SYMPTOM p_symptom, string p_cost)
	{
		GetCostTextToUpdate(p_symptom).text = p_cost;
	}

	public void UpdateSymptomCostState(PLAGUE_SYMPTOM p_symptom, bool p_state)
	{
		GetCostTextToUpdate(p_symptom).gameObject.SetActive(p_state);
	}

	public void UpdateSymptomUpgradeButtonInteractable(PLAGUE_SYMPTOM p_symptom, bool p_interactable)
	{
		GetSymptomUpgradeBtn(p_symptom).interactable = p_interactable;
		GetSymptomUpgradeBtnText(p_symptom).color = GameUtilities.GetUpgradeButtonTextColor(p_interactable);
	}

	public void UpdateSymptomCheckmarkState(PLAGUE_SYMPTOM p_symptom, bool p_state)
	{
		GetSymptomUpgradeCheckmark(p_symptom).SetActive(p_state);
	}
}
