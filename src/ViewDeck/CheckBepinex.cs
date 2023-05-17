#if BEPINEX


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ViewDeck
{
    internal class CheckBepinex
    {



        [MethodImpl(MethodImplOptions.NoInlining)]
        static internal bool LoadedByBepinex()
        {

            if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "BepInEx") != null)
            {
                BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(HarmonyContainer.GUID);
            }
            return false;
        
        }


    }
}
#endif

