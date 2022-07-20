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
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using I2.Loc;

//2do maybe add localization
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

		public static Coroutine cullRoutine;

		void Awake()
		{
			logger = Logger;


			quickManager = new QuickManager(Skills2AddQuick.defaultQuickness, 10000);
			harmony.PatchAll();
			cullRoutine = StartCoroutine(CleanFields());

		}
		void OnDestroy()
		{
			if(cullRoutine != null)
				StopCoroutine(cullRoutine);
			if (harmony != null)
				harmony.UnpatchAll(GUID);
		}


		IEnumerator CleanFields()
		{
			while (true)
			{
				yield return new WaitForSeconds(20);
				quickManager.CullDestroyed();
			}
		}

		public readonly static string NewSwiftnessName = "Effortless";

		public readonly static string QuickKeyWordKey = GUID + "_QuickKeyWordKey";
		public readonly static string QuickKeyWordName = "Quick";
		public readonly static string QuickKeyWordDesc = "Does not advance enemy action counts.";

		[HarmonyPatch(typeof(GDESkillKeywordData), nameof(GDESkillKeywordData.LoadFromSavedData))]
		class CustomKeywordTooltips
		{
			static void Postfix(GDESkillKeywordData __instance)
			{
				if (__instance.Key == QuickKeyWordKey)
				{
					__instance.Name = "<b>" + QuickKeyWordName + "</b>";
					__instance.Desc = QuickKeyWordDesc;
				}
			}
		}



    }
}
