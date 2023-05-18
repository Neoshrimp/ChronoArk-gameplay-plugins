#if BEPINEX



using BepInEx;

namespace ViewDeck
{
    [BepInPlugin(HarmonyContainer.GUID, "View deck and discard", HarmonyContainer.version)]
    [BepInProcess("ChronoArk.exe")]
    public class BepisPlugin : BaseUnityPlugin
    {

        private static BepInEx.Logging.ManualLogSource logger;
       
        void Awake()
        {
            logger = Logger;

            //if(WorkshopPlugin.attachObject == null)
            gameObject.AddComponent<ViewDeck>();


        }

        void OnDestroy()
        {
            var viewDeck = gameObject.GetComponent<ViewDeck>();
            if(viewDeck != null)
                Destroy(viewDeck);
        }




    }
}
#endif

