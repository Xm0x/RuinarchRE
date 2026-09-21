using System;
using System.IO;
using UnityEngine;

namespace Ruinarch.Modding
{
	/// <summary>
	/// Descriptive metadata for a mod. Read from an optional <c>mod.json</c> file
	/// placed next to the mod DLL:
	/// <code>
	/// {
	///   "id": "author.mymod",
	///   "name": "My Mod",
	///   "version": "1.0.0",
	///   "author": "you",
	///   "description": "what it does"
	/// }
	/// </code>
	/// Any missing field falls back to a sensible default derived from the
	/// assembly file name. Parsed with Unity's <see cref="JsonUtility"/> so no
	/// external JSON dependency is needed.
	/// </summary>
	[Serializable]
	public class ModInfo
	{
		public string id;
		public string name;
		public string version;
		public string author;
		public string description;

		/// <summary>
		/// Load metadata from <paramref name="jsonPath"/> if it exists, otherwise
		/// synthesize defaults from <paramref name="fallbackName"/> (the DLL name).
		/// Never returns null and never throws on malformed JSON.
		/// </summary>
		public static ModInfo LoadOrDefault(string jsonPath, string fallbackName)
		{
			ModInfo info = null;
			try
			{
				if (!string.IsNullOrEmpty(jsonPath) && File.Exists(jsonPath))
				{
					info = JsonUtility.FromJson<ModInfo>(File.ReadAllText(jsonPath));
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning($"[ModLoader] Bad mod.json at {jsonPath}: {e.Message}");
			}
			if (info == null)
			{
				info = new ModInfo();
			}
			if (string.IsNullOrEmpty(info.id))
			{
				info.id = fallbackName;
			}
			if (string.IsNullOrEmpty(info.name))
			{
				info.name = fallbackName;
			}
			if (string.IsNullOrEmpty(info.version))
			{
				info.version = "0.0.0";
			}
			if (string.IsNullOrEmpty(info.author))
			{
				info.author = "unknown";
			}
			if (info.description == null)
			{
				info.description = string.Empty;
			}
			return info;
		}

		public override string ToString()
		{
			return $"{name} v{version} by {author} ({id})";
		}
	}
}
