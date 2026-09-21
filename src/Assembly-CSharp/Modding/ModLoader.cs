using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Ruinarch.Modding
{
	/// <summary>
	/// A mod that has been successfully loaded.
	/// </summary>
	public sealed class LoadedMod
	{
		public ModInfo Info { get; internal set; }
		public Assembly Assembly { get; internal set; }
		public IRuinarchMod Instance { get; internal set; }
		public string Directory { get; internal set; }
	}

	/// <summary>
	/// First-class mod loader baked into the game. Runs automatically before the
	/// first scene loads (via <see cref="RuntimeInitializeOnLoadMethodAttribute"/>),
	/// scans the <c>Mods/</c> folder next to the executable, loads every mod
	/// assembly, and calls <see cref="IRuinarchMod.OnLoad"/> on each entry point.
	///
	/// Isolation: a mod that throws while loading is logged and skipped; it never
	/// takes down the game or the other mods. An <c>AssemblyResolve</c> hook lets
	/// mods drop their own dependencies (e.g. <c>0Harmony.dll</c>) anywhere under
	/// <c>Mods/</c> and have them resolve.
	/// </summary>
	public static class ModLoader
	{
		private static readonly List<LoadedMod> _loaded = new List<LoadedMod>();
		private static bool _initialized;

		/// <summary>Absolute path to the <c>Mods/</c> root (set during init).</summary>
		public static string ModsRoot { get; private set; }

		/// <summary>Shared log file all mods append to.</summary>
		public static string LogFile { get; private set; }

		/// <summary>Every mod that loaded successfully, in load order.</summary>
		public static IReadOnlyList<LoadedMod> Loaded => _loaded;

		// Runs from <Module>.cctor when Mono first loads Assembly-CSharp - the
		// earliest managed entry point, and the one that actually fires for a
		// post-build-injected loader (Unity's RuntimeInitializeOnLoads registry is
		// baked at build time and won't list us). The RuntimeInitializeOnLoadMethod
		// below is kept as a fallback for Unity-side rebuilds. Initialize() is
		// idempotent, so both firing is harmless.
		[System.Runtime.CompilerServices.ModuleInitializer]
		internal static void ModuleInit()
		{
			Initialize();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void Initialize()
		{
			if (_initialized)
			{
				return;
			}
			_initialized = true;

			try
			{
				ModsRoot = ResolveModsRoot();
				Directory.CreateDirectory(ModsRoot);
				LogFile = Path.Combine(ModsRoot, "mods.log");
				TruncateLog();

				// Let mods resolve their own dependencies from anywhere under Mods/.
				AppDomain.CurrentDomain.AssemblyResolve += ResolveFromMods;

				Debug.Log($"[ModLoader] Scanning for mods in: {ModsRoot}");
				List<string> dlls = DiscoverModDlls(ModsRoot);
				foreach (string dll in dlls)
				{
					TryLoad(dll);
				}
				Debug.Log($"[ModLoader] Done. {_loaded.Count} mod(s) loaded.");
			}
			catch (Exception e)
			{
				Debug.LogError($"[ModLoader] Fatal error during init: {e}");
			}
		}

		private static string ResolveModsRoot()
		{
			// Application.dataPath == "<gameRoot>/Ruinarch_Data"; put Mods/ at the
			// game root, next to Ruinarch.exe, where players expect to find it.
			string gameRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
			return Path.Combine(gameRoot, "Mods");
		}

		private static void TruncateLog()
		{
			try
			{
				File.WriteAllText(LogFile, $"# Ruinarch mod log - {DateTime.Now}{Environment.NewLine}");
			}
			catch
			{
			}
		}

		/// <summary>
		/// Candidate DLLs: every <c>*.dll</c> directly in Mods/ plus one level of
		/// subfolders (<c>Mods/MyMod/MyMod.dll</c>). Non-mod DLLs (dependencies
		/// like 0Harmony) simply expose no <see cref="IRuinarchMod"/> and are
		/// skipped after inspection.
		/// </summary>
		private static List<string> DiscoverModDlls(string root)
		{
			var result = new List<string>();
			result.AddRange(Directory.GetFiles(root, "*.dll", SearchOption.TopDirectoryOnly));
			foreach (string dir in Directory.GetDirectories(root))
			{
				result.AddRange(Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly));
			}
			return result;
		}

		private static void TryLoad(string dllPath)
		{
			string fileName = Path.GetFileNameWithoutExtension(dllPath);
			Assembly assembly;
			try
			{
				assembly = Assembly.LoadFrom(dllPath);
			}
			catch (Exception e)
			{
				Debug.LogWarning($"[ModLoader] Could not load '{Path.GetFileName(dllPath)}': {e.Message}");
				return;
			}

			// Already processed this assembly (e.g. a shared dependency)?
			if (_loaded.Any(m => m.Assembly == assembly))
			{
				return;
			}

			Type[] types = SafeGetTypes(assembly);
			Type entryType = types.FirstOrDefault(t =>
				t != null && !t.IsAbstract && !t.IsInterface && typeof(IRuinarchMod).IsAssignableFrom(t));

			if (entryType == null)
			{
				// Not a mod (dependency DLL, resource assembly, etc.) - fine.
				return;
			}

			try
			{
				string dir = Path.GetDirectoryName(dllPath);
				string jsonPath = Path.Combine(dir ?? ModsRoot, "mod.json");
				ModInfo info = ModInfo.LoadOrDefault(jsonPath, fileName);
				var logger = new ModLogger(info.id, LogFile);
				var context = new ModContext(info, dir, ModsRoot, logger);

				var instance = (IRuinarchMod)Activator.CreateInstance(entryType);
				instance.OnLoad(context);

				_loaded.Add(new LoadedMod
				{
					Info = info,
					Assembly = assembly,
					Instance = instance,
					Directory = dir
				});
				Debug.Log($"[ModLoader] Loaded {info}");
			}
			catch (Exception e)
			{
				Debug.LogError($"[ModLoader] Mod '{fileName}' failed in OnLoad and was skipped: {e}");
			}
		}

		private static Type[] SafeGetTypes(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				return ex.Types.Where(t => t != null).ToArray();
			}
			catch
			{
				return Array.Empty<Type>();
			}
		}

		private static Assembly ResolveFromMods(object sender, ResolveEventArgs args)
		{
			try
			{
				string simpleName = new AssemblyName(args.Name).Name;

				// Return an already-loaded assembly with a matching simple name first.
				Assembly existing = AppDomain.CurrentDomain.GetAssemblies()
					.FirstOrDefault(a => a.GetName().Name == simpleName);
				if (existing != null)
				{
					return existing;
				}

				if (string.IsNullOrEmpty(ModsRoot) || !Directory.Exists(ModsRoot))
				{
					return null;
				}
				string match = Directory
					.GetFiles(ModsRoot, simpleName + ".dll", SearchOption.AllDirectories)
					.FirstOrDefault();
				return match != null ? Assembly.LoadFrom(match) : null;
			}
			catch
			{
				return null;
			}
		}
	}
}
