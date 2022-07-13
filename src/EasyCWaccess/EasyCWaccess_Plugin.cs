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

namespace EasyCWaccess
{
    [BepInPlugin(GUID, "Ez CW access", version)]
    [BepInProcess("ChronoArk.exe")]
    public class EasyCWaccess_Plugin : BaseUnityPlugin
    {

        public const string GUID = "neo.ca.qol.ezCrimson";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;

        public static ConfigEntry<int> configCWsoulstones;
        public static ConfigEntry<int> configCWkeys;



        void Awake()
        {
            logger = Logger;
            harmony.PatchAll();

            configCWsoulstones = Config.Bind("CW entry cost config", "number of soulstones required", 4, "Number of soulstones required at the event. Vanilla default is 4");
            configCWkeys = Config.Bind("CW entry cost config", "number of keys required", 2, "Number of keys required at the event. Vanilla default is 2");

        }


        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }


        static GameObject CreateCWShortcutObject()
        {

            var re = new GDERandomEventData(RE_cwEntryEventKey);

            var fo = Instantiate(re.EventFieldObject);

            fo.SetActive(false);

            var geo = fo.transform.Find("EventObject (1)").gameObject;
            geo.tag = "EventObject";

            var coll = geo.GetComponent<PolygonCollider2D>();
            var triggerCol = geo.AddComponent<PolygonCollider2D>();

            foreach (var p in AccessTools.GetDeclaredProperties(typeof(PolygonCollider2D)))
            {
                p.SetValue(triggerCol, p.GetValue(coll, null), null);
            }

            triggerCol.isTrigger = true;

            var eo = geo.AddComponent<EventObject>();

            eo.TargetEvent = new UnityEvent();
            eo.TargetEvent.AddListener(OnIteract);
            void OnIteract()
            {
                RandomEventUI component = UIManager.InstantiateActive(UIManager.inst.RandomEventMainUI).GetComponent<RandomEventUI>();
                component.EventInit(RE_cwEntryEventKey, null, eo);
                component.gameObject.transform.SetParent(FieldSystem.instance.RandomEventUITrans);

            }
            eo.TargetEvent_FastMode = new UnityEvent();
            eo.TargetInit = new UnityEvent();
            eo.TargetOnesEnable = new UnityEvent();

            //fucking magic flag which makes event ui destruction work properly 
            eo.In = true;

            fo.transform.localScale *= 0.35f;

            fo.SetActive(true);

            return fo;
        }

        [HarmonyPatch(typeof(HiddenDoor), "Start")]
        class DisableCWAccessObjectsPatch
        {
            static void Postfix(HiddenDoor __instance)
            {
                __instance.MyEventObj.Useless();
            }
        }

        [HarmonyPatch(typeof(FieldSystem), nameof(FieldSystem.CampfireMap))]
        class SpawnSecretPassagePatch
        {

            static void InstantiateSecretPassage(GameObject camp)
            {
                if (PlayData.TSavedata.StageNum == 3)
                {
                    var shortcut = CreateCWShortcutObject();
                    shortcut.transform.SetParent(camp.transform);
                    shortcut.transform.localPosition = new Vector3(7.266f, -1.35f, 0f);
                }
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                int i = 0;
                var list = instructions.ToList();
                var c = list.Count;


                foreach (var ci in instructions)
                {

                    if (ci.opcode == OpCodes.Call && ci.operand.ToString() == "UnityEngine.GameObject Instantiate[GameObject](UnityEngine.GameObject, UnityEngine.Transform)"
                        && list[Math.Max(0, i-8)].Is(OpCodes.Ldsfld, AccessTools.Field(typeof(GDEItemKeys), nameof(GDEItemKeys.Stage_Stage2_Camp)))
                        )
                        
                    {
                        yield return ci;
                        yield return new CodeInstruction(OpCodes.Dup);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SpawnSecretPassagePatch), nameof(SpawnSecretPassagePatch.InstantiateSecretPassage)));
                    }
                    else
                    {
                        yield return ci;
                    }
                    i++;
                }
            }
        }

        // deliberately not added to gdeManager data structures as this is not strictly a random event and just uses random event ui for enabling choice
        static string RE_cwEntryEventKey = GUID + "cwEntryEvent";

        [HarmonyPatch(typeof(GDERandomEventData), nameof(GDERandomEventData.LoadFromSavedData))]
        class RE_GdeData_Patch
        {
            static void Postfix(GDERandomEventData __instance, ref GameObject ____EventFieldObject, ref string ____PathEventFieldObject)

            {
                if (__instance.Key == RE_cwEntryEventKey)
                {
                    __instance.Name = "Secret Passage";
                    __instance.Desc = "Pay &s soulstones and &k keys to get the Crimson Wilderness key. These values can be configured in config file and updated by restarting the game";

                    __instance.Script = typeof(REScript_cwEntryEvent).AssemblyQualifiedName;

                    __instance.UseButton = new List<string>() { "Pay the toll" };
                    __instance.UseButtonTooltip = new List<string>() { $"Pay &s soulstones and &k keys" };


                    __instance.EventDetails = "Forgotten passage to the Crimson Wilderness (for the ones forgetting )))))";
                    __instance.OrderStrings = new List<string>() { "Thank you for your patronage! Please come again!" };

                    var ekd = new GDERandomEventData(GDEItemKeys.RandomEvent_RE_EatingKnowledge);

                    __instance.MainImage = ekd.MainImage;
                    __instance.MainImage_2Stage = ekd.MainImage;
                    __instance.MainImage_3Stage = ekd.MainImage;

                    ____EventFieldObject = ekd.EventFieldObject;
                    AccessTools.FieldRef<GDERandomEventData, string> _PathEventFieldObjectRef = AccessTools.FieldRefAccess<GDERandomEventData, string>("_PathEventFieldObject");
                    ____PathEventFieldObject = _PathEventFieldObjectRef(ekd);



                }
            }
        }

    }
}
