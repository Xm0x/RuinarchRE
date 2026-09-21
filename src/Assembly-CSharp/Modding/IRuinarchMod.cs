namespace Ruinarch.Modding
{
	/// <summary>
	/// Entry point contract for a Ruinarch mod. Implement this on exactly one
	/// public, parameterless-constructible class in your mod assembly. The loader
	/// instantiates it and calls <see cref="OnLoad"/> once, very early in startup
	/// (before the first scene loads) so any Harmony patches you apply are in
	/// place before the game code they target ever runs.
	/// </summary>
	public interface IRuinarchMod
	{
		/// <summary>
		/// Called once when the mod is loaded. Do your setup here: apply Harmony
		/// patches, subscribe to <c>Messenger</c> signals, cache config, etc.
		/// Throwing here is caught and logged by the loader; it will not crash the
		/// game or other mods.
		/// </summary>
		void OnLoad(ModContext context);
	}
}
