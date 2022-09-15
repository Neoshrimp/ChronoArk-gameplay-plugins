using GameDataEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwiftnessRework
{
    public class Skills2AddQuick
    {
        public static HashSet<string> defaultQuickness = new HashSet<string>()
        {
            // common
            GDEItemKeys.Skill_S_Public_25,
            GDEItemKeys.Skill_S_Public_23,
            GDEItemKeys.Skill_S_Public_18,
            GDEItemKeys.Skill_S_Public_22,
            GDEItemKeys.Skill_S_Public_21,
            GDEItemKeys.Skill_S_Public_34,
            GDEItemKeys.Skill_S_Public_36,
            GDEItemKeys.Skill_S_Public_33,
            GDEItemKeys.Skill_S_Public_30,
            GDEItemKeys.Skill_S_Public_29,
            GDEItemKeys.Skill_S_Public_31,
            GDEItemKeys.Skill_S_Public_6,
            GDEItemKeys.Skill_S_Public_5_0,
            GDEItemKeys.Skill_S_Public_1,
            GDEItemKeys.Skill_S_Public_0,
            GDEItemKeys.Skill_S_Public_2,
            GDEItemKeys.Skill_S_Public_15,
            GDEItemKeys.Skill_S_Public_16,
            GDEItemKeys.Skill_S_Public_9,
            GDEItemKeys.Skill_S_Public_14,
            // lucy
            GDEItemKeys.Skill_S_LucyD_6,
            GDEItemKeys.Skill_S_LucyD_9,
            GDEItemKeys.Skill_S_LucyD_1,
            GDEItemKeys.Skill_S_Lucy_24,
            GDEItemKeys.Skill_S_Lucy_1,
            GDEItemKeys.Skill_S_Lucy_2,
            GDEItemKeys.Skill_S_Lucy_0,
            GDEItemKeys.Skill_S_Lucy_25,
            GDEItemKeys.Skill_S_Lucy_20,
            GDEItemKeys.Skill_S_Lucy_22,
            GDEItemKeys.Skill_S_Lucy_21,
            GDEItemKeys.Skill_S_Lucy_18,
            GDEItemKeys.Skill_S_Lucy_8,
            GDEItemKeys.Skill_S_Lucy_9,
            GDEItemKeys.Skill_S_Lucy_16,
            GDEItemKeys.Skill_S_Lucy_7,
            GDEItemKeys.Skill_S_Lucy_17,
            GDEItemKeys.Skill_S_Lucy_14,
            GDEItemKeys.Skill_S_Lucy_15,
            // azar
            GDEItemKeys.Skill_S_Azar_P_0,
			GDEItemKeys.Skill_S_Azar_0,
            GDEItemKeys.Skill_S_Azar_1,
            GDEItemKeys.Skill_S_Azar_2,
            GDEItemKeys.Skill_S_Azar_4,
            GDEItemKeys.Skill_S_Azar_7,
            GDEItemKeys.Skill_S_Azar_8_LucyDraw,
            GDEItemKeys.Skill_S_Azar_11,
            // joey
			GDEItemKeys.Skill_S_Joey_10,
            GDEItemKeys.Skill_S_Joey_11,
            //GDEItemKeys.Skill_S_Joey_11_0,
            GDEItemKeys.Skill_S_Joey_0,
            GDEItemKeys.Skill_S_Joey_4,
            GDEItemKeys.Skill_S_Joey_6,
            GDEItemKeys.Skill_S_Joey_7,
            GDEItemKeys.Skill_S_Joey_7_Final,
            // chain
            GDEItemKeys.Skill_S_MissChain_0,
            GDEItemKeys.Skill_S_MissChain_3,
            GDEItemKeys.Skill_S_MissChain_6,
            // leave this to me counter
            GDEItemKeys.Skill_S_MissChain_7_0,
            GDEItemKeys.Skill_S_MissChain_13,
            // hein
            GDEItemKeys.Skill_S_Hein_0,
            GDEItemKeys.Skill_S_Hein_1,
            GDEItemKeys.Skill_S_Hein_4,
            GDEItemKeys.Skill_S_Hein_7,
            GDEItemKeys.Skill_S_Hein_6,
            // sizz
            GDEItemKeys.Skill_S_Sizz_7,
            GDEItemKeys.Skill_S_Sizz_6,
            GDEItemKeys.Skill_S_Sizz_6_1,
            GDEItemKeys.Skill_S_Sizz_11,
            GDEItemKeys.Skill_S_Sizz_9,
            GDEItemKeys.Skill_S_Sizz_1,
            GDEItemKeys.Skill_S_Sizz_10,
            GDEItemKeys.Skill_S_Sizz_12,
            // trisha
            GDEItemKeys.Skill_S_Trisha_6,
            GDEItemKeys.Skill_S_Trisha_7,
            GDEItemKeys.Skill_S_Trisha_0,
            GDEItemKeys.Skill_S_Trisha_9,
            GDEItemKeys.Skill_S_Trisha_12,
            // lian
            GDEItemKeys.Skill_S_Lian_8,
            GDEItemKeys.Skill_S_Lian_2,
            GDEItemKeys.Skill_S_Lian_12,
            GDEItemKeys.Skill_S_Lian_3,
            GDEItemKeys.Skill_S_Lian_11,
            GDEItemKeys.Skill_S_Lian_4,
            GDEItemKeys.Skill_S_Lian_13,
            // pressel
            GDEItemKeys.Skill_S_Priest_6,
            GDEItemKeys.Skill_S_Priest_8,
            GDEItemKeys.Skill_S_Priest_8_0,
            GDEItemKeys.Skill_S_Priest_12,
            GDEItemKeys.Skill_S_Priest_5,
            GDEItemKeys.Skill_S_Priest_1,
            GDEItemKeys.Skill_S_Priest_10,
            GDEItemKeys.Skill_S_Priest_3,
            GDEItemKeys.Skill_S_Priest_9,
            // bird
            GDEItemKeys.Skill_S_Phoenix_7,
            GDEItemKeys.Skill_S_Phoenix_11,
            GDEItemKeys.Skill_S_Phoenix_1,
            GDEItemKeys.Skill_S_Phoenix_5,
            GDEItemKeys.Skill_S_Phoenix_6,
            GDEItemKeys.Skill_S_Phoenix_2,
            GDEItemKeys.Skill_S_Phoenix_0,
            GDEItemKeys.Skill_S_Phoenix_4_0,
            
            //GDEItemKeys.Skill_S_Phoenix_4,
            GDEItemKeys.Skill_S_Phoenix_8,
            GDEItemKeys.Skill_S_Phoenix_9,
            // find bread
            //GDEItemKeys.Skill_S_Phoenix_10_0,
            GDEItemKeys.Skill_S_Phoenix_10_1,
            GDEItemKeys.Skill_S_Phoenix_5_0,
            GDEItemKeys.Skill_S_Phoenix_3_0,
            GDEItemKeys.Skill_S_Phoenix_8_0,

            // iron
            GDEItemKeys.Skill_S_Prime_11,
            GDEItemKeys.Skill_S_Prime_12,
            GDEItemKeys.Skill_S_Prime_4,
            GDEItemKeys.Skill_S_Prime_7_LucyDraw,
            GDEItemKeys.Skill_S_Prime_3,
            GDEItemKeys.Skill_S_Prime_8,
            GDEItemKeys.Skill_S_Prime_13,
            // charon
            GDEItemKeys.Skill_S_ShadowPriest_7,
            GDEItemKeys.Skill_S_ShadowPriest_5,
            GDEItemKeys.Skill_S_ShadowPriest_12_0,
            GDEItemKeys.Skill_S_ShadowPriest_4,
            GDEItemKeys.Skill_S_ShadowPriest_3,
            GDEItemKeys.Skill_S_ShadowPriest_13,
            GDEItemKeys.Skill_S_ShadowPriest_9,
            GDEItemKeys.Skill_S_ShadowPriest_11,
            // silver
            //GDEItemKeys.Skill_S_SilverStein_10,
            // created rapid shot
            GDEItemKeys.Skill_S_SilverStein_10_0,
            GDEItemKeys.Skill_S_SilverStein_7,
            GDEItemKeys.Skill_S_SilverStein_8,
            GDEItemKeys.Skill_S_SilverStein_0,
            GDEItemKeys.Skill_S_SilverStein_11,
            GDEItemKeys.Skill_S_SilverStein_9,
            // helia
            GDEItemKeys.Skill_S_TW_Red_5,
            GDEItemKeys.Skill_S_TW_Red_R0,
            GDEItemKeys.Skill_S_TW_Red_R0_0,
            GDEItemKeys.Skill_S_TW_Red_4,
            // for solar ring effect. might not be needed
            GDEItemKeys.Skill_S_TW_Red_4_0,
            GDEItemKeys.Skill_S_TW_Red_4_1,
            // celestial alignment
            GDEItemKeys.Skill_S_TW_Blue_R1,
            // selena
            GDEItemKeys.Skill_S_TW_Blue_6,
            GDEItemKeys.Skill_S_TW_Blue_9,
            GDEItemKeys.Skill_S_TW_Blue_5,
            GDEItemKeys.Skill_S_TW_Blue_5_0,
            GDEItemKeys.Skill_S_TW_Blue_0,
            // for lunar ring effect. might not be needed
            GDEItemKeys.Skill_S_TW_Blue_7_0,
            GDEItemKeys.Skill_S_TW_Blue_7_1,
            GDEItemKeys.Skill_S_TW_Blue_4,
            GDEItemKeys.Skill_S_TW_Blue_3,
            GDEItemKeys.Skill_S_TW_Blue_R0,
            GDEItemKeys.Skill_S_TW_Blue_1,
            // Huz 
            GDEItemKeys.Skill_S_Queen_7,
            GDEItemKeys.Skill_S_Queen_11,
            GDEItemKeys.Skill_S_Queen_13,
            GDEItemKeys.Skill_S_Queen_12,
            GDEItemKeys.Skill_S_Queen_1,
            GDEItemKeys.Skill_S_Queen_0,
            GDEItemKeys.Skill_S_Queen_0_1,
            GDEItemKeys.Skill_S_Queen_9,
            GDEItemKeys.Skill_S_Queen_10,
            // Narhan
            GDEItemKeys.Skill_S_Control_11,
            GDEItemKeys.Skill_S_Control_0,
            GDEItemKeys.Skill_S_Control_4,
            GDEItemKeys.Skill_S_Control_10,
            GDEItemKeys.Skill_S_Control_9,
            // yohan
			GDEItemKeys.Skill_S_Mement_P,
            GDEItemKeys.Skill_S_Mement_1,
            GDEItemKeys.Skill_S_Mement_2,
            GDEItemKeys.Skill_S_Mement_4,
            GDEItemKeys.Skill_S_Mement_3,
            GDEItemKeys.Skill_S_Mement_R0,
            GDEItemKeys.Skill_S_Mement_LucyDraw,
            // illya
            GDEItemKeys.Skill_S_Ilya_10,
            GDEItemKeys.Skill_S_Ilya_7,
            GDEItemKeys.Skill_S_Ilya_1,
            GDEItemKeys.Skill_S_Ilya_3,
            GDEItemKeys.Skill_S_Ilya_4,
            // lucyC
            GDEItemKeys.Skill_S_LucyC_7,
            GDEItemKeys.Skill_S_LucyC_6,

            // other
			GDEItemKeys.Skill_S_MessiahbladesPrototype,
            GDEItemKeys.Skill_S_DefultSkill_2,
            GDEItemKeys.Skill_S_StraightFlush_0,
            GDEItemKeys.Skill_S_S2_MainBoss_1_Lucy_0,
            GDEItemKeys.Skill_S_Witch_P_0,
            GDEItemKeys.Skill_S_Witch_2,
            GDEItemKeys.Skill_S_S_TheLight_P_1,
            GDEItemKeys.Skill_S_Joker_0,
            GDEItemKeys.Skill_S_Sniper_1,
            GDEItemKeys.Skill_S_MBoss2_1_5,
            GDEItemKeys.Skill_S_S4_King_LastAttack_0,
            GDEItemKeys.Skill_S_Lucy_25,
            GDEItemKeys.Skill_S_DorchiX_Lucy,

        };
    }
}
