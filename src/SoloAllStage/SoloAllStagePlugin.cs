using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace PlayableLucy
{
    [BepInPlugin(GUID, "SoloAllStage", version)]
    [BepInProcess("ChronoArk.exe")]
    public class PlayableLucyPlugin : BaseUnityPlugin
    {

        public const string GUID = "catrice.SoloAllStage";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;


        void Awake()
        {
            logger = Logger;
            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }

        [HarmonyPatch(typeof(SR_Solo), nameof(SR_Solo.GameSetting))]
        class SoloSettingPatch
        {
            static void Postfix(SR_Solo __instance)
            {
                __instance.RuleChange.EndisStage4 = false;
            }
        }
        




    }
}
