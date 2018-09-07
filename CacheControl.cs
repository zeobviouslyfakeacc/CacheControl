﻿using System;
using System.Reflection;
using Harmony;
using ModSettings;
using UnityEngine;

namespace CacheControl {
	internal class CacheControl {

		private const string SAVE_FILE_NAME = "MOD_CacheControl";

		private static CacheSettings settings;

		public static void OnLoad() {
			settings = new CacheSettings();
			settings.AddToCustomModeMenu(Position.BelowGear);

			Version version = Assembly.GetExecutingAssembly().GetName().Version;
			Debug.Log("[CacheControl] Version " + version + " loaded!");
		}

		[HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData", new Type[] { typeof(SaveSlotType), typeof(string) })]
		private static class SaveCacheSettings {
			private static void Postfix(SaveSlotType gameMode, string name) {
				if (!IsEnabled())
					return;

				string data = JsonUtility.ToJson(settings);
				SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, SAVE_FILE_NAME, data);
			}
		}

		[HarmonyPatch(typeof(SaveGameSystem), "RestoreGlobalData", new Type[] { typeof(string) })]
		private static class LoadCacheSettings {
			private static void Prefix(string name) {
				string data = SaveGameSlots.LoadDataFromSlot(name, SAVE_FILE_NAME);

				if (!string.IsNullOrEmpty(data)) {
					JsonUtility.FromJsonOverwrite(data, settings);
				} else {
					settings.enabled = false;
				}

				settings.RefreshFieldsVisible();
			}
		}

		[HarmonyPatch(typeof(RandomSpawnObject), "ActivateRandomObject", new Type[0])]
		private static class ApplyCacheSettings {
			private static void Prefix(RandomSpawnObject __instance) {
				if (!IsEnabled())
					return;

				if (__instance.m_RerollAfterGameHours == 0f && __instance.gameObject.name == "PrepperHatch") {
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

		private static void SetNumCachesToSpawn(RandomSpawnObject spawner, int numCachesToSpawn) {
			spawner.m_NumObjectsToEnablePilgrim = numCachesToSpawn;
			spawner.m_NumObjectsToEnableVoyageur = numCachesToSpawn;
			spawner.m_NumObjectsToEnableStalker = numCachesToSpawn;
			spawner.m_NumObjectsToEnableInterloper = numCachesToSpawn;
		}
	}
}
