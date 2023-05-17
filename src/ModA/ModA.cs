using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ModANamespace
{
    [BepInPlugin(GUID, "moda", version)]
    [BepInProcess("ChronoArk.exe")]
    public class ModA : BaseUnityPlugin
    {

        public const string GUID = "moda";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        public static BepInEx.Logging.ManualLogSource logger;


        void Awake()
        {
            logger = Logger;

            logger.LogInfo("mod A");
            var qualifiedName = typeof(ModB.ModB.MyType).AssemblyQualifiedName;
            logger.LogInfo(qualifiedName);
            var type = Type.GetType(qualifiedName);
            logger.LogInfo(type);

            if (!ReferenceEquals(type, null))
            {
                var instance = (ModB.ModB.MyType)Activator.CreateInstance(type);
                logger.LogInfo(instance.name);
            }

            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }




    }
}
