using BepInEx;
using HarmonyLib;
using GameDataEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Configuration;
using I2.Loc;
using System.Reflection;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace Remove_hitcap
{
    [BepInPlugin(GUID, "Remove expert hitcap", version)]
    [BepInProcess("ChronoArk.exe")]
    public class Remove_hitcap_plugin : BaseUnityPlugin
    {

        public const string GUID = "com.neo.chronoark.qolmods.removehitcap";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;

        private static ConfigEntry<bool> removeHitcapForEnemies;

        void Awake()
        {
            removeHitcapForEnemies = Config.Bind("Fairness config", "remove_hitcap_for_enemies", false, "Remove acc cap for enemies too. Vanilla is 98% no matter the difficulty");
            logger = Logger;
            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }


        //TODO
        //language change requires restart maybe? but changing languages mid game is no perfect in vanilla
        //expert icon in party select screen still has old description remaining



        static string ProcessTooltip(string tooltip)
        {
            return new Regex("(?!\n).*?98.*?\n").Replace(tooltip, "");
        }


        [HarmonyPatch]
        class DifficultySelectTooltipPatch
        {
            static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(typeof(ScriptLocalization.UI_SelectParty), "OriginDesc");
            }

            static void Postfix(ref string __result)
            {
                __result = ProcessTooltip(__result);
            }
        }

        [HarmonyPatch(typeof(PartyInventory), "Awake")]
        class ExpertIconPatch
        {
            static void Postfix(PartyInventory __instance)
            {

                if (__instance.ExpertIcon != null)
                {

                    SimpleTooltip tooltip = __instance.ExpertIcon.GetComponent<SimpleTooltip>();
                    if (tooltip != null)
                    {
                        tooltip.TooltipString = ProcessTooltip(tooltip.ToolTipString_l2);
                        tooltip.ToolTipString_l2 = null;
                    }

                }
            }
        }



        // actual logic
        [HarmonyPatch(typeof(BattleChar), nameof(BattleChar.HitPerNum))]
        class HitcapPatch
        {

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                int changeCount = 0;
                int changeLim = 2;
                if (removeHitcapForEnemies.Value)
                    changeLim = 4;

                foreach (CodeInstruction i in instructions)
                {
                    //maybe there's a better way for detecting this
                    if (i.opcode == OpCodes.Ldc_R4 && i.operand.ToString() == "98" && changeCount < changeLim)
                    {
                        yield return new CodeInstruction(OpCodes.Ldc_R4, 100f);
                        changeCount += 1;
                    }
                    else
                    {
                        yield return i;
                    }
                    //Debug.Log( i.opcode + ": " + i.operand);
                }
                //Debug.Log(changeCount);
            }
        }



    }
}
