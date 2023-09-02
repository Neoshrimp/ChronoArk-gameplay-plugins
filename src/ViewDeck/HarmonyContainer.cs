using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;

namespace ViewDeck
{
    public class HarmonyContainer
 
    {
        public const string GUID = "neo.ca.qol.viewDeck";

        public const string version = "1.0.0";

        public const string name = "ViewDeck";


        public static Harmony harmony;

        public static void PatchAll()
        {
            if (harmony == null)
                harmony = new Harmony(GUID);

            harmony.PatchAll();
        }

        public static void UnpatchAll()
        {
            if (harmony != null)
#if WORKSHOP
                harmony.UnpatchSelf();
#endif
#if BEPINEX
                harmony.UnpatchAll();
#endif
        }

    }
}
