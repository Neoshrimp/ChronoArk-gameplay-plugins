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
using System;

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

		public static QuickManager quickManager;

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

			//var deez = (Skill_Extended)Activator.CreateInstance(typeof(Extended_Azar_1));

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

			static void Postfix(Skill_Extended __instance)
			{
				quickManager.AddField(__instance, false);
				//Debug.Log(__instance.GetType().Name);
			}
		}

       






        [HarmonyPatch(typeof(Skill), nameof(Skill.initField))]
		class SkillinitFieldPatch
		{
			static void Postfix(Skill __instance)
			{
				if (quickManager.defaultQuickness.Contains(__instance.MySkill.KeyID))
				{
					quickManager.SetVal(__instance, true);
				}
			}
		}



		/*        [HarmonyPatch(typeof(Skill), nameof(Skill.CloneSkill)]
				class Skill_ClonePatch
				{

				}*/
			


		



    }
}
