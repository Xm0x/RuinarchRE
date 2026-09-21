using System;
using UnityEngine;

public class GoapThread : Multithread
{
	public GoapPlan recalculationPlan;

	private Character owner;

	public Character actor { get; private set; }

	public GoapEffect goalEffect { get; private set; }

	public INTERACTION_TYPE goalType { get; private set; }

	public IPointOfInterest target { get; private set; }

	public bool isPersonalPlan { get; private set; }

	public GoapPlanJob job { get; private set; }

	public string log { get; private set; }

	public bool isRecalculationSuccess { get; private set; }

	public void Initialize(Character actor, IPointOfInterest target, GoapEffect goalEffect, bool isPersonalPlan, GoapPlanJob job)
	{
		recalculationPlan = null;
		this.actor = actor;
		this.target = target;
		this.goalEffect = goalEffect;
		this.isPersonalPlan = isPersonalPlan;
		this.job = job;
		owner = actor;
		isRecalculationSuccess = false;
	}

	public void Initialize(Character actor, INTERACTION_TYPE goalType, IPointOfInterest target, bool isPersonalPlan, GoapPlanJob job)
	{
		recalculationPlan = null;
		this.actor = actor;
		this.target = target;
		this.goalType = goalType;
		this.isPersonalPlan = isPersonalPlan;
		this.job = job;
		owner = actor;
		isRecalculationSuccess = false;
	}

	public void InitializeForRecalculation(Character actor, GoapPlan currentPlan, GoapPlanJob job)
	{
		this.actor = actor;
		recalculationPlan = currentPlan;
		this.job = job;
		owner = actor;
		isRecalculationSuccess = false;
	}

	public override void DoMultithread()
	{
		base.DoMultithread();
		try
		{
			CreatePlan();
		}
		catch (Exception ex)
		{
			Debug.unityLogger.LogError("Error", "Problem with " + actor.name + "'s GoapThread! \nJob is " + (job?.jobType.ToStringEnum() ?? "None") + "\nTarget is " + target.name + "\n" + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public override void FinishMultithread()
	{
		base.FinishMultithread();
		ReturnPlanFromGoapThread();
	}

	public void CreatePlan()
	{
		if (recalculationPlan != null)
		{
			RecalculatePlan();
		}
		else
		{
			CreateNewPlan();
		}
	}

	private void CreateNewPlan()
	{
		string empty = string.Empty;
		if (goalType != INTERACTION_TYPE.NONE)
		{
			GoapAction goapAction = InteractionManager.Instance.goapActionData[goalType];
			if (target.CanAdvertiseActionToActor(actor, goapAction, job) && target.Advertises(goalType))
			{
				actor.planner.PlanActions(target, goapAction, isPersonalPlan, ref empty, job, this);
			}
		}
		else
		{
			actor.planner.PlanActions(target, goalEffect, isPersonalPlan, ref empty, job, this);
		}
	}

	private void RecalculatePlan()
	{
		if (!recalculationPlan.isEnd)
		{
			string empty = string.Empty;
			isRecalculationSuccess = actor.planner.RecalculatePathForPlan(recalculationPlan, job, ref empty);
		}
	}

	private void ReturnPlanFromGoapThread()
	{
		if (actor.planner != null)
		{
			GoapPlan createdPlan = actor.planner.TransformRawPlanToActualPlan();
			actor.planner.ReceivePlanFromGoapThread(createdPlan);
		}
	}

	public void Reset()
	{
		actor = null;
		goalEffect = null;
		goalType = INTERACTION_TYPE.NONE;
		target = null;
		isPersonalPlan = false;
		job = null;
		log = null;
		recalculationPlan = null;
		isRecalculationSuccess = false;
		owner = null;
	}
}
