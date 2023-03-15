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
    [BepInPlugin(HarmonyContainer.GUID, "View deck and discard", HarmonyContainer.version)]
    [BepInProcess("ChronoArk.exe")]
    public class BepisPlugin : BaseUnityPlugin
    {

        

        private static BepInEx.Logging.ManualLogSource logger;

       
        void Awake()
        {
            logger = Logger;


            if(WorkshopPlugin.attachObject == null)
                gameObject.AddComponent<ViewDeck>();


        }



    }
}
