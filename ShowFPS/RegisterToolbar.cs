
using UnityEngine;
using ToolbarControl_NS;


namespace ShowFPS
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        bool initted = false;
        void Start()
        {
            ToolbarControl.RegisterMod(Graph.MODID, Graph.MODNAME);
        }

        void OnGUI()
        {
            if (!initted)
            {
                Graph.instance.InitToolbar();
            }
        }
    }
}