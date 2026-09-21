using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Ruinarch.Modding;
using UnityEngine;

namespace ExampleMod
{
	/// <summary>
	/// Minimal reference mod. Proves the full pipeline works:
	///  1. the loader finds and instantiates this class,
	///  2. OnLoad runs at startup and logs,
	///  3. a Harmony patch is applied and actually fires in-game.
	/// Copy this folder as a starting point for your own mod.
	/// </summary>
	public class ExampleMod : IRuinarchMod
	{
		internal static ModLogger Log;

		public void OnLoad(ModContext context)
		{
			Log = context.Logger;
			Log.Info($"Hello from {context.Info.name} v{context.Info.version}!");
			Log.Info($"Loaded from: {context.ModDirectory}");

			var target = AccessTools.Method(typeof(MainMenuManager), "Start");
			Log.Info("AccessTools MainMenuManager.Start = " + (target != null ? target.ToString() : "NOT FOUND"));

			Harmony harmony = new Harmony(context.Info.id);
			harmony.PatchAll(typeof(ExampleMod).Assembly);

			var patched = Harmony.GetAllPatchedMethods()
				.Select(m => (m.DeclaringType != null ? m.DeclaringType.Name : "?") + "." + m.Name)
				.ToArray();
			Log.Info($"Harmony attached to {patched.Length} method(s): {string.Join(", ", patched)}");

			// Self-test: call a method we just patched, right now. If the postfix
			// fires, Harmony detours execute in this runtime - which decides whether
			// the MainMenuManager patch not firing is a Harmony problem or just that
			// the target method is never invoked in normal flow.
			int probeResult = Probe();
			Log.Info($"Probe() returned {probeResult}");

			Log.Info("Waiting for the main menu...");
		}

		/// <summary>
		/// Target for the self-patch detour test. NoInlining + a real return value
		/// stop Mono from inlining the callsite, so the Harmony detour can run.
		/// </summary>
		[MethodImpl(MethodImplOptions.NoInlining)]
		public static int Probe()
		{
			return System.Environment.TickCount;
		}
	}

	[HarmonyPatch(typeof(ExampleMod), "Probe")]
	public static class Probe_Patch
	{
		private static void Postfix()
		{
			ExampleMod.Log?.Info(">>> SELF-PATCH postfix fired - Harmony detours WORK in this runtime <<<");
		}
	}

	[HarmonyPatch(typeof(MainMenuManager), "Start")]
	public static class MainMenuManager_Start_Patch
	{
		private static void Postfix()
		{
			Debug.Log("[example.mod] >>> MainMenuManager.Start ran - our Harmony patch is LIVE! <<<");
			ExampleMod.Log?.Info(">>> MainMenuManager.Start ran - our Harmony patch is LIVE! <<<");
		}
	}
}
