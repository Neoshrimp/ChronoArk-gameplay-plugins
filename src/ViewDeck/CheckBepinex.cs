#if WORKSHOP


using HarmonyLib;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ViewDeck
{
    internal class CheckBepinex
    {



        [MethodImpl(MethodImplOptions.NoInlining)]
        static internal bool LoadedByBepinex()
        {

            if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "BepInEx") != null)
            {

                var f_chainDic = AccessTools.Field(AccessTools.TypeByName("BepInEx.Bootstrap.Chainloader"), "PluginInfos");

                var t_PluginInfo = HarmonyLib.AccessTools.TypeByName("BepInEx.PluginInfo");

               


                //Type.GetType("BepInEx.PluginInfo");

                var dType = typeof(Dictionary<,>);
                dType.MakeGenericType(new Type[] { typeof(string), t_PluginInfo });

                
                var m = AccessTools.Method(dType, "ContainsKey");

                m.MakeGenericMethod(new Type[] { typeof(string) } );
                return (bool)m.Invoke(f_chainDic, new object[] { HarmonyContainer.GUID });

                
                //BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(HarmonyContainer.GUID);
            }
            return false;
        
        }


    }
}
#endif

