using System;
using System.IO;
using UnityEngine;

namespace Ruinarch.Modding
{
	/// <summary>
	/// Per-mod logger. Writes to the Unity player log (visible in
	/// <c>Player.log</c>) prefixed with the mod id, and appends to a shared
	/// <c>Mods/mods.log</c> file for easy out-of-game inspection.
	/// </summary>
	public sealed class ModLogger
	{
		private readonly string _prefix;
		private readonly string _file;

		internal ModLogger(string modId, string logFile)
		{
			_prefix = $"[{modId}] ";
			_file = logFile;
		}

		public void Info(string message)
		{
			Debug.Log(_prefix + message);
			Append("INFO", message);
		}

		public void Warning(string message)
		{
			Debug.LogWarning(_prefix + message);
			Append("WARN", message);
		}

		public void Error(string message)
		{
			Debug.LogError(_prefix + message);
			Append("ERROR", message);
		}

		private void Append(string level, string message)
		{
			if (string.IsNullOrEmpty(_file))
			{
				return;
			}
			try
			{
				File.AppendAllText(_file, $"{DateTime.Now:HH:mm:ss} [{level}] {_prefix}{message}{Environment.NewLine}");
			}
			catch
			{
				// Logging must never take the game (or a mod) down.
			}
		}
	}
}
