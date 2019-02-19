using System;
using System.Text;
using Harmony;
using UnityEngine;

namespace CacheControl {
	internal static class RerollCommand {

		internal static void ForceRerollCaches() {
			if (uConsole.GetNumParameters() != 1) {
				PrintUsage();
				return;
			}

			RandomSpawnObject spawner = CacheControl.FindCacheSpawner();
			if (!spawner) {
				uConsole.Log("|  Error: Could not find a cache spawner in the current scene.");
				return;
			}

			string arg = uConsole.GetString();
			int totalCaches = spawner.m_ObjectList.Length;
			int cachesToSpawn;

			if (int.TryParse(arg, out int num)) {
				cachesToSpawn = ClampInt(num, 0, totalCaches);
			} else if (arg.EndsWith("%") && float.TryParse(arg.Substring(0, arg.Length - 1), out float chance)) {
				chance = Mathf.Clamp(chance, 0f, 100f);
				cachesToSpawn = CacheControl.RandomBinomial(totalCaches, chance);
			} else {
				PrintUsage();
				return;
			}

			CacheControl.SetNumCachesToSpawn(spawner, cachesToSpawn);
			RerollSpawner(spawner);
			uConsole.Log("|  Successfully modified and rerolled cache spawner!");
		}

		private static void PrintUsage() {
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("|  Usage:");
			builder.AppendLine("|   - force_reroll_caches NumberOfCaches");
			builder.AppendLine("|      -> Activates exactly 'NumberOfCaches' caches in the current scene.");
			builder.AppendLine("|   - force_reroll_caches SpawnChance%");
			builder.AppendLine("|      -> Gives each cache a 'SpawnChance' percent chance of spawning.");
			builder.AppendLine("|");
			builder.AppendLine("|  Examples:");
			builder.AppendLine("|   - force_reroll_caches 5");
			builder.AppendLine("|   - force_reroll_caches 30%");
			builder.AppendLine("|");
			builder.Append("|  Warning: Executing this command can prevent access to previously discovered caches!");
			uConsole.Log(builder.ToString());
		}

		private static int ClampInt(int value, int min, int max) {
			return Math.Max(min, Math.Min(max, value));
		}

		private static void RerollSpawner(RandomSpawnObject spawner) {
			// First disable all objects spawned by this spawner
			spawner.DisableAll();

			// Then re-enable some, moving around the spawner to modify the seed that ActivateRandomObject uses
			// and changing the spawner's name so we don't activate our own patch
			Vector3 oldPos = spawner.transform.localPosition;
			string oldName = spawner.gameObject.name;

			try {
				spawner.transform.Translate(UnityEngine.Random.onUnitSphere);
				spawner.gameObject.name = "Temp";

				AccessTools.Method(typeof(RandomSpawnObject), "ActivateRandomObject").Invoke(spawner, new object[0]);
			} finally {
				spawner.transform.localPosition = oldPos;
				spawner.gameObject.name = oldName;
			}
		}
	}
}
