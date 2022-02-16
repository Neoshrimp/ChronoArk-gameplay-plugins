using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Select_skills_on_Normal
{
    [BepInPlugin(GUID, "Skill select on Normal difficulty", version)]
    [BepInProcess("ChronoArk.exe")]
    public class SelectSkillsOnNormalPlugin : BaseUnityPlugin
    {

        public const string GUID = "org.neo.ca.qol.selectSkillsOnNormal";
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


        [HarmonyPatch]
        class SelectTwoSkillsPatch
        {

            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(Character), nameof(Character.Set_AllyData));
                yield return AccessTools.Method(typeof(CharacterWindow), nameof(CharacterWindow.Upgrade));
            }

            static MethodInfo get_Difficalty_method = AccessTools.PropertyGetter(typeof(SaveManager), "Difficalty");
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var list = instructions.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    // works as long as expert is difficulty 2
                    // could be trouble if there are more expert difficulty checks in these methods
                    if (list[i].opcode == OpCodes.Call && ((MethodInfo)list[i].operand).Equals(get_Difficalty_method) && i+1 < list.Count && list[i+1].opcode == OpCodes.Ldc_I4_2)
                    {
                        list[i].opcode = OpCodes.Ldc_I4_2;
                        list[i].operand = null;
                    }
                }
                return list.AsEnumerable();

            }
        }


    }
}
