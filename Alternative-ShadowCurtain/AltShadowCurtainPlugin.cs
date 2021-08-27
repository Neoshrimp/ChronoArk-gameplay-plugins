using BepInEx;
using GameDataEditor;
using HarmonyLib;
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


        [HarmonyPatch(typeof(S_Trisha_5))]
        class Shadow_Curtain_Patch
        {
            static AccessTools.FieldRef<GDESkillData, bool> NoBasicSkillRef = AccessTools.FieldRefAccess<GDESkillData, bool>("_NoBasicSkill");


            [HarmonyPatch(nameof(S_Trisha_5.Init))]
            [HarmonyPrefix]
            static void InitPrefix(Skill ___MySkill)
            {
                // once keyword
                ___MySkill.Disposable = true;
                //removes no fix restriction
                NoBasicSkillRef(___MySkill.MySkill) = false;

                //___MySkill.AP = 10; // mana cost. ___MySkill.MySkill:int _UseAp is 'default' mana cost. It's a private field with no good setter so use AccessTools to modify it
                //_UseAp should have the same value as AP for mana number icon to be displayed in correct colour. However mana cost is still not updated in encyclopedia

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
