using Ruinarch.MVCFramework;
using UnityEngine;

public class SubGoalsUIController : MVCUIController, SubGoalsUIView.IListener
{
	[SerializeField]
	private SubGoalsUIModel m_goalsUIModel;

	private SubGoalsUIView m_goalsUIView;

	public bool isShowing;

	public Transform parent;

	[ContextMenu("Instantiate UI")]
	public override void InstantiateUI()
	{
		SubGoalsUIView.Create(_canvas, m_goalsUIModel, delegate(SubGoalsUIView p_ui)
		{
			m_goalsUIView = p_ui;
			m_goalsUIView.Subscribe(this);
			InitUI(p_ui.UIModel, p_ui);
			p_ui.UIModel.transform.SetParent(parent);
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
	}

	public override void HideUI()
	{
		base.HideUI();
		isShowing = false;
		PlayerUI.Instance.OnCloseSubGoalsUI();
	}

	public void OnClickClose()
	{
		HideUI();
	}

	public void HideViaShortcutKey()
	{
		HideUI();
	}

	public void InitializeSubGoals(SubGoal[] p_subGoals)
	{
		SubGoal[] array = new SubGoal[p_subGoals.Length];
		foreach (SubGoal subGoal in p_subGoals)
		{
			array[subGoal.GetSideGoalIndex()] = subGoal;
		}
		foreach (SubGoal p_subGoal in array)
		{
			ObjectPoolManager.Instance.InstantiateObjectFromPool(m_goalsUIView.UIModel.prefabSubGoalItem.name, Vector3.zero, Quaternion.identity, m_goalsUIView.UIModel.scrollRectSubGoals.content).GetComponent<SubGoalItem>().Initialize(p_subGoal);
		}
	}
}
