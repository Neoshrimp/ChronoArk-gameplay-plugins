#if WORKSHOP



using ChronoArkMod.Plugin;
using Debug = UnityEngine.Debug;
using UnityEngine;

namespace ViewDeck
{

    [PluginConfig(HarmonyContainer.GUID, HarmonyContainer.name, HarmonyContainer.version)]
    public class WorkshopPlugin : ChronoArkPlugin
    {

        public static GameObject attachObject;

     
        public override void Initialize()
        {

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
