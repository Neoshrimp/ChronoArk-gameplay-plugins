﻿using BepInEx;
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
			var defaultQuickness = new HashSet<string>();
			defaultQuickness.Add(GDEItemKeys.Skill_S_Public_9);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_P_0);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_9);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_0);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_2);
			defaultQuickness.Add(GDEItemKeys.Skill_S_Azar_11);

			defaultQuickness.Add(GDEItemKeys.Skill_S_Mement_P);
			defaultQuickness.Add(GDEItemKeys.Skill_S_MessiahbladesPrototype);

			quickManager = new QuickManager(defaultQuickness, 10000);
			harmony.PatchAll();
			cullRoutine = StartCoroutine(CleanFields());
			

		}
		void OnDestroy()
		{
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


    }
}
