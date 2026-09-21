public class RaceManager : BaseMonoBehaviour
{
	public static RaceManager Instance;

	public RaceDataDictionary racesDictionary;

	private void Awake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}

	public void Initialize()
	{
	}

	public bool CanCharacterDoGoapAction(Character character, INTERACTION_TYPE goapType)
	{
		return true;
	}

	public RaceData GetRaceData(RACE race)
	{
		if (racesDictionary.ContainsKey(race))
		{
			return racesDictionary[race];
		}
		return null;
	}
}
