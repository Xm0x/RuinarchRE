namespace Quests.Alerts;

public class SaveDataBuildImpHutOrCrypt : SaveDataGameAlert
{
	public int currentStep;

	public STRUCTURE_TYPE neededStructureType;

	public PLAYER_SKILL_TYPE neededPlayerSkillType;

	public string strNeededStructure;

	public string strNeededStructurePlural;

	public string strProducedMonsters;

	public override void Save(GameAlert data)
	{
		base.Save(data);
		BuildImpHutOrCrypt buildImpHutOrCrypt = data as BuildImpHutOrCrypt;
		currentStep = buildImpHutOrCrypt.currentStep;
		neededStructureType = buildImpHutOrCrypt.neededStructureType;
		neededPlayerSkillType = buildImpHutOrCrypt.neededPlayerSkillType;
		strNeededStructure = buildImpHutOrCrypt.strNeededStructure;
		strProducedMonsters = buildImpHutOrCrypt.strProducedMonsters;
		strNeededStructurePlural = buildImpHutOrCrypt.strNeededStructurePlural;
	}
}
