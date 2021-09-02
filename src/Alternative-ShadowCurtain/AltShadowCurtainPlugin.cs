using BepInEx;
using GameDataEditor;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Alternative_ShadowCurtain
{
    [BepInPlugin(GUID, "Alt Shadow Curtain", version)]
    [BepInProcess("ChronoArk.exe")]
    public class AltShadowCurtainPlugin : BaseUnityPlugin
    {
        public const string GUID = "org.neo.chronoark.cardmod.shadowcurtain";
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


        // TODO
        // Update db if card is unlocked mid-run. Unlikely to be noticed or significantly affect gameplay. Or figure unified way to add/patch cards.

        // updates some changes in temporary 'database'. Some effects use this skill list to function properly, for example, golem module relic can never create once skills
        // important things to update properly are keywords, mana cost, regular/red skill etc. However, some other changes might be required for skill to be properly integrated.
        // Encyclopedia entries mana cost is still not updated properly even with this addition
        // as far as I know PlayData.DataBaseInit is only called once when game is started from main menu
        [HarmonyPatch(typeof(PlayData), nameof(PlayData.DataBaseInit))]
        class DataBase_Patch
        {
            static void Postfix(List<GDESkillData> ____ALLSKILLLIST)
            {
                GDESkillData shadowCurtain = ____ALLSKILLLIST.Find(x => x.Key == "S_Trisha_5");
                if (shadowCurtain != null)
                {
                    shadowCurtain.NoBasicSkill = false;
                    shadowCurtain.Disposable = true;
                }
            }
        }



        [HarmonyPatch(typeof(S_Trisha_5))]
        class Shadow_Curtain_Patch
        {
            [HarmonyPatch(nameof(S_Trisha_5.Init))]
            [HarmonyPrefix]
            static void InitPrefix(Skill ___MySkill)
            {
                // once keyword
                ___MySkill.Disposable = true;
                //removes no fix restriction
                ___MySkill.MySkill.NoBasicSkill = false;

                //___MySkill.AP = 10; // mana cost. ___MySkill.MySkill:int _UseAp is 'default' mana cost.
                //_UseAp should have the same value as AP for mana number icon to be displayed in correct colour. However mana cost is still not updated in encyclopedia
                //___MySkill.IsWaste = true; // cannot exchange keyword


                //___MySkill.NotCount = true; // swiftness keyword
                //___MySkill.NotChuck = true; // bind keyword(no cycling)
                //___MySkill.isExcept = true; // except keyword (if combined with 'once' only 'except' will be displayed)
            }


            [HarmonyPatch(nameof(S_Trisha_5.DescExtended))]
            [HarmonyPostfix]
            static void DescExtendedPostfix(ref string __result)
            {
                __result = __result.Replace("<b>Exclude after being used 3 times</b>\n", "");
            }
        }


        // removes remaining curtain use count icon and logic
        // each Skill has a List of Skill_Extended which add additional effects or provide card buff/debuffs (i.e. Azar's passive)
        [HarmonyPatch(typeof(Trisha_5_Ex))]
        class Shadow_Curtain_ExtendedIcon_Patch
        {
            [HarmonyPatch(nameof(Trisha_5_Ex.Init))]
            [HarmonyPrefix]
            static void InitPrefix(Trisha_5_Ex __instance)
            {
                __instance.SelfDestroy();
            }
        }

    }
}
