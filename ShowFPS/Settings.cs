using System;
using System.IO;
using UnityEngine;


namespace ShowFPS
{
    public static class Settings
    {
         static string configPath { get { return KSPUtil.ApplicationRootPath + "GameData/ShowFPS/PluginData/settings.cfg"; } }
        static ConfigNode settings;

        internal static float position_x;
        internal static float position_y;

        internal static int fontSize = 10;

        internal const int GRAPHWIDTH = 600;
        internal const int GRAPHHEIGHT = 256;

        internal static int GraphWidth = GRAPHWIDTH;
        internal static int GraphHeight = GRAPHHEIGHT;
        internal static float winX = 80;
        internal static float winY = 80;

        internal static KeyCode keyToggleWindow;
        internal static KeyCode keyScaleUp;
        internal static KeyCode keyScaleDown;

        internal static bool showPerfectSym;
        internal static bool periodicRescale = false;
        internal static float frequency = 0.5f;
        internal static float alpha = 1f;

        public static void LoadConfig()
        {
            settings = ConfigNode.Load(configPath) ?? new ConfigNode();
            // These values are based on screen size
            position_x = GetValue("position_x", 50);
            position_y = GetValue("position_y", 50);
            fontSize = GetValue("fontSize", 10);
            keyToggleWindow = GetValue("keyToggleWindow", KeyCode.KeypadMultiply);
            keyScaleUp = GetValue("keyScaleUp", KeyCode.KeypadPlus);
            keyScaleDown = GetValue("keyScaleDown", KeyCode.KeypadMinus);


            showPerfectSym = GetValue("showPerfectSym", false);
            periodicRescale = GetValue("periodicRescale", false);
            frequency = GetValue("frequency", 0.5f);
            alpha = GetValue("alpha", 1f);
            winX = GetValue("winX", 80f);
            winY = GetValue("winY", 80f);



            Graph.instance.InitGraphWindow();
            PluginKeys.Setup();
        }

        public static void SaveConfig()
        {
            SetValue("position_x", position_x);
            SetValue("position_y", position_y);
            SetValue("plugin_key", PluginKeys.PLUGIN_TOGGLE.primary.ToString());
            SetValue("fontSize", fontSize);

            SetValue("showPerfectSym", showPerfectSym);
            SetValue("periodicRescale", periodicRescale);
            SetValue("frequency", frequency);
            SetValue("alpha", alpha);
            SetValue("winX", winX);
            SetValue("winY", winY);

            settings.Save(configPath);
        }

        public static void SetValue(string key, object value)
        {
            if (settings.HasValue(key))
            {
                settings.RemoveValue(key);
            }
            settings.AddValue(key, value);
        }

        public static int GetValue(string key, int defaultValue)
        {
            int value;
            return int.TryParse(settings.GetValue(key), out value) ? value : defaultValue;
        }

        public static bool GetValue(string key, bool defaultValue)
        {
            bool value;
            return bool.TryParse(settings.GetValue(key), out value) ? value : defaultValue;
        }

        public static float GetValue(string key, float defaultValue)
        {
            float value;
            return float.TryParse(settings.GetValue(key), out value) ? value : defaultValue;
        }

        public static string GetValue(string key, string defaultValue)
        {
            string value = settings.GetValue(key);
            return String.IsNullOrEmpty(value) ? defaultValue : value;
        }
        public static KeyCode GetValue(string key, KeyCode defaultValue)
        {
            string value = settings.GetValue(key);
            return String.IsNullOrEmpty(value) ? defaultValue : (KeyCode)Enum.Parse(typeof(KeyCode), value, false);
        }
    }

}
