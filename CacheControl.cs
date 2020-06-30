using System;
using Harmony;
using MelonLoader;
using MelonLoader.TinyJSON;
using ModSettings;
using UnityEngine;

namespace CacheControl {
	internal class CacheControl : MelonMod {

		private const string SAVE_FILE_NAME = "MOD_CacheControl";
		private const string SPAWNER_NAME = "PrepperHatch";

		private static CacheSettings settings;

		public override void OnApplicationStart() {
			CacheSettingsGUI settingsGUI = new CacheSettingsGUI();
			settings = settingsGUI.confirmedSettings;
			settingsGUI.AddToCustomModeMenu(Position.BelowGear);

			uConsole.RegisterCommand("force_reroll_caches", new Action(RerollCommand.ForceRerollCaches));

			Debug.Log($"[{InfoAttribute.Name}] version {InfoAttribute.Version} loaded!");
		}

		[HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData", new Type[] { typeof(SaveSlotType), typeof(string) })]
		private static class SaveCacheSettings {
			private static void Postfix(SaveSlotType gameMode, string name) {
				if (!IsEnabled())
					return;

				string data = JSON.Dump(settings, EncodeOptions.NoTypeHints);
				SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, SAVE_FILE_NAME, data);
			}
		}

		[HarmonyPatch(typeof(SaveGameSystem), "RestoreGlobalData", new Type[] { typeof(string) })]
		private static class LoadCacheSettings {
			private static void Prefix(string name) {
				string data = SaveGameSlots.LoadDataFromSlot(name, SAVE_FILE_NAME);

				if (!string.IsNullOrEmpty(data)) {
					JSON.Load(data).Populate(settings);
				} else {
					settings.enabled = false;
				}
			}
		}

		[HarmonyPatch(typeof(RandomSpawnObject), "ActivateRandomObject", new Type[0])]
		private static class ApplyCacheSettings {
			private static void Prefix(RandomSpawnObject __instance) {
				if (!IsEnabled())
					return;

				if (__instance.m_RerollAfterGameHours == 0f && __instance.gameObject.name == SPAWNER_NAME) {
					// Found the cache spawner
					ModifySpawner(__instance);
				}
			}
		}

		private static bool IsEnabled() {
			return settings.enabled && GameManager.InCustomMode() && !GameManager.IsStoryMode();
		}

		private static void ModifySpawner(RandomSpawnObject spawner) {
			string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
			switch (sceneName) {
				case "LakeRegion":
					Debug.Log("[CacheControl] Modified spawner in ML to spawn " + settings.numCachesML + " caches");
					SetNumCachesToSpawn(spawner, settings.numCachesML);
					break;
				case "RuralRegion":
					Debug.Log("[CacheControl] Modified spawner in PV to spawn " + settings.numCachesPV + " caches");
					SetNumCachesToSpawn(spawner, settings.numCachesPV);
					break;
				default:
					Debug.Log("[CacheControl] Cache spawner in unknown scene " + sceneName);
					break;
			}
		}

		internal static RandomSpawnObject FindCacheSpawner() {
			return GameObject.Find(SPAWNER_NAME)?.GetComponentInChildren<RandomSpawnObject>();
		}

		internal static void SetNumCachesToSpawn(RandomSpawnObject spawner, int numCachesToSpawn) {
			spawner.m_NumObjectsToEnablePilgrim = numCachesToSpawn;
			spawner.m_NumObjectsToEnableVoyageur = numCachesToSpawn;
			spawner.m_NumObjectsToEnableStalker = numCachesToSpawn;
			spawner.m_NumObjectsToEnableInterloper = numCachesToSpawn;
		}

		// RandomBinomial ~ Binomial(n, p)
		// Could implement this properly using an inverse binomial distribution, but for small n,
		// this is plenty fast while being trivial to implement.
		internal static int RandomBinomial(int n, float p) {
			int result = 0;
			for (int i = 0; i < n; ++i) {
				if (Utils.RollChance(p)) {
					++result;
				}
			}
			return result;
		}
	}
}
