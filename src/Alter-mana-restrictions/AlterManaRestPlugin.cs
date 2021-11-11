using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
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

        private static int firstRest = 2;
        private static int secondRest = 3;


        void Awake()
        {

            if (firstRest > secondRest)
            {
                secondRest = firstRest + 1;
            }
            logger = Logger;
            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }

        //TODO
        //add config file
        //test odd values
        //maybe display requirement tooltip even if soulstone condition isn't met

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
        }

    }
}
