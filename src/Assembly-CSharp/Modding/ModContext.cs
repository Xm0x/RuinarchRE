namespace Ruinarch.Modding
{
	/// <summary>
	/// Everything a mod is handed when it loads: its own metadata, where it lives
	/// on disk, and a logger. Create your own Harmony instance in
	/// <see cref="IRuinarchMod.OnLoad"/> with <c>new HarmonyLib.Harmony(context.Info.id)</c>
	/// - drop <c>0Harmony.dll</c> in the <c>Mods/</c> folder and the loader's
	/// assembly resolver will find it.
	/// </summary>
	public sealed class ModContext
	{
		/// <summary>Metadata from the mod's <c>mod.json</c> (or defaults).</summary>
		public ModInfo Info { get; }

		/// <summary>Absolute path to the folder this mod's DLL was loaded from.</summary>
		public string ModDirectory { get; }

		/// <summary>Absolute path to the shared <c>Mods/</c> root.</summary>
		public string ModsRoot { get; }

		/// <summary>Logger scoped to this mod (Player.log + Mods/mods.log).</summary>
		public ModLogger Logger { get; }

		internal ModContext(ModInfo info, string modDirectory, string modsRoot, ModLogger logger)
		{
			Info = info;
			ModDirectory = modDirectory;
			ModsRoot = modsRoot;
			Logger = logger;
		}
	}
}
