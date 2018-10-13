using System;
using System.Reflection;
using ModSettings;

namespace CacheControl {
	internal class CacheSettingsGUI : ModSettingsBase {

		private const int CACHES_IN_ML = 9;
		private const int CACHES_IN_PV = 9;

		internal enum SpawnAlgorithm {
			FixedNumber, SpawnChance
		}

		internal readonly CacheSettings confirmedSettings = new CacheSettings();

		[Name("Override number of Prepper Caches")]
		[Description("Determines whether CacheControl is enabled in this save file.")]
		public bool enabled = false;

		[Name("Cache spawning algorithm")]
		[Description("- Fixed number of caches:\n     Spawns exactly the number of caches\n     you specified.\n"
		             + "- Cache spawn chance:\n     Each cache has a set chance of spawning,\n     leading to a randomized amount of caches.")]
		[Choice("Fixed number of caches", "Cache spawn chance")]
		public SpawnAlgorithm algorithm = SpawnAlgorithm.FixedNumber;

		[Name("Number of caches in Mystery Lake")]
		[Slider(0, CACHES_IN_ML)]
		public int numCachesML = 0;

		[Name("Number of caches in Pleasant Valley")]
		[Slider(0, CACHES_IN_PV)]
		public int numCachesPV = 0;

		[Name("Cache spawn chance in Mystery Lake")]
		[Slider(0, 100, NumberFormat = "{0:F0}%")]
		public float spawnChanceML = 0;

		[Name("Cache spawn chance in Pleasant Valley")]
		[Slider(0, 100, NumberFormat = "{0:F0}%")]
		public float spawnChancePV = 0;

		internal CacheSettingsGUI() {
			RefreshFieldsVisible();
		}

		protected override void OnChange(FieldInfo field, object oldValue, object newValue) {
			if (field.Name == nameof(enabled) || field.Name == nameof(algorithm)) {
				RefreshFieldsVisible();
			}
		}

		internal void RefreshFieldsVisible() {
			SetFieldVisible(nameof(algorithm), enabled);
			SetFieldVisible(nameof(numCachesML), enabled && algorithm == SpawnAlgorithm.FixedNumber);
			SetFieldVisible(nameof(numCachesPV), enabled && algorithm == SpawnAlgorithm.FixedNumber);
			SetFieldVisible(nameof(spawnChanceML), enabled && algorithm == SpawnAlgorithm.SpawnChance);
			SetFieldVisible(nameof(spawnChancePV), enabled && algorithm == SpawnAlgorithm.SpawnChance);
		}

		protected override void OnConfirm() {
			confirmedSettings.enabled = enabled;

			switch (algorithm) {
				case SpawnAlgorithm.FixedNumber:
					confirmedSettings.numCachesML = numCachesML;
					confirmedSettings.numCachesPV = numCachesPV;
					break;
				case SpawnAlgorithm.SpawnChance:
					confirmedSettings.numCachesML = CacheControl.RandomBinomial(CACHES_IN_ML, spawnChanceML);
					confirmedSettings.numCachesPV = CacheControl.RandomBinomial(CACHES_IN_PV, spawnChancePV);
					break;
				default:
					throw new NotImplementedException();
			}
		}
	}
}
