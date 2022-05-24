using BepInEx;
using GameDataEditor;
using HarmonyLib;
using I2.Loc;
using System.Collections.Generic;
using UnityEngine;

namespace Eve_NO
{
    [BepInPlugin(GUID, "Eve NOOO", version)]
    [BepInProcess("ChronoArk.exe")]
    public class Eve_NO_Plugin : BaseUnityPlugin
    {

        public const string GUID = "org.neo.ca.memes.eveNo";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);


        void Awake()
        {
            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }

        [HarmonyPatch(typeof(GDESkillData), nameof(GDESkillData.LoadFromDict))]
        class SacrificeNamePatch
        {
            static void Postfix(GDESkillData __instance)
            {
                if (__instance.Key == GDEItemKeys.Skill_S_Sizz_4)
                {
                    if (LocalizationManager.CurrentLanguageCode == "en")
                    {
                        __instance.Name = "Eve, NOOOOOOOO!";
                    }
                    else if (LocalizationManager.CurrentLanguageCode == "ja")
                    {
                        __instance.Name = "Yamero, Eve-chan!";

                    }

                }
            }
        
        }
        

        [HarmonyPatch(typeof(GDEBuffData), nameof(GDEBuffData.LoadFromDict))]
        class SacrifeDeBuffPatch
        {
            static void Postfix(GDEBuffData __instance, Dictionary<string, object> dict)
            {
                if (__instance.Key == GDEItemKeys.Buff_B_Sizz_4_S)
                {
                    if (LocalizationManager.CurrentLanguageCode == "en")
                    {
                        dict.TryGetString("Description", out string ogDesc, GDEItemKeys.Buff_B_Sizz_4_S);

                        __instance.Description = ogDesc + "\n\n<i>you bastard..</i>";
                    }
                }
            }

        }

        [HarmonyPatch(typeof(Extended_Sizz_4), nameof(Extended_Sizz_4.SkillUseSingle))]
        class SacrificeBarksPatch
        {


            static void Prefix(ref HashSet<BattleAlly> __state)
            {
                __state = new HashSet<BattleAlly>();
                foreach (BattleAlly battleAlly in BattleSystem.instance.AllyList)
                {
                    if (battleAlly.BuffFind(GDEItemKeys.Buff_P_Sizz_0, false) || battleAlly.BuffFind(GDEItemKeys.Buff_B_Sizz_10_T, false))
                    {
                        __state.Add(battleAlly);
                    }
                }
            }

            static void Postfix(ref HashSet<BattleAlly> __state)
            {
                var currentEveUsers = new HashSet<BattleAlly>();
                foreach (BattleAlly battleAlly in BattleSystem.instance.AllyList)
                {
                    if (battleAlly.BuffFind(GDEItemKeys.Buff_P_Sizz_0, false) || battleAlly.BuffFind(GDEItemKeys.Buff_B_Sizz_10_T, false))
                    {
                        currentEveUsers.Add(battleAlly);
                    }
                }

                BattleAlly caster = null;

                if (__state != null)
                {
                    __state.ExceptWith(currentEveUsers);
                    if (__state.Count > 0)
                    {

                        var list = new List<BattleAlly>();
                        list.AddRange(__state);
                        caster = list.Random();
                        //Debug.Log("caster " + caster.Info.KeyData);
                    }
                }
                else
                {
                    caster = BattleSystem.instance.AllyList.Random();
                }
                if (caster != null)
                {
                    Barks.RollVoiceLine(Barks.CreateVoiceLines(null, caster))?.dialog.Invoke();
                }
            }


        }
    }


}




