using System.Reflection;
using ModSettings;

namespace CacheControl {
	internal class CacheSettings : ModSettingsBase {

		private const int CACHES_IN_ML = 9;
		private const int CACHES_IN_PV = 9;

		[Name("Override number of Prepper Caches")]
		[Description("Determines whether CacheControl is enabled in this save file.")]
		public bool enabled = false;

		[Name("Number of caches in Mystery Lake")]
		[Slider(0, CACHES_IN_ML)]
		public int numCachesML = 0;

		[Name("Number of caches in Pleasant Valley")]
		[Slider(0, CACHES_IN_PV)]
		public int numCachesPV = 0;

		internal CacheSettings() {
			RefreshFieldsVisible();
		}

		protected override void OnChange(FieldInfo field, object oldValue, object newValue) {
			if (field.Name == nameof(enabled)) {
				RefreshFieldsVisible();
			}
		}

		internal void RefreshFieldsVisible() {
			SetFieldVisible(nameof(numCachesML), enabled);
			SetFieldVisible(nameof(numCachesPV), enabled);
		}
	}
}
