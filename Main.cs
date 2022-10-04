using System.Collections;
using System.Reflection;

using UnityModManagerNet;
using HarmonyLib;

namespace UnlimitedSketchZoom {
#if DEBUG
	[EnableReloading]
#endif
	public static class Main {
		static Harmony harmony;

		public static bool Load(UnityModManager.ModEntry entry) {
			harmony = new Harmony(entry.Info.Id);

			entry.OnToggle = OnToggle;
#if DEBUG
			entry.OnUnload = OnUnload;
#endif

			return true;
		}

		static bool OnToggle(UnityModManager.ModEntry entry, bool active) {
			if (active) {
				harmony.PatchAll(Assembly.GetExecutingAssembly());
			} else {
				harmony.UnpatchAll(entry.Info.Id);
			}

			return true;
		}

#if DEBUG
		static bool OnUnload(UnityModManager.ModEntry entry) {
			return true;
		}
#endif

		[HarmonyPatch(typeof(Visor.ProcessorUI), nameof(Visor.ProcessorUI.EnterZoomView))]
		class Patch {
			static MethodInfo rewired_getAxis = AccessTools.Method(typeof(Rewired.Player), nameof(Rewired.Player.GetAxis), new[] { typeof(string) });

			static FieldInfo _canClose = AccessTools.Field(typeof(Visor.ProcessorUI), "_canClose");
			static FieldInfo _input = AccessTools.Field(typeof(Visor.ProcessorUI), "_input");
			static FieldInfo _sketchView = AccessTools.Field(typeof(Visor.ProcessorUI), "_sketchView");

			static IEnumerator EnterZoomView(Visor.ProcessorUI instance) {
				_canClose.SetValue(instance, false);
				
				try {
					float delta = ((Rewired.Player) _input.GetValue(instance)).GetAxis("ScaleComponent");

					if (delta > 0f) {
						((Visor.SketchView) _sketchView.GetValue(instance)).zoomFactor /= 2f;
					} else if (delta < 0f) {
						((Visor.SketchView) _sketchView.GetValue(instance)).zoomFactor *= 2f;
					}
				} finally {
					_canClose.SetValue(instance, true);
				}

				yield break;
			}

			public static bool Prefix(Visor.ProcessorUI __instance, ref IEnumerator __result) {
				__result = EnterZoomView(__instance);
				return false;
			}
		}
	}
}
