using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using I2.Loc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Alter_mana_restrictions
{
    [BepInPlugin(GUID, "Alter mana restrictions", version)]
    [BepInProcess("ChronoArk.exe")]
    public class AlterManaRestPlugin : BaseUnityPlugin
    {

        public const string GUID = "org.neo.gameplay.moremana";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;

        private static int firstRest;
        private static int secondRest;

        private static ConfigFile configFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "neo." + "org.neo.gameplay.moremana".Split('.').Last() + ".cfg"), true);
        private static ConfigEntry<int> firstManaRestriction;
        private static ConfigEntry<int> secondManaRestriction;

        private static string tooltip1 = ScriptLocalization.UI.MPUpLv_1;
        private static string tooltip2 = ScriptLocalization.UI.MPUpLv_2;



        void Awake()
        {

            firstManaRestriction = configFile.Bind("Restriction config", "first restriction mana value", -1, "Mana value where 3 characters at level 2+ restriction will be applied. Example, setting to 3 won't allow any mana level ups until each of the 3 characters reach level 2. Set below 3 to disable");
            secondManaRestriction = configFile.Bind("Restriction config", "second restriction mana value", -1, "Mana value where 4 characters at level 3+ restriction will be applied. Set below 3 to disable. Should be higher than first restriction unless disabled");

            firstRest = Mathf.Max(firstManaRestriction.Value - 3, -1);
            secondRest = Mathf.Max(secondManaRestriction.Value - 3, -1);

            if (firstRest >= secondRest && secondRest >= 0)
            {
                secondRest = firstRest + 1;
            }
            logger = Logger;

            setTooltips();

            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }
\

        static void setTooltips()
        {
            if (LocalizationManager.CurrentLanguage == "English")
            {
                tooltip1 = "You must have at least 3 party members at or above level 2.";
                tooltip2 = "You must have 4 party members at or above level 3.";
            }
            else
            {
                tooltip1 = ScriptLocalization.UI.MPUpLv_1;
                tooltip2 = ScriptLocalization.UI.MPUpLv_2;
            }
        }

        [HarmonyPatch]
        class ManaTooptipPatch
        {
            static MethodBase TargetMethod()
            {
                return AccessTools.PropertySetter(typeof(LocalizationManager), "CurrentLanguage");
            }
            static void Postfix()
            {
                setTooltips();
            }
        }


        [HarmonyPatch(typeof(CharStatV3), "Update")]
        class ManaPatch
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                CodeInstruction prevInst = null;
                int count = 0;
                foreach (CodeInstruction i in instructions)
                {
                    if (prevInst != null && prevInst.opcode == OpCodes.Ldfld && prevInst.operand.ToString() == "System.Int32 AP"
                        && i.opcode == OpCodes.Ldc_I4_1)
                    {
                        //Debug.Log(count + ": " + prevInst.operand);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, firstRest);

                    }
                    else if (prevInst != null && prevInst.opcode == OpCodes.Ldfld && prevInst.operand.ToString() == "System.Int32 AP"
                        && i.opcode == OpCodes.Ldc_I4_2)
                    {
                        //Debug.Log(count + ": " + prevInst.operand);
                        yield return new CodeInstruction(OpCodes.Ldc_I4, secondRest);
                    }
                    else
                    {
                        yield return i;
                    }
                    count++;
                    prevInst = i;

                }
            }

            static void Postfix(CharStatV3 __instance)
            {
                if (PlayData.TSavedata.SoulUpgrade.AP == firstRest && PlayData.TSavedata.Party.FindAll((Character a) => a.LV >= 2).Count < 3)
                {
                    __instance.MPTooltip.enabled = true;
                    __instance.MPTooltip.TooltipString = tooltip1;


                }
                else if (PlayData.TSavedata.SoulUpgrade.AP == 2 && PlayData.TSavedata.Party.Find((Character a) => a.LV <= 2) != null)
                {
                    __instance.MPTooltip.enabled = true;
                    __instance.MPTooltip.TooltipString = tooltip2;
                }
            }
        }

    }
}
