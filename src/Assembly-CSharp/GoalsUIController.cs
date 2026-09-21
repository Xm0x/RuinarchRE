using Quests;
using Ruinarch;
using Ruinarch.MVCFramework;
using UnityEngine;

public class GoalsUIController : MVCUIController, GoalsUIView.IListener
{
	[SerializeField]
	private GoalsUIModel m_goalsUIModel;

	private GoalsUIView m_goalsUIView;

	public bool isShowing;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		GoalsUIView.Create(_canvas, m_goalsUIModel, delegate(GoalsUIView p_ui)
		{
			m_goalsUIView = p_ui;
			m_goalsUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
		});
	}

	private void Start()
	{
		InstantiateUI();
		HideUI();
	}

	private void OnDestroy()
	{
		m_goalsUIView?.Unsubscribe(this);
	}

	public override void ShowUI()
	{
		base.ShowUI();
		isShowing = true;
		UIManager.Instance.Pause();
		UIManager.Instance.SetSpeedTogglesState(state: false);
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: false);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cancel, p_state: true);
		InputManager.Instance.SetSpecificHotkeyEnabledState(SHORTCUT_ACTION.Cycle_Top_Menus, p_state: true);
		InnerMapCameraMove.Instance.DisableMovement();
		UpdateInstructionText();
	}

	public override void HideUI()
	{
		base.HideUI();
		isShowing = false;
		PlayerUI.Instance.OnCloseGoalsUI();
		UIManager.Instance.ResumeLastProgressionSpeed();
		InputManager.Instance.SetAllHotkeysEnabledState(p_state: true);
		InnerMapCameraMove.Instance.EnableMovement();
	}

	public void Initialize(Goal[] p_goals)
	{
		foreach (Goal goal in p_goals)
		{
			if (goal != null)
			{
				ObjectPoolManager.Instance.InstantiateObjectFromPool(m_goalsUIView.UIModel.prefabGoalItem.name, Vector3.zero, Quaternion.identity, m_goalsUIView.UIModel.scrollRectGoals.content).GetComponent<GoalUIItem>().Initialize(goal);
			}
		}
		UpdateInstructionText();
	}

	private void UpdateInstructionText()
	{
		if (QuestManager.Instance.victoryCondition.type == VICTORY_CONDITION.Attainment)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "UI", "UIStrings_Table", "Goal_Task_Instruction");
			log.AddToFillers(null, 13.ToString(), LOG_IDENTIFIER.STRING_1);
			log.AddToFillers(null, (13 - PlayerManager.Instance.player.goalComponent.completedTasks).ToString(), LOG_IDENTIFIER.STRING_2);
			m_goalsUIView.UIModel.lblInstructions.text = log.logText;
		}
		else
		{
			m_goalsUIView.UIModel.lblInstructions.text = string.Empty;
		}
	}

	public void OnClickClose()
	{
		HideUI();
	}

	public void HideViaShortcutKey()
	{
		HideUI();
	}
}
