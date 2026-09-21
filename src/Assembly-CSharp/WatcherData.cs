using UnityEngine;

public class WatcherData : DemonicStructurePlayerSkill
{
	public override string name => "Watcher";

	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.WATCHER;

	public override string description => "This Structure produces Eyes. The Player may place Eyes all over the world to obtain information and store them as Intel.";

	public override Vector2Int size => new Vector2Int(5, 6);

	public WatcherData()
	{
		base.structureType = STRUCTURE_TYPE.WATCHER;
	}
}
