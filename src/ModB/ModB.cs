using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ModB
{
    [BepInPlugin(GUID, "", version)]
    [BepInProcess("ChronoArk.exe")]
    public class ModB : BaseUnityPlugin
    {

        public const string GUID = "modb";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;


        public class MySkill_Extended : Skill_Extended
        {
            
        }

        public class MyType
        {
            public string name = "deez";
        }


        void Awake()
        {
            logger = Logger;
            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }




    }
}
