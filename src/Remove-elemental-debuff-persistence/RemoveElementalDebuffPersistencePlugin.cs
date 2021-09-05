using BepInEx;
using GameDataEditor;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Remove_elemental_debuff_persistence
{
    [BepInPlugin(GUID, "No more elemental trio debuff persistence", version)]
    [BepInProcess("ChronoArk.exe")]
    public class RemoveElementalDebuffPersistencePlugin : BaseUnityPlugin
    {

        public const string GUID = "org.neo.chronoark.qolmods.triodebuffnerfp";
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

        [HarmonyPatch(typeof(B_S4_Guard_0_0_T), nameof(B_S4_Guard_0_0_T.Init))]
        class ElectricShockNerfPatch
        {
            static void Postfix(B_S4_Guard_0_0_T __instance)
            {
                __instance.BuffData.IsFieldBuff = false;
            }
        }

        [HarmonyPatch(typeof(Buff), nameof(Buff.Init))]
        class BurtNerfPatch
        {
            static void Postfix(Buff __instance)
            {
                if(__instance.BuffData.Key == GDEItemKeys.Buff_B_S4_Guard_1_0_T)
                    __instance.BuffData.IsFieldBuff = false;
            }
        }
    }
}
