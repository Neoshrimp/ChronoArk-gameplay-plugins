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
using DarkTonic.MasterAudio;
using Debug = UnityEngine.Debug;


namespace ClassLibrary1
{
    [BepInPlugin(GUID, "Ez CW access", version)]
    [BepInProcess("ChronoArk.exe")]
    public class EasyCWaccess_Plugin : BaseUnityPlugin
    {

        public const string GUID = "neo.ca.3.5test";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;




        void Awake()
        {
            logger = Logger;
            harmony.PatchAll();


            var se1 = new Skill_Extended();
            var se2 = new Skill_Extended();

            Debug.Log(se1 == se2);


        }


        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                MasterAudio.FadeBusToVolume("ArkBGM", 1f, 4f, null, false, false);
            }
        }
    }
}
