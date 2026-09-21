using System;
using System.Collections.Generic;
using UtilityScripts;

[Serializable]
public class SaveDataMovementComponent : SaveData<MovementComponent>
{
	public bool isRunning;

	public bool noRunExceptCombat;

	public bool noRunWithoutException;

	public int useRunSpeed;

	public float speedModifier;

	public float walkSpeedModifier;

	public float runSpeedModifier;

	public bool hasMovedOnCorruption;

	public bool isStationary;

	public bool cameFromWurmHole;

	public bool isTravellingInWorld;

	public List<string> structuresToAvoid;

	public int enableDiggingCounter;

	public int avoidSettlementsCounter;

	public int traversableTags;

	public int[] tagPenalties;

	public int previousTraversableTags;

	public int[] previousTagPenalties;

	public override void Save(MovementComponent data)
	{
		isRunning = data.isRunning;
		noRunExceptCombat = data.noRunExceptCombat;
		noRunWithoutException = data.noRunWithoutException;
		useRunSpeed = data.useRunSpeed;
		speedModifier = data.speedModifier;
		walkSpeedModifier = data.walkSpeedModifier;
		runSpeedModifier = data.runSpeedModifier;
		hasMovedOnCorruption = data.hasMovedOnCorruption;
		isStationary = data.isStationary;
		cameFromWurmHole = data.cameFromWurmHole;
		isTravellingInWorld = data.isTravellingInWorld;
		structuresToAvoid = RuinarchListPool<string>.Claim();
		for (int i = 0; i < data.structuresToAvoid.Count; i++)
		{
			structuresToAvoid.Add(data.structuresToAvoid[i].persistentID);
		}
		enableDiggingCounter = data.enableDiggingCounter;
		avoidSettlementsCounter = data.avoidSettlementsCounter;
		traversableTags = data.traversableTags;
		tagPenalties = data.tagPenalties;
		previousTraversableTags = data.previousTraversableTags;
		previousTagPenalties = data.previousTagPenalties;
	}

	public override MovementComponent Load()
	{
		return new MovementComponent(this);
	}

	public override void CleanUp()
	{
		if (structuresToAvoid != null)
		{
			RuinarchListPool<string>.Release(structuresToAvoid);
			structuresToAvoid = null;
		}
		tagPenalties = null;
		previousTagPenalties = null;
	}
}
