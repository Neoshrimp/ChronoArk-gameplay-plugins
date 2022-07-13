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
using Debug = UnityEngine.Debug;

namespace PlayableLucy
{
    [BepInPlugin(GUID, "Playable Lucy", version)]
    [BepInProcess("ChronoArk.exe")]
    public class PlayableLucyPlugin : BaseUnityPlugin
    {

        public const string GUID = "neo.ca.qol.playableLucy";
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

        [HarmonyPatch(typeof(GDECharacterData), nameof(GDECharacterData.LoadFromDict))]
        class GDECharacterPatch
        {
            static void Postfix(GDECharacterData __instance)
            {
                if (__instance.Key == GDEItemKeys.Character_LucyC)
                {
                    __instance.Off = false;
                }
            }
        }

        [HarmonyPatch]
        class ExtendSomePoolsWithLucySkills
        {

            static IEnumerable<MethodBase> TargetMethods()
            {
                // error_505
                yield return AccessTools.Method(typeof(CharacterWindow), nameof(CharacterWindow.GetRandomSkill));
                yield return AccessTools.Method(typeof(RE_EatingKnowledge), nameof(RE_EatingKnowledge.SkillReturn));
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {

                CodeInstruction prevCi = null;
                foreach (var ci in instructions)
                {
                    if (ci.opcode == OpCodes.Call && prevCi != null && 
                        prevCi.Is(OpCodes.Ldsfld, AccessTools.Field(typeof(GDEItemKeys), nameof(GDEItemKeys.Character_LucyC))))
                    {
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Pop);
                        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                    }
                    else
                    {
                        yield return ci;
                    }
                    prevCi = ci;    
                }
            }
        }




    }
}
