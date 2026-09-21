using Traits;

public class SaveDataNecromancer : SaveDataTrait
{
	public string lairStructureID;

	public string attackVillageID;

	public string prevClassName;

	public int lifeAbsorbed;

	public int energy;

	public bool doNotSpawnLair;

	public GameDate spawnLairDate;

	public int bonusIntelligence;

	public int bonusPiercing;

	public int bonusAllResistances;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Necromancer necromancer = trait as Necromancer;
		lairStructureID = ((necromancer.lairStructure == null) ? string.Empty : necromancer.lairStructure.persistentID);
		attackVillageID = ((necromancer.attackVillageTarget == null) ? string.Empty : necromancer.attackVillageTarget.persistentID);
		prevClassName = necromancer.prevClassName;
		lifeAbsorbed = necromancer.lifeAbsorbed;
		energy = necromancer.energy;
		doNotSpawnLair = necromancer.doNotSpawnLair;
		spawnLairDate = necromancer.spawnLairDate;
	}
}
