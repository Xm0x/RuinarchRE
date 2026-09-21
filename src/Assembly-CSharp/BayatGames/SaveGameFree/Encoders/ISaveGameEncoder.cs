namespace BayatGames.SaveGameFree.Encoders;

public interface ISaveGameEncoder
{
	string Encode(string input, string password);

	string Decode(string input, string password);
}
