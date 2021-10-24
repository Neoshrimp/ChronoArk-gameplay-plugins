using BepInEx;
using HarmonyLib;
using GameDataEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Configuration;

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
    //tweak expert tooltip

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
