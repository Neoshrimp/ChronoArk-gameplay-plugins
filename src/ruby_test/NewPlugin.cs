using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RemoveHarcoded
{
    [BepInPlugin(GUID, "asdf", version)]
    [BepInProcess("ChronoArk.exe")]
    public class NewPlugin : BaseUnityPlugin
    {

        public const string GUID = "rubydeeznuts";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;


        void Awake()
        {

            /*            
                        Debug.Log(a);*/
            //
            // Debug.Log(typeof(Ruby.S_Ruby_0).AssemblyQualifiedName);

            Paths.DllSearchPaths.ToList().ForEach(p => Debug.Log(p));

            Debug.Log("-------");

            Debug.Log(Path.Combine(Paths.BepInExRootPath, "plugins\\Ruby\\Ruby&NewType.dll"));

            //var a = Assembly.LoadFile(Path.Combine(Paths.PluginPath, "plugins\\Ruby\\Ruby&NewType.dll"));
            var qualifiedName = typeof(Ruby.S_Ruby_1).AssemblyQualifiedName;
            Debug.Log(qualifiedName);
            var type = Type.GetType(qualifiedName);
            Debug.Log(type);
            var skill_ex = Skill_Extended.DataToExtendedC(qualifiedName);
            Debug.Log(skill_ex);


            logger = Logger;
            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }


        [HarmonyPatch(typeof(SaveManager), "Awake")]
        class SaveManager_Patch
        {
            static void Prefix()
            {
                Debug.Log("Awake prefix");
            }
            static void Postfix()
            {
                Debug.Log("Awake POSTfix");
            }
        }






    }
}
