using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Randomizer
{
    [BepInPlugin(GUID, "Randomizer", version)]
    [BepInProcess("ChronoArk.exe")]
    public class New_CA_Plugin_Template1 : BaseUnityPlugin
    {

        public const string GUID = "org.neo.chronoark.runmutators.randomizer";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;

        //TODO
        //random chars
        //first screen
        //campfires
        //random skill
        //statues
        //neutral skill books?\
        //add ui button "Random"
        //maybe add animations


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

        // Fisher–Yates shuffle
        private static void KnuthShuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, list.Count); // Don't select from the entire array on subsequent loops
                T temp = list[i]; list[i] = list[j]; list[j] = temp;
            }
        }

        // skill books and level ups
        [HarmonyPatch(typeof(BattleSystem), nameof(BattleSystem.I_OtherSkillSelect))]
        class RandomCard
        {
            static void Prefix(ref List<Skill> Skills, ref SkillButton.SkillClickDel Delegate, ref bool HideButtonView)
            {

                /*                void SkillAdd(SkillButton Mybutton)
                                {
                                    Mybutton.interactable = false;
                                    Mybutton.Myskill.Master.Info.UseSoulStone(Mybutton.Myskill);
                                    UIManager.inst.CharstatUI.GetComponent<CharStatV3>().SkillUPdate();
                                }
                                HideButtonView = false;*/
                if (BattleSystem.instance == null)
                {
                    //Delegate = new SkillButton.SkillClickDel(SkillAdd);
                    /*                    public static IEnumerator I_OtherSkillSelect(List<Skill> Skills, SkillButton.SkillClickDel Delegate, string Desc, bool back = false, bool ManaView = true, bool HideButtonView = true)
                                        {
                                            List<Skill> Tempskills = new List<Skill>();
                                            Tempskills.AddRange(Skills);
                                            if (Tempskills.Count != 0)
                                            {
                                                BattleSystem.OhterSkillSelect(Tempskills, Delegate, Desc, back, ManaView, HideButtonView);
                                                while (!Misc.NullCheck(UIManager.NowActiveUI) && UIManager.NowActiveUI.gameObject != UIManager.inst.CharstatUI)
                                                {
                                                    yield return new WaitForSeconds(0.2f);
                                                }
                                            }
                                            yield break;
                                        }*/
                }
            }
        }

        static void EmptyClick(SkillButton sb)
        {
            return;
        }


        [HarmonyPatch(typeof(BattleSystem), "OhterSkillSelect")]
        class RandomSkill : MonoBehaviour
        {
            static RandomSkill ins = new RandomSkill();
            static bool Prefix(List<Skill> Skills, SkillButton.SkillClickDel Delegate, string Desc, bool back = false, bool ManaView = true, bool HideButtonView = true)
            {
                if (BattleSystem.instance == null)
                {

                    SelectSkillList selectSkillList = SelectSkillList.NewSelectSkillList(back, Desc, Skills, Delegate, ManaView, HideButtonView);
                    Skill pickedSkill = Skills[UnityEngine.Random.Range(0, Skills.Count)];
                    int pickedSkillPos = 0;
                    List<SkillButton> skillButtons = new List<SkillButton>();

                    foreach (Transform c in selectSkillList.Align.transform)
                    {
                        SkillButton skillButton = c.gameObject.GetComponent<SkillButtonMain>().Skillbutton;
                        skillButtons.Add(skillButton);
                        if (skillButton.Myskill == pickedSkill)
                        {
                            pickedSkillPos = skillButtons.Count - 1;
                        }
                        skillButton.Ani.SetBool("On", false);
                        //skillButton.ClickDelegate = new SkillButton.SkillClickDel(EmptyClick);

                        /*                        if (skillButton.Myskill != pickedSkill)
                                                {

                                                    skillButton.Ani.SetBool("On", false);
                                                    skillButton.ClickDelegate = new SkillButton.SkillClickDel(EmptyClick);

                                                    Debug.Log(c.gameObject.GetComponent<SkillButtonMain>().Skillbutton.Myskill);

                                                    foreach (Component comp in c.gameObject.GetComponents<Component>())
                                                    {
                                                        Debug.Log(comp);
                                                    }
                                                    //Debug.Log(c.gameObject.GetComponent<SkillButtonMain>().Skillbutton.interactable);
                                                    //c.gameObject.GetComponent<SkillButtonMain>().Skillbutton.AlreadyWasted = true;
                                                    //Debug.Log(c.gameObject.GetComponent<SkillButtonMain>().Skillbutton.interactable);
                                                }*/
                    }
                    //ins.StartCoroutine(AnimRoutine(skillButtons, pickedSkillPos)); 


                    return false;
                }
                return true;

            }

            static IEnumerator AnimRoutine(List<SkillButton> skillButtons, int pickedSkillPos, int cycles = 10, int startPos = 0)
            {
                int len = skillButtons.Count;
                float acc;
                for (int i = startPos; i < cycles * len; i++)
                {
                    skillButtons[i % len].Ani.SetBool("On", true);
                    yield return new WaitForSeconds(0.07f);
                    Debug.Log("hop");
                    //skillButtons[i % len].Ani.SetBool("On", false);

                }
                for (int i = 0; i < pickedSkillPos; i++)
                {
                    skillButtons[i % len].Ani.SetBool("On", true);
                    yield return new WaitForSeconds(0.1f);
                    //skillButtons[i % len].Ani.SetBool("On", false);
                }
                skillButtons[pickedSkillPos].Ani.SetBool("On", true);
                void SkillAdd(SkillButton Mybutton)
                {
                    Mybutton.Myskill.Master.Info.UseSoulStone(Mybutton.Myskill);
                    UIManager.inst.CharstatUI.GetComponent<CharStatV3>().SkillUPdate();
                }
                skillButtons[pickedSkillPos].ClickDelegate = new SkillButton.SkillClickDel(SkillAdd);

            }

        }

        [HarmonyPatch(typeof(SkillButton), nameof(SkillButton.OnPointerEnter), new Type[] { })]
        class skillbtnd
        {
            static void Postfix(SkillButton __instance)
            {
                if (__instance.ClickDelegate.Equals(new SkillButton.SkillClickDel(EmptyClick)))
                {
                    __instance.Ani.SetBool("On", false);
                }
                foreach (FieldInfo fi in AccessTools.GetDeclaredFields(typeof(SkillButton)))
                {
                    Debug.Log(fi.Name + ": " + fi.GetValue(__instance));

                }
                Debug.Log("----------------------------------");

                foreach (Component comp in __instance.GetComponents<Component>())
                {
                    Debug.Log(comp);
                }
                Debug.Log("----------------------------------");
            }
        }



    }
}
