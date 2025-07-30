
using KSP.Localization;
using ToolbarControl_NS;
using UnityEngine;


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
                Graph.helpText1 =
             Localizer.Format("#LOC_ShowFPS_B") + "" + "<color=yellow>" + Localizer.Format("#LOC_ShowFPS_General_Controls") + "</color>" + Localizer.Format("#LOC_ShowFPS_B_DUP1") +
            Localizer.Format("#LOC_ShowFPS_B_Mod_KeypadMultiply_B_to") +
            Localizer.Format("#LOC_ShowFPS_B_Mod_KeypadPlus_B_increa") +
            Localizer.Format("#LOC_ShowFPS_B_Mod_KeypadMinus_B_decre") +

            "<B><color=yellow>" + // NO_LOCALIZATION
                Localizer.Format("#LOC_ShowFPS_Legend") +
                "</color>\n\n</B>\b" +  // NO_LOCALIZATION
            "<color=green>" + // NO_LOCALIZATION
                Localizer.Format("#LOC_ShowFPS_Green") +
                "</color>      " + // NO_LOCALIZATION
                Localizer.Format("#LOC_ShowFPS_FPS") +
                "﻿\n" + // NO_LOCALIZATION
            "<color=yellow>" + // NO_LOCALIZATION
                Localizer.Format("#LOC_ShowFPS_Yellow") +
                "</color>     " + // NO_LOCALIZATION
                Localizer.Format("#LOC_ShowFPS_FPS_Average") +
                "﻿\n" + // NO_LOCALIZATION
            "<color=red>" + // NO_LOCALIZATION
                Localizer.Format("#LOC_ShowFPS_Red") +
                "</color>        " + // NO_LOCALIZATION
                Localizer.Format("#LOC_ShowFPS_Simulation_Rate") +
                "\n" + // NO_LOCALIZATION
            "<color=grey>" + // NO_LOCALIZATION
                Localizer.Format("#LOC_ShowFPS_Grey") +
                "</color>      " + // NO_LOCALIZATION
                Localizer.Format("#LOC_ShowFPS_Max_Sim_Rate");
            }
        }
    }
}