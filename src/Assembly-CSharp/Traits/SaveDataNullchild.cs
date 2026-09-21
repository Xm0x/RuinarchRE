using System.Collections.Generic;

namespace Traits;

public class SaveDataNullchild : SaveDataTrait
{
	public List<PLAYER_SKILL_TYPE> lockedSkill;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Nullchild nullchild = trait as Nullchild;
		lockedSkill = nullchild.lockedSkills;
	}
}
