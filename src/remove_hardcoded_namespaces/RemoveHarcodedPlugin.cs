using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RemoveHarcoded
{
    [BepInPlugin(GUID, "", version)]
    [BepInProcess("ChronoArk.exe")]
    public class RemoveHarcodedPlugin : BaseUnityPlugin
    {

        public const string GUID = "remove_hard_coded_namespaces";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;


        public class MyType
        {
            string name = "deez";
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


        [HarmonyPatch] 
        class RemoveHardCodedNameSpaces_Patch
        {

            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(EquipCurse), nameof(EquipCurse.NewCurse));
                yield return AccessTools.Method(typeof(ItemEnchant), nameof(ItemEnchant.NewEnchant));
                yield return AccessTools.Method(typeof(Item_Active), nameof(Item_Active.InputInfo));
                yield return AccessTools.Method(typeof(Item_Consume), nameof(Item_Consume.Use));
                yield return AccessTools.Method(typeof(Item_Equip), nameof(Item_Equip.InputInfo));
                yield return AccessTools.Method(typeof(Item_Passive), nameof(Item_Passive.InputInfo));
                yield return AccessTools.Method(typeof(Item_Potions), nameof(Item_Potions.Use));
                yield return AccessTools.Method(typeof(Item_Scroll), nameof(Item_Scroll.Use));
            }


            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {


                var namePrefixes = new HashSet<string>() { "Curse.", "Enchent.", "AItem.", "UseItem.", "EItem.", "PItem.", "Potions.", "Scrolls."};

                foreach (var ci in instructions)
                {
                    if (ci.opcode == OpCodes.Ldstr && namePrefixes.Contains((string)ci.operand))
                    {
                        yield return new CodeInstruction(OpCodes.Ldstr, "");

                    }
                    else 
                    {
                        yield return ci;
                    }



                }
            }



        }



    }
}
