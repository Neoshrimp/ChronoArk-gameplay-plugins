using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Three_turn_tfk
{
    class FunnyMaymays
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
                odds = 40,
                dialog = () =>
                {
                    BattleSystem.DelayInput(BattleText.InstBattleText_Co(enemy, "Time's up", false, 0, 0f));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 40,
                dialog = () =>
                {
                    BattleSystem.DelayInput(BattleText.InstBattleText_Co(enemy, "Goodbye", false, 0, 0f));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 30,
                dialog = () =>
                {
                    BattleSystem.DelayInput(BattleText.InstBattleText_Co(enemy, "Omae Wa Mou Shindeiru", false, 0, 0f));
                    BattleSystem.DelayInput(BattleText.InstBattleTextAlly_Co(BattleSystem.instance.AllyList[Random.Range(0, BattleSystem.instance.AllyList.Count)].GetTopPos(),
                        "NANI?!?!", true));
                }
            });


            voiceLines.Add(new VoiceLine
            {
                odds = 20,
                dialog = () =>
                {
                    BattleSystem.DelayInput(BattleText.InstBattleText_Co(enemy, "Git gud", false, 0, 0f));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 20,
                dialog = () =>
                {
                    BattleSystem.DelayInput(BattleText.InstBattleText_Co(enemy, "Where's the damage? I can't find the damage. Can you show me? Where's the DAMAGE at!?", false, 0, 0f));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 20,
                dialog = () =>
                {
                    BattleSystem.DelayInput(BattleText.InstBattleText_Co(enemy, "Heh. nothing personnel, kid", false, 0, 0f));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 10,
                dialog = () =>
                {
                    BattleSystem.DelayInput(BattleText.InstBattleText_Co(enemy, "Shrimp sends his regards", false, 0, 0f));
                }
            });

            voiceLines.Add(new VoiceLine
            {
                odds = 10,
                dialog = () =>
                {
                    BattleSystem.DelayInput(BattleText.InstBattleText_Co(enemy, "Deeeeez nuts!", false, 0, 0f));
                    BattleSystem.DelayInput(BattleText.InstBattleTextAlly_Co(BattleSystem.instance.AllyList[Random.Range(0, BattleSystem.instance.AllyList.Count)].GetTopPos(),
                        "GOTTEM", true));
                }
            });

            return voiceLines;

        }

        public static VoiceLine RollVoiceLine(VoiceLinesInfo voicelines, int rigg = -1)
        {
            int roll = Random.Range(0, voicelines.totalOdds);
            if (rigg >= 0)
                roll = rigg;
            Debug.Log(roll);

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
