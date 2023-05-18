#if WORKSHOP



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChronoArkMod;
using ChronoArkMod.Plugin;
using Debug = UnityEngine.Debug;
using UnityEngine;
using Mono.Cecil;
using System.Reflection;

namespace ViewDeck
{

    [PluginConfig(HarmonyContainer.GUID, HarmonyContainer.name, HarmonyContainer.version)]
    public class WorkshopPlugin : ChronoArkPlugin
    {

        public static GameObject attachObject;

     
        public override void Initialize()
        {

            DependencyResolutionTest.ParseBadAss();


            if (!CheckBepinex.LoadedByBepinex())
            {
                Debug.Log($"Loading {HarmonyContainer.GUID} from workshop");
                attachObject = new GameObject( HarmonyContainer.GUID + "attachObject");
                UnityEngine.Object.DontDestroyOnLoad(attachObject);
                attachObject.hideFlags = HideFlags.HideAndDontSave;
                attachObject.AddComponent<ViewDeck>();
            }
        }

        public override void Dispose()
        {
            UnityEngine.Object.Destroy(attachObject);
        }




    }
}
#endif
