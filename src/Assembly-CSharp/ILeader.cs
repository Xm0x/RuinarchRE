public interface ILeader : ISavable
{
	int id { get; }

	string name { get; }

	RACE race { get; }

	GENDER gender { get; }

	Region currentRegion { get; }

	Region homeRegion { get; }
}
