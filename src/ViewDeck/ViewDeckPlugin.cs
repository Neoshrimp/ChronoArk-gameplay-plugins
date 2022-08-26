using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NewNamespace
{
    [BepInPlugin(GUID, "View deck and discard", version)]
    [BepInProcess("ChronoArk.exe")]
    public class ViewDeckPlugin : BaseUnityPlugin
    {

        public const string GUID = "neo.ca.qol.viewDeck";
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


/*        static List<Skill> frozenDeck;
        static HashSet<Skill> frozenContent;*/

        static void ShowDeck()
        {

            if (BattleSystem.instance != null && BattleSystem.instance.AllyTeam.Skills_Deck.Count != 0)
            {
                showingDeck = true;
                var deckCopy = new List<Skill>(BattleSystem.instance.AllyTeam.Skills_Deck);
                BattleSystem.DelayInput(BattleSystem.I_OtherSkillSelect(Misc.Shuffle(deckCopy), DeckDelegate, "Deck in random order\n[click card to close]"));
            }

        }


        static void ShowDiscard()
        {
            if (BattleSystem.instance != null && BattleSystem.instance.AllyTeam.Skills_UsedDeck.Count != 0)
            {
                showingDiscard = true;
                BattleSystem.DelayInput(BattleSystem.I_OtherSkillSelect(BattleSystem.instance.AllyTeam.Skills_UsedDeck, DiscardDelegate, "Discard pile\n[click card to close]"));
            }
        }

        public static void DeckDelegate(SkillButton Mybutton) { showingDeck = false; }
        public static void DiscardDelegate(SkillButton Mybutton) { showingDiscard = false; }



        static bool showingDeck = false;
        static bool showingDiscard = false;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F) && !showingDeck && !showingDiscard)
            {
                ShowDeck();
            }
            if (Input.GetKeyDown(KeyCode.G) && !showingDeck && !showingDiscard)
            {
                ShowDiscard();
            }

        }



        [HarmonyPatch(typeof(BattleSystem), nameof(BattleSystem.BattleInit))]
        class BattleSystem_Patch
        {
            static void Prefix()
            {
                showingDeck = false;
                showingDiscard = false;
            }
        }


        // 2do doesn't work
        //[HarmonyPatch]
        class DeckToolTip_Patch
        {


            static IEnumerable<MethodBase> TargetMethods()
            {

                yield return AccessTools.PropertyGetter(typeof(ScriptLocalization.UI_Battle_Tooltip), "Deck");
            }


            static void Postfix(ref string __result)
            {
                Debug.Log("deck tooltip");
                __result = __result + "\n Press [[F]] to view.";
            }
        }

        //[HarmonyPatch]
        class DiscardToolTip_Patch
        {


            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.PropertyGetter(typeof(ScriptLocalization.UI_Battle_Tooltip), "TrashDeck");
            }


            static void Postfix(ref string __result)
            {
                Debug.Log("disc tooltip");

                __result = __result + "\n Press [[G]] to view.";
            }
        }

    }
}
