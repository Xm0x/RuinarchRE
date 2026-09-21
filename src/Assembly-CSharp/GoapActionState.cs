using System;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class GoapActionState
{
	public GoapAction parentAction { get; private set; }

	public string name { get; private set; }

	public int duration { get; private set; }

	public Action<ActualGoapNode> preEffect { get; private set; }

	public Action<ActualGoapNode> perTickEffect { get; private set; }

	public Action<ActualGoapNode> afterEffect { get; private set; }

	public string status { get; private set; }

	public string animationName { get; private set; }

	public GoapActionState(string name, GoapAction parentAction, Action<ActualGoapNode> preEffect, Action<ActualGoapNode> perTickEffect, Action<ActualGoapNode> afterEffect, int duration, string status, string animationName)
	{
		this.name = name;
		this.preEffect = preEffect;
		this.perTickEffect = perTickEffect;
		this.afterEffect = afterEffect;
		this.parentAction = parentAction;
		this.duration = duration;
		this.status = status;
		this.animationName = animationName;
	}

	public Log CreateDescriptionLog(ActualGoapNode goapNode)
	{
		string goapName = parentAction.goapName;
		string text = name.ToLower();
		string key = goapName + " " + text + "_description";
		if ((LocalizationSettings.SelectedLocale.LocaleName.Equals("Polish (pl)") || LocalizationSettings.SelectedLocale.LocaleName.Equals("English (en)")) && goapNode.action.goapType == INTERACTION_TYPE.EAT && goapNode.poiTarget is Table)
		{
			key = "Eat at table success description";
		}
		if (!string.IsNullOrEmpty(LocalizationManager.Instance.GetLocalizedValue("GoapActionsStrings_Table", key)))
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "GoapAction", "GoapActionsStrings_Table", key, goapNode.logTags);
			goapNode.action.AddFillersToLog(log, goapNode);
			return log;
		}
		Debug.LogWarning(name + " had problems creating it's description log");
		return null;
	}

	public override string ToString()
	{
		return $"{name} {parentAction}";
	}
}
