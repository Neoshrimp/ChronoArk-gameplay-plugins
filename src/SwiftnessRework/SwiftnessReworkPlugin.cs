using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using System.Linq;
using Debug = UnityEngine.Debug;
using DarkTonic.MasterAudio;
using System.Reflection.Emit;

namespace SwiftnessRework
{
	[BepInPlugin(GUID, "Swiftness Rework", version)]
	[BepInProcess("ChronoArk.exe")]
	public class SwiftnessReworkPlugin : BaseUnityPlugin
	{

		public const string GUID = "neo.ca.gameplay.swiftnessRework";
		public const string version = "1.0.0";


		private static readonly Harmony harmony = new Harmony(GUID);

		private static BepInEx.Logging.ManualLogSource logger;

		static QuickManager quickManager;

		void Awake()
		{

			logger = Logger;
			var defaultQuickness = new HashSet<string>();
			defaultQuickness.Add(GDEItemKeys.Skill_S_Public_9);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_P_0);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_9);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_0);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_2);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_11);

			quickManager = new QuickManager(defaultQuickness);
			harmony.PatchAll();

			StartCoroutine(CleanFields());
		}
		void OnDestroy()
		{
			if (harmony != null)
				harmony.UnpatchAll(GUID);
		}

		public static bool cullFields = true;

		IEnumerator CleanFields()
		{
			while (cullFields)
			{
				yield return new WaitForSeconds(30);
				quickManager.CullDestroyed();
			}

		}


		[HarmonyPatch]
		class SkillConstPatch
		{
			static IEnumerable<MethodBase> TargetMethods()
			{
				//Debug.Log(AccessTools.GetDeclaredConstructors(typeof(Skill))[0]);
				yield return AccessTools.GetDeclaredConstructors(typeof(Skill))[0];
			}

			static void Postfix(Skill __instance)
			{

				//AddField(__instance, true);
				// doesnt have key yet
				quickManager.AddField(__instance, false);
			}
		}

		[HarmonyPatch(typeof(Skill), nameof(Skill.initField))]
		class SkillinitFieldPatch
		{
			static void Postfix(Skill __instance)
			{
				if (quickManager.defaultQuickness.Contains(__instance.MySkill.Key) || quickManager.defaultQuickness.Contains(__instance.MySkill.KeyID))
				{
					quickManager.AddField(__instance, true);
				}
			}
		}

		[HarmonyPatch]
		class SkillExtendedConstPatch
		{
			static IEnumerable<MethodBase> TargetMethods()
			{
				//Debug.Log(AccessTools.GetDeclaredConstructors(typeof(Skill))[0]);
				yield return AccessTools.GetDeclaredConstructors(typeof(Skill_Extended))[0];
			}

			static void Postfix(Skill __instance)
			{
				quickManager.AddField(__instance, false);
			}
		}

		/*        [HarmonyPatch(typeof(Skill), nameof(Skill.CloneSkill)]
				class Skill_ClonePatch
				{

				}*/

		static bool CheckQuick(Skill skill)
		{
			return quickManager.SkillGetQuick(skill);
		}

		[HarmonyPatch]
		class BattleActWindow_CountSkillPointEntePatch
		{


			static IEnumerable<MethodBase> TargetMethods()
			{
				yield return AccessTools.Method(typeof(BattleActWindow), nameof(BattleActWindow.CountSkillPointEnter));
				yield return AccessTools.Method(typeof(BattleAlly), nameof(BattleAlly.UseSkillAfter));
				yield return AccessTools.Method(typeof(BattleSystem), nameof(BattleSystem.CastEnqueue));

			}

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				int i = 0;
				var ciList = instructions.ToList();
				var c = ciList.Count();
				foreach (var ci in instructions)
				{
					if (ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(Skill), "get_NotCount")))
					{
						Debug.Log("deez");
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SwiftnessReworkPlugin), nameof(SwiftnessReworkPlugin.CheckQuick)));
					}
					else
					{
						yield return ci;
					}
					i++;
				}
			}



			//[HarmonyPatch(typeof(SkillButton), nameof(SkillButton.ChoiceSkill))] quick

		}
	}
}
