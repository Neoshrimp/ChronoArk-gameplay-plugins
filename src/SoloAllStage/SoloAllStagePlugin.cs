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
using DarkTonic.MasterAudio;
using I2.Loc;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UseItem;
using Debug = UnityEngine.Debug;

namespace PlayableLucy
{
    [BepInPlugin(GUID, "SoloAllStage", version)]
    [BepInProcess("ChronoArk.exe")]
    public class PlayableLucyPlugin : BaseUnityPlugin
    {

        public const string GUID = "catrice.SoloAllStage";
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

        [HarmonyPatch(typeof(SR_Solo), nameof(SR_Solo.GameSetting))]
        class SoloSettingPatch
        {
            static void Postfix(SR_Solo __instance)
            {
                __instance.RuleChange.EndisStage4 = false;
            }
        }

        [HarmonyPatch(typeof(B_S4_King_minion_0_0_T), nameof(B_S4_King_minion_0_0_T.Init))]
        class KingTighteningChainFix
        {
            static void Postfix(B_S4_King_minion_0_0_T __instance)
            {
                if (PlayData.TSavedata.SpRule != null && __instance.BChar.MyTeam.AliveChars.Count < 2)
                {
                    __instance.PlusStat.Stun = false;
                    __instance.PlusStat.spd = -10;
                    __instance.PlusStat.dod = -50f;
                    __instance.PlusStat.cri = -100f;
                }
            }
        }

        [HarmonyPatch(typeof(BattleSystem), nameof(BattleSystem.BattleEnd))]
        class BattleEndPatch
        {
            static void Prefix(SR_Solo __instance)
            {
                if (PlayData.TSavedata.StageNum == 5 && PlayData.TSavedata.SpRule != null && PlayData.TSavedata.Party.Count == 1)
                {
                    PlayData.TSavedata.SpRule.ChellangeClear();
                    PlayData.TSavedata.SpRule.ChellangeClearUnlockAndReward();
                }
            }
        }

        [HarmonyPatch(typeof(SkillBookCharacter_Rare), nameof(SkillBookCharacter_Rare.Use))]
        class RareSkillBookPatch
        {
            static void Postfix(SkillBookCharacter_Rare __instance, ref bool __result)
            {
                if (__result == false && PlayData.TSavedata.SpRule != null && PlayData.TSavedata.Party.Count == 1)
                {
                    List<string> list = new List<string>();
                    list.Add(PlayData.LucyRandomSkill());
                    list.Add(PlayData.LucyRandomSkill());
                    list.Add(PlayData.LucyRandomSkill());
                    List<Skill> list2 = new List<Skill>();
                    foreach (string key in list)
                    {
                        list2.Add(Skill.TempSkill(key, PlayData.BattleLucy, PlayData.TempBattleTeam));
                    }
                    FieldSystem.DelayInput(BattleSystem.I_OtherSkillSelect(list2, new SkillButton.SkillClickDel(__instance.SkillAdd), ScriptLocalization.System_Item.SkillAdd, false, true, true, true, false));
                    MasterAudio.PlaySound("BookFlip", 1f, null, 0f, null, null, false, false);
                    __result = true;
                }
            }
        }
    }
}
