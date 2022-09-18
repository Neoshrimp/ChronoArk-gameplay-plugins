using BepInEx;
using BepInEx.Configuration;
using DarkTonic.MasterAudio;
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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Recruit_Clones
{
    [BepInPlugin(GUID, "Recruit Clones", version)]
    [BepInProcess("ChronoArk.exe")]
    public class Recruit_ClonesPlugin : BaseUnityPlugin
    {

        public const string GUID = "org.neo.ca.memes.recruitClones";
        public const string version = "0.0.1";


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
                harmony.UnpatchSelf();
        }


        [HarmonyPatch(typeof(CharacterButton), nameof(CharacterButton.OnPointerClick))]
        class CharacterButtonPatch
        {
            static AccessTools.FieldRef<CharacterButton, int> MynumRef = AccessTools.FieldRefAccess<int>(typeof(CharacterButton), "Mynum");

            static bool Prefix(CharacterButton __instance, PointerEventData eventData)
            {

                if (__instance.main.SelectNum <= 1)
                {
                    return true;
                }
                if (!__instance.Lock)
                {
                    __instance.main.InfoView(MynumRef(__instance));
                    __instance.main.NowSelectedName = __instance.CharName;
                    if (eventData.button == PointerEventData.InputButton.Right)
                    {

                        MasterAudio.PlaySound("SE_ClickButton", 1f, null, 0f, null, null, false, false);
                        __instance.main.SelectLeftBtn(__instance.Index);
                        __instance.main.SelectRightBtn(__instance.Index);
                        __instance.main.Select();
                    }
                    else if (eventData.button == PointerEventData.InputButton.Left)
                    {
                        __instance.main.SelectLeftBtn(__instance.Index);
                    }
                }
                return false;
            }
        }


        //2do doesn't inject with current version of ca
        [HarmonyPatch(typeof(StartPartySelect), nameof(StartPartySelect.Init))]
        class PartySelectPatch
        {

            static List<GDECharacterData> AddCharacters(List<GDECharacterData> list)
            {
                GDEDataManager.GetAllDataKeysBySchema(GDESchemaKeys.Character, out List<string> allChars);

                foreach (var char2Add in allChars)
                {
                    int odds = 4;
                    var charFound = PlayData.TSavedata.DonAliveChars.FindAll(k => k == char2Add);
                    if (charFound.Count != 0)
                    {
                        odds = 40*charFound.Count;
                    }

                    for (int i = 0; i < odds; i++)
                    {
                        list.Add(new GDECharacterData(char2Add));
                    }
                }
                return list;
            }
            static List<GDECharacterData> RemoveMultiple(List<GDECharacterData> list, GDECharacterData charData)
            {
                list.RemoveAll(cd => cd.Key == charData.Key);
                return list;
            }



            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                int i = 0;

                var ciList = instructions.ToList();
                var c = ciList.Count();
                foreach (var ci in instructions)
                {
                    if (ci.opcode == OpCodes.Newobj && ((ConstructorInfo)ci.operand).Equals(AccessTools.Constructor(typeof(List<GDECharacterData>)))
                        && ciList[Math.Min(i + 1, c - 1)].opcode == OpCodes.Stloc_S && ((LocalBuilder)ciList[Math.Min(i + 1, c - 1)].operand).LocalIndex == 17
                        )
                    {

                        Debug.Log("deez 1");
                        yield return ci;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PartySelectPatch), nameof(PartySelectPatch.AddCharacters)));
                    }
                    else if (ci.opcode == OpCodes.Callvirt
                        && ((MethodInfo)ci.operand).Equals(AccessTools.Method(typeof(List<GDECharacterData>), "Remove", new Type[] { typeof(GDECharacterData) }))
                        && ciList[Math.Max(i - 2, 0)].opcode == OpCodes.Ldloc_S && ((LocalBuilder)ciList[Math.Max(i - 2, 0)].operand).LocalIndex == 17
                        )
                    {
                        Debug.Log("deez 2");
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PartySelectPatch), nameof(PartySelectPatch.RemoveMultiple)));
                    }
                    else
                    {
                        yield return ci;
                    }
                    i++;
                }
            }
        }



        [HarmonyPatch(typeof(StartPartySelect), "Select")]
        class TwinsPartySelectPatch
        {
            static bool Prefix(StartPartySelect __instance)
            {
                MasterAudio.PlaySound("SE_ClickButton", 1f, null, 0f, null, null, false, false);
                for (int i = __instance.Locked; i < __instance.Selected.Length; i++)
                {
                    if (__instance.Selected[i].CharacterNum == -1)
                    {
                        __instance.Selected[i].Input(__instance.NowSelectedVIew);
                        break;
                    }
                }
                __instance.CBListGrayOn();
                return false;
            }
        }


        [HarmonyPatch(typeof(SelectedAlly), "OutPut")]
        class TwinsSelectedAllyPatch
        {

            static AccessTools.FieldRef<SelectedAlly, bool> IsLockRef = AccessTools.FieldRefAccess<bool>(typeof(SelectedAlly), "IsLock");

            static bool Prefix(SelectedAlly __instance)
            {
                if (!IsLockRef(__instance))
                {
                    __instance.Main.CBList[__instance.CharacterNum].BG.GetComponent<Image>().sprite = __instance.Main.CBList[__instance.CharacterNum].NormalBG;
                    __instance.Main.CBList[__instance.CharacterNum].MainFrame.sprite = __instance.Main.CBList[__instance.CharacterNum].OriginFrame;
                    __instance.Main.NowShowNum = -1;
                    int characterNum = __instance.CharacterNum;
                    __instance.CharacterNum = -1;
                    ChildClear.Clear(__instance.FaceImage.transform);
                    MasterAudio.PlaySound("SE_ClickButton", 1f, null, 0f, null, null, false, false);

                    __instance.Main.CBListGrayOff();
                }
                return false;
		    }
        }

        [HarmonyPatch(typeof(P_Phoenix), "FixedUpdate")]
        class MakePhoenixMortalPatch
        {
            static void Postfix(P_Phoenix __instance)
            {
                if (__instance.BChar.HP < -__instance.BChar.GetStat.maxhp*2)
                {
                    __instance.BChar.Dead();
                }
            }
        }

    }
}
