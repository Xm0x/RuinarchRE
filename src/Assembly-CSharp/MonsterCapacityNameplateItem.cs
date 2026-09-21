using UnityEngine;

public class MonsterCapacityNameplateItem : NameplateItem<MonsterCapacity>
{
	[Header("Monster Capacity Attributes")]
	[SerializeField]
	private CharacterPortrait classPortrait;

	private SUMMON_TYPE _summonType;

	public SUMMON_TYPE summonType => _summonType;

	public override void SetObject(MonsterCapacity o)
	{
		base.SetObject(o);
		_summonType = o.summonType;
		mainLbl.text = o.strSummonType;
		subLbl.text = o.remainingCharges + "/" + o.maxCapacity;
	}

	public void UpdateData(MonsterCapacity p_monsterCapacity)
	{
		mainLbl.text = p_monsterCapacity.strSummonType;
		subLbl.text = p_monsterCapacity.remainingCharges + "/" + p_monsterCapacity.maxCapacity;
		classPortrait.GeneratePortrait(CharacterManager.Instance.GeneratePortraitSettings(p_monsterCapacity.monsterRace, p_monsterCapacity.strMonsterClass));
	}
}
