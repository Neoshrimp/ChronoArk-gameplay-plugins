using GameDataEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyCWaccess
{
    public class REScript_cwEntryEvent : RandomEventBaseScript  
    {
        public REScript_cwEntryEvent()
        {
            soulNeeded = Math.Max(0, EasyCWaccess_Plugin.configCWsoulstones.Value);
            keysNeeded = Math.Max(0, EasyCWaccess_Plugin.configCWkeys.Value);
        }

        public override void EventOpen()
        {
            base.EventOpen();
            this.MyUI.ButtonTooltips[0].TooltipString = MainData.UseButtonTooltip[0].Replace("&s", soulNeeded.ToString()).Replace("&k", keysNeeded.ToString());
            MyUI.ButtonTooltips[0].ToolTipString_l2 = null;

            MyUI.HelpTooltip.TooltipString = MainData.Desc.Replace("&s", soulNeeded.ToString()).Replace("&k", keysNeeded.ToString());
            MyUI.HelpTooltip.ToolTipString_l2 = null;
        }


        public override void UseButton1()
        {
            base.UseButton1();
            if (PlayData.Soul >= EasyCWaccess_Plugin.configCWsoulstones.Value 
                && PartyInventory.InvenM.FindItem(GDEItemKeys.Item_Misc_Item_Key) >= EasyCWaccess_Plugin.configCWkeys.Value)
            {
                PlayData.Soul -= EasyCWaccess_Plugin.configCWsoulstones.Value;
                PartyInventory.InvenM.DelItem(GDEItemKeys.Item_Misc_Item_Key, EasyCWaccess_Plugin.configCWkeys.Value);
                InventoryManager.Reward(ItemBase.GetItem(GDEItemKeys.Item_Misc_RWEnterItem));

                base.EventDisable(base.Orderstrings[0]);
            }
            else
            {
			    EffectView.SimpleTextout(this.MyUI.ButtonList[0].transform, "Pay up, bum", 1f, false, 1f);

            }
        }

        int soulNeeded;
        int keysNeeded;
    }
}
