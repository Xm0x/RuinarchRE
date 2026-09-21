using System;

[Serializable]
public class SaveDataLimiterComponent : SaveData<LimiterComponent>
{
	public int canWitnessValue;

	public int canMoveValue;

	public int canBeAttackedValue;

	public int canPerformValue;

	public int canTakeJobsValue;

	public int sociableValue;

	public int canDoFullnessRecoveryValue;

	public int canDoHappinessRecoveryValue;

	public int canDoTirednessRecoveryValue;

	public int targetedByDemonicSnatchValue;

	public int hinderAfflictionVotes;

	public override void Save(LimiterComponent data)
	{
		canWitnessValue = data.canWitnessValue;
		canMoveValue = data.canMoveValue;
		canBeAttackedValue = data.canBeAttackedValue;
		canPerformValue = data.canPerformValue;
		canTakeJobsValue = data.canTakeJobsValue;
		sociableValue = data.sociableValue;
		canDoFullnessRecoveryValue = data.canDoFullnessRecoveryValue;
		canDoHappinessRecoveryValue = data.canDoHappinessRecoveryValue;
		canDoTirednessRecoveryValue = data.canDoTirednessRecoveryValue;
		targetedByDemonicSnatchValue = data.targetedByDemonicSnatchValue;
		hinderAfflictionVotes = data.hinderAfflictionVotes;
	}

	public override LimiterComponent Load()
	{
		return new LimiterComponent(this);
	}
}
