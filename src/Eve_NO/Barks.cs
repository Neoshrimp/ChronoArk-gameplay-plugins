using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Eve_NO
{
    // ᗜˬᗜ
    class Barks
    {

        public class VoiceLine
        {
            public int odds;

            public delegate void Dialog();

            public Dialog dialog;
        }

        public class VoiceLinesInfo
        {
            public List<VoiceLine> vlList = new List<VoiceLine>();
            public int totalOdds = 0;
            // change to binary tree?
            // vector2 as a lazy alternative to Pair
            public Dictionary<Vector2, int> range2index = new Dictionary<Vector2, int>();

            public void Add(VoiceLine voiceLine)
            {
                vlList.Add(voiceLine);
                range2index.Add(new Vector2(totalOdds, totalOdds + vlList.Last().odds), vlList.Count - 1);
                totalOdds += vlList.Last().odds;
            }
        }

        public static VoiceLinesInfo CreateVoiceLines(BattleEnemy enemy, BattleAlly ally = null)
        {
            var voiceLines = new VoiceLinesInfo();

            voiceLines.Add(new VoiceLine
            {
                odds = 35,
                dialog = () =>
                {
                    BattleSystem.instance.StartCoroutine(BattleText.InstBattleTextAlly_Co(ally.GetTopPos(),
                        "Eve, NOOOOOOOOO!!", true));
                }
            });


            voiceLines.Add(new VoiceLine
            {
                odds = 35,
                dialog = () =>
                {
                    BattleSystem.instance.StartCoroutine(BattleText.InstBattleTextAlly_Co(ally.GetTopPos(),
                        "YAMERO!!", true));
                }
            });
            
            voiceLines.Add(new VoiceLine
            {
                odds = 30,
                dialog = () =>
                {
                    BattleSystem.instance.StartCoroutine(BattleText.InstBattleTextAlly_Co(ally.GetTopPos(),
                        "｡ﾟ(T_T)ﾟ     ", true));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 25,
                dialog = () =>
                {
                    BattleSystem.instance.StartCoroutine(BattleText.InstBattleTextAlly_Co(ally.GetTopPos(),
                        "YEEET", true));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 20,
                dialog = () =>
                {
                    BattleSystem.instance.StartCoroutine(BattleText.InstBattleTextAlly_Co(ally.GetTopPos(),
                        "Do your best, Eve-chan!", true));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 20,
                dialog = () =>
                {
                    BattleSystem.instance.StartCoroutine(BattleText.InstBattleTextAlly_Co(ally.GetTopPos(),
                        "Frag out.", true));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 20,
                dialog = () =>
                {
                    BattleSystem.instance.StartCoroutine(BattleText.InstBattleTextAlly_Co(ally.GetTopPos(),
                        "You will be remembered :(", true));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 15,
                dialog = () =>
                {
                    BattleSystem.instance.StartCoroutine(BattleText.InstBattleTextAlly_Co(ally.GetTopPos(),
                        "(/°o°)/--------(*_*)", true));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 7,
                dialog = () =>
                {
                    BattleSystem.instance.StartCoroutine(BattleText.InstBattleTextAlly_Co(ally.GetTopPos(),
                        "deez nuts", true));
                }
            });

            return voiceLines;

        }

        public static VoiceLine RollVoiceLine(VoiceLinesInfo voicelines, int rigg = -1)
        {
            int roll = Random.Range(0, voicelines.totalOdds);
            if (rigg >= 0)
                roll = rigg;
            //Debug.Log("roll: "+ roll.ToString());

            foreach (var kv in voicelines.range2index)
            {
                if (kv.Key.x <= roll && roll < kv.Key.y)
                {
                    return voicelines.vlList[kv.Value];
                }
            }
            return null;
        }
    }
}
