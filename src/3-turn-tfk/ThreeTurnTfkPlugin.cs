using BepInEx;
using BepInEx.Configuration;
using DarkTonic.MasterAudio;
using GameDataEditor;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Select_skills_on_Normal
{
    [BepInPlugin(GUID, "3 turn TFK", version)]
    [BepInProcess("ChronoArk.exe")]
    public class ThreeTurnTfkPlugin : BaseUnityPlugin
    {

        public const string GUID = "org.neo.ca.joke.3turntfk";
        public const string version = "1.0.0";


        private static readonly Harmony harmony = new Harmony(GUID);

        private static BepInEx.Logging.ManualLogSource logger;

        private static ConfigEntry<int> deathTurnConf;


        void Awake()
        {
            deathTurnConf = Config.Bind("Gameplay config", "tfk_turn_limit", 3, "leave at 3 or no balls");

            logger = Logger;
            harmony.PatchAll();
        }
        void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchAll(GUID);
        }


        [HarmonyPatch]
        class FogTurnPatch
        {
            static MethodBase TargetMethod()
            {
                return AccessTools.PropertyGetter(typeof(GDEEnemyQueueData), "CustomeFogTurn");
            }

            static void Postfix(GDEEnemyQueueData __instance, ref int __result)
            {
                if (__instance.Key == GDEItemKeys.EnemyQueue_Queue_S4_King)
                {
                    __result = deathTurnConf.Value;
                }
            }

        }



        [HarmonyPatch(typeof(BattleSystem))]
        class BattlesystemPatch
        {

            // example of how to patch a method which returns IEnumerable
            // https://gist.github.com/pardeike/c873b95e983e4814a8f6eb522329aee5

            delegate void ActionDelegate();

            class PostfixEnum : IEnumerable
            {
                public IEnumerator enumerator;
                public ActionDelegate prefixAction, postfixAction;

                IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
                public IEnumerator GetEnumerator()
                {
                    //prefixAction.Invoke();
                    while (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                    postfixAction.Invoke();
                }
            }



            [HarmonyPatch(nameof(BattleSystem.EnemyTurn))]
            [HarmonyPostfix]
            static void TurnEndPostfix(BattleSystem __instance, bool EndButton, ref IEnumerator __result)
            {
                void PostfixAction()
                {
                    if (__instance.MainQueueData.Key == GDEItemKeys.EnemyQueue_Queue_S4_King && EndButton == true && __instance.TurnNum >= deathTurnConf.Value)

                    {

                        BattleEnemy tfk = __instance.EnemyList.Find(be => be.Info.KeyData == GDEItemKeys.Enemy_S4_King_0);
                        if (tfk == null)
                            return;
                        var endOflight = MasterAudio.GetAllPlayingVariations().Find(e => e.name.Contains("End of Light"));
                        if (endOflight != null)
                            __instance.StartCoroutine(CustomFadeTo(endOflight, 0.2f, 3f));

                        FunnyMaymays.RollVoiceLine(FunnyMaymays.CreateVoiceLines(tfk))?.dialog.Invoke();

                        BattleSystem.DelayInput(WaitAndClap(0f));


                        BattleSystem.DelayInput(typeof(BattleSystem).GetMethod("LoseBattle", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null) as IEnumerator);

                    }
                };


                var postFixEnum = new PostfixEnum()
                {
                    enumerator = __result,
                    prefixAction = new ActionDelegate(() => { return; }),
                    postfixAction = new ActionDelegate(PostfixAction)
                };

                __result = postFixEnum.GetEnumerator();


            }

            static IEnumerator WaitAndClap(float t)
            {
                yield return new WaitForSeconds(t);
                MasterAudio.PlaySound("Clap", 1f, null, 0f, null, null, false, false); //duration 0.73s
                yield return new WaitForSeconds(0.5f);
                yield break;
            }

            // it almost works as intended
            static IEnumerator CustomFadeTo(SoundGroupVariation soundGroupVariation, float targetVolume, float time)
            {
                float volume = soundGroupVariation.PlaySoundParm.VolumePercentage;
                float volChange = targetVolume - volume;
                if (volChange == 0)
                    yield break;
                float step = volChange / time;

                bool RunCond()
                {
                    if (step > 0)
                        return volume + step * Time.deltaTime < targetVolume;
                    else
                        return volume + step * Time.deltaTime > targetVolume;
                }
                //var stopwatch = Stopwatch.StartNew();
                while (RunCond())
                {
                    //time -= Time.deltaTime;
                    volume += step * Time.deltaTime;
                    soundGroupVariation.AdjustVolume(volume);
                    yield return null;
                }
                yield return null;
                //Debug.Log("Fade time " + " " + stopwatch.Elapsed);
                soundGroupVariation.AdjustVolume(targetVolume);
                yield break;
            }


        }




    }
}
