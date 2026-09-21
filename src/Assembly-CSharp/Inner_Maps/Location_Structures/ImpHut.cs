using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class ImpHut : DemonicStructure
{
	private const int ProvidedCharges = 3;

	public override SUMMON_TYPE housedMonsterType => SUMMON_TYPE.Imp;

	protected override int maximumConnectedMonsters => 3;

	public ImpHut(Region location)
		: base(STRUCTURE_TYPE.IMP_HUT, location)
	{
		SetMaxHPAndReset(1500);
	}

	public ImpHut(Region location, SaveDataDemonicStructure data)
		: base(location, data)
	{
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingMaxCharge(SUMMON_TYPE.Imp, 3, adjustCurrentCharges: false);
		if (PlayerManager.Instance.player.underlingsComponent.HasMonsterUnderlingEntry(SUMMON_TYPE.Imp, out var p_monsterAndDemonUnderlingCharges) && p_monsterAndDemonUnderlingCharges.hasMaxCharge && p_monsterAndDemonUnderlingCharges.currentCharges < p_monsterAndDemonUnderlingCharges.maxCharges)
		{
			p_monsterAndDemonUnderlingCharges.StartMonsterReplenish();
		}
	}

	protected override void DestroyStructure(Character p_responsibleCharacter = null, bool isPlayerSource = false, bool shouldBeCleanedUp = true)
	{
		PlayerManager.Instance.player.underlingsComponent.AdjustMonsterUnderlingMaxCharge(SUMMON_TYPE.Imp, -3, adjustCurrentCharges: false);
		base.DestroyStructure(p_responsibleCharacter, isPlayerSource, shouldBeCleanedUp: false);
		if (shouldBeCleanedUp)
		{
			MarkForCleanup();
		}
	}

	public override void SetStructureObject(LocationStructureObject structureObj)
	{
		base.SetStructureObject(structureObj);
		Vector3 position = structureObj.transform.position;
		position.x -= 0.5f;
		worldPosition = position;
	}
}
