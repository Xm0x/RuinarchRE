public static class PlayerDB
{
	public const int MAX_LEVEL_SUMMON = 3;

	public const int MAX_LEVEL_ARTIFACT = 3;

	public const int MAX_LEVEL_COMBAT_ABILITY = 3;

	public const int MAX_LEVEL_INTERVENTION_ABILITY = 3;

	public const int DIVINE_INTERVENTION_DURATION = 2880;

	public const int MAX_INTEL = 5;

	public const int MAX_INTERVENTION_ABILITIES = 4;

	public const string Zap_Action = "Zap";

	public const string Seize_Character_Action = "Seize Character";

	public const string Seize_Object_Action = "Seize Object";

	public const string Remove_Trait_Action = "Remove Trait";

	private static string[] unlockableActions = new string[4] { "Seize Object", "Seize Character", "Remove Trait", "Zap" };

	private static string[] unlockableStructures = new string[5] { "THE_KENNEL", "THE_PIT", "TORTURE_CHAMBER", "THE_EYE", "THE_PROFANE" };

	public static string[] GetChoicesForUnlockableType(ARTIFACT_UNLOCKABLE_TYPE type)
	{
		return type switch
		{
			ARTIFACT_UNLOCKABLE_TYPE.Action => unlockableActions, 
			ARTIFACT_UNLOCKABLE_TYPE.Structure => unlockableStructures, 
			_ => null, 
		};
	}
}
