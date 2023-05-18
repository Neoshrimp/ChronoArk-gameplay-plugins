using BepInEx;
using BepInEx.Configuration;
using GameDataEditor;
using HarmonyLib;
using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;
using System.Reflection;
using System.Linq;
using System.Reflection.Emit;
using ChronoArkMod.Plugin;
using Mono.Cecil;

namespace ViewDeck
{
    public class ViewDeck : MonoBehaviour
    {



        void Awake()
        {
            HarmonyContainer.PatchAll();


        }

        void OnDestroy()
        {
            HarmonyContainer.UnpatchAll();
        }

        //static List<Skill> frozenDeck;
        //static HashSet<Skill> frozenContent;


        static void ShowDeck()
        {

            if (BattleSystem.instance != null)
            {
                showingDeck = true;
                var deckCopy = new List<Skill>(BattleSystem.instance.AllyTeam.Skills_Deck);

                BattleSystem.DelayInput(BattleSystem.I_OtherSkillSelect(Misc.Shuffle(deckCopy), DeckDelegate, "Deck in random order\n[click card to close]"));
                if (BattleSystem.instance.AllyTeam.Skills_Deck.Count == 0)
                    showingDeck = false;

            }

        }


        static void ShowDiscard()
        {
            if (BattleSystem.instance != null)
            {
                showingDiscard = true;
                BattleSystem.DelayInput(BattleSystem.I_OtherSkillSelect(BattleSystem.instance.AllyTeam.Skills_UsedDeck, DiscardDelegate, "Discard pile\n[click card to close]"));
                if (BattleSystem.instance.AllyTeam.Skills_UsedDeck.Count == 0)
                    showingDiscard = false;

            }
        }

        public static void DeckDelegate(SkillButton Mybutton) { showingDeck = false; }
        public static void DiscardDelegate(SkillButton Mybutton) { showingDiscard = false; }



        static bool showingDeck = false;
        static bool showingDiscard = false;
        static SkillButtonMain pileViewButton;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (!showingDeck && !showingDiscard)
                {
                    ShowDeck();
                }
                // toggle deck view
                else if (showingDeck && pileViewButton != null)
                {
                    pileViewButton.SkillbuttonBig.Click();
                }
                // switch to discard view
                else if (showingDiscard && pileViewButton != null)
                {
                    pileViewButton.SkillbuttonBig.Click();
                    ShowDeck();
                }
            }


            if (Input.GetKeyDown(KeyCode.G))
            {
                if (!showingDeck && !showingDiscard)
                {
                    ShowDiscard();

                }
                // toggle deck view
                else if (showingDiscard && pileViewButton != null)
                {
                    pileViewButton.SkillbuttonBig.Click();
                }
                // switch to discard view
                else if (showingDeck && pileViewButton != null)
                {
                    pileViewButton.SkillbuttonBig.Click();
                    ShowDiscard();
                }
            }

        }



        [HarmonyPatch(typeof(BattleSystem), nameof(BattleSystem.BattleInit))]
        class BattleSystem_Patch
        {
            static void Prefix()
            {
                showingDeck = false;
                showingDiscard = false;
                pileViewButton = null;
            }
        }


        [HarmonyPatch(typeof(SelectSkillList), nameof(SelectSkillList.NewSelectSkillList))]
        class SelectSkillList_Patch
        {


            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                int i = 0;
                var ciList = instructions.ToList();
                var c = ciList.Count();
                CodeInstruction prevCi = null;
                foreach (var ci in instructions)
                {
                    if (ci.opcode == OpCodes.Ldloc_S && prevCi.opcode == OpCodes.Stloc_S && ciList[Math.Max(0, i - 4)].opcode == OpCodes.Ldloc_0)
                    {
                        yield return ci;
                        yield return new CodeInstruction(OpCodes.Dup);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SelectSkillList_Patch), nameof(SelectSkillList_Patch.AssignButton)));

                    }
                    else
                    {
                        yield return ci;
                    }
                    prevCi = ci;
                    i++;
                }
            }


            static void AssignButton(GameObject go)
            {
                pileViewButton = go.gameObject.GetComponent<SkillButtonMain>();
            }
        }




        [HarmonyPatch(typeof(SimpleTooltip), "Start")]
        class ClickableIcons_Patch
        {
            static void Postfix(SimpleTooltip __instance)
            {

                if (__instance.gameObject.name == "Image" && __instance.transform.parent.gameObject.name == "Deck")
                {

                    __instance.TooltipString = ScriptLocalization.UI_Battle_Tooltip.Deck;
                    __instance.ToolTipString_l2.mTerm = ScriptLocalization.UI_Battle_Tooltip.Deck;

                    var viewBehaviour = __instance.gameObject.AddComponent<OnClickView>();
                    viewBehaviour.action = ShowDeck;
                }

                else if (__instance.gameObject.name == "Image" && __instance.transform.parent.gameObject.name == "Trash")
                {
                    __instance.TooltipString = ScriptLocalization.UI_Battle_Tooltip.TrashDeck;
                    __instance.ToolTipString_l2.mTerm = ScriptLocalization.UI_Battle_Tooltip.TrashDeck;

                    var viewBehaviour = __instance.gameObject.AddComponent<OnClickView>();
                    viewBehaviour.action = ShowDiscard;
                }
            }

        }

        public delegate void SingleFunc();

        public class OnClickView : MonoBehaviour, IPointerClickHandler
        {
            public SingleFunc action;

            public void OnPointerClick(PointerEventData eventData)
            {
                if (!showingDeck && !showingDiscard)
                {
                    action.Invoke();
                }
            }
        }


        [HarmonyPatch]
        class DeckToolTip_Patch
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.PropertyGetter(typeof(ScriptLocalization.UI_Battle_Tooltip), "Deck");
            }


            static void Postfix(ref string __result)
            {
                __result = __result + "\n Press [[F]] to view.";
            }
        }

        [HarmonyPatch]
        class DiscardToolTip_Patch
        {


            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.PropertyGetter(typeof(ScriptLocalization.UI_Battle_Tooltip), "TrashDeck");
            }


            static void Postfix(ref string __result)
            {
                __result = __result + "\n Press [[G]] to view.";
            }
        }


    }
}
