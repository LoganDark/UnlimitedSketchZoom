using System.Collections;
using System.Reflection;

using UnityModManagerNet;
using HarmonyLib;
using UnityEngine;

namespace UnlimitedSketchZoom {
#if DEBUG
	[EnableReloading]
#endif
	public static class Main {
		static Harmony harmony;
		static float factor = 2f;

		public static bool Load(UnityModManager.ModEntry entry) {
			harmony = new Harmony(entry.Info.Id);
			entry.OnToggle = OnToggle;
			entry.OnGUI = OnGUI;
			entry.OnSaveGUI = OnSaveGUI;
			return true;
		}

		private static void OnGUI(UnityModManager.ModEntry entry) {
			GUILayout.BeginHorizontal();
			UnityModManager.UI.DrawFloatField(ref factor, "Factor");
			GUILayout.Label("Controls how much you zoom with each click of the scroll wheel");
			GUILayout.EndHorizontal();
		}
		private static void OnSaveGUI(UnityModManager.ModEntry entry) {
			UnityModManager.Logger.Log("haha not saving lmao");
		}

		static bool OnToggle(UnityModManager.ModEntry entry, bool active) {
			if (active) {
				harmony.PatchAll(Assembly.GetExecutingAssembly());
			} else {
				harmony.UnpatchAll(entry.Info.Id);
			}

			return true;
		}

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
						((Visor.SketchView) _sketchView.GetValue(instance)).zoomFactor /= factor;
					} else if (delta < 0f) {
						((Visor.SketchView) _sketchView.GetValue(instance)).zoomFactor *= factor;
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
