using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FakePlayer : MonoBehaviour
{
	[FormerlySerializedAs("currenciesComponent")]
	public FakeCurrenciesComponent fakeCurrenciesComponent = new FakeCurrenciesComponent();

	public PlayerSkillComponent skillComponent = new PlayerSkillComponent();

	public List<PLAYER_SKILL_TYPE> availableSkills;

	public List<SUMMON_TYPE> summons;

	public SkillProgressionManager progressionManager = new SkillProgressionManager();

	public PlayerUnderlingsComponent underlingsComponent { get; private set; }

	public void Initialize()
	{
		PlayerSkillManager.Instance.Initialize();
		availableSkills.ForEach(delegate(PLAYER_SKILL_TYPE eachAvailableSkill)
		{
			skillComponent.AddAndCategorizePlayerSkill(eachAvailableSkill, testScene: true);
		});
		progressionManager.CheckRequirementsAndGetUnlockCost(skillComponent, fakeCurrenciesComponent, PLAYER_SKILL_TYPE.LIGHTNING);
		underlingsComponent = new PlayerUnderlingsComponent();
		summons.ForEach(delegate(SUMMON_TYPE eachSummon)
		{
			underlingsComponent.AdjustMonsterUnderlingCharge(eachSummon, 5);
		});
	}
}
