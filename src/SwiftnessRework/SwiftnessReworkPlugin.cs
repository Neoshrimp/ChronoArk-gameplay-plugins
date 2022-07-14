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

			defaultQuickness.Add(GDEItemKeys.Skill_S_Mement_P);


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
				yield return AccessTools.GetDeclaredConstructors(typeof(Skill))[0];
			}

			static void Postfix(Skill __instance)
			{

				quickManager.AddField(__instance, false);
			}
		}

		[HarmonyPatch]
		class SkillExtendedConstPatch
		{
			static IEnumerable<MethodBase> TargetMethods()
			{
				yield return AccessTools.GetDeclaredConstructors(typeof(Skill_Extended))[0];
			}

			static void Postfix(Skill __instance)
			{
				quickManager.AddField(__instance, false);
			}
		}

		[HarmonyPatch(typeof(Skill), nameof(Skill.initField))]
		class SkillinitFieldPatch
		{
			static void Postfix(Skill __instance)
			{
				if (quickManager.defaultQuickness.Contains(__instance.MySkill.KeyID))
				{
					Debug.Log(__instance.MySkill.Key);
					Debug.Log(__instance.MySkill.KeyID);

					quickManager.SetVal(__instance, true);
				}
			}
		}



		/*        [HarmonyPatch(typeof(Skill), nameof(Skill.CloneSkill)]
				class Skill_ClonePatch
				{

				}*/
			
		static bool CheckQuick(Skill skill)
		{
			Debug.Log($"Quick: {quickManager.SkillGetQuick(skill)}");
			return quickManager.SkillGetQuick(skill);
		}

		[HarmonyPatch]
		class CheckQuickPatch
		{


			static IEnumerable<MethodBase> TargetMethods()
			{
				yield return AccessTools.Method(typeof(BattleActWindow), nameof(BattleActWindow.CountSkillPointEnter));
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
		}


		[HarmonyDebug]
        [HarmonyPatch(typeof(BattleAlly), nameof(BattleAlly.UseSkillAfter))]
        class Swiftness2IgnoreOverlaodPatch
        {

			static void IcreaseOverload(Skill skill, BattleAlly ally)
			{
				if (!skill.NotCount && !skill.IsNowCasting && SaveManager.NowData.GameOptions.Difficulty != 1)
				{
					ally.Overload++;
				}
			}

			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				int i = 0;
				var ciList = instructions.ToList();
				var c = ciList.Count();

				bool notCountInject = false;
				foreach (var ci in instructions)
				{
					if (ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(Skill), "get_NotCount")))
					{
						Debug.Log("deez2");
						notCountInject = true;

						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SwiftnessReworkPlugin), nameof(SwiftnessReworkPlugin.CheckQuick)));
					}
					else if (ci.Is(OpCodes.Callvirt, AccessTools.Method(typeof(BattleChar), "set_Overload")) && notCountInject)
					{
						Debug.Log("deez3");

						yield return new CodeInstruction(OpCodes.Pop);
						yield return new CodeInstruction(OpCodes.Pop);
					}
					else if (ci.Is(OpCodes.Ldfld, AccessTools.Field(typeof(Skill), nameof(Skill.BasicSkill))))
					{
						Debug.Log("deez4");

						yield return new CodeInstruction(OpCodes.Dup);
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Swiftness2IgnoreOverlaodPatch), 
							nameof(Swiftness2IgnoreOverlaodPatch.IcreaseOverload)));
						yield return ci;
					}
					else
					{
						yield return ci;
					}
					i++;
				}
			}
		}



        //[HarmonyPatch(typeof(SkillButton), nameof(SkillButton.ChoiceSkill))] quick
    }
}
