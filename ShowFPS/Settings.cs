using System;
using System.IO;
using UnityEngine;


namespace ShowFPS
{
    public static class Settings
    {
        const string configPath = "GameData/ShowFPS/PluginData/settings.cfg";
        static string configAbsolutePath;
        static ConfigNode settings;

        public static float position_x;
        public static float position_y;

        public static void LoadConfig()
        {
            configAbsolutePath = Path.Combine(KSPUtil.ApplicationRootPath, configPath);
            settings = ConfigNode.Load(configAbsolutePath) ?? new ConfigNode();

            position_x = GetValue("position_x", 0.05f);
            position_y = GetValue("position_y", 0.25f);

            PluginKeys.Setup();
        }

        public static void SaveConfig()
        {
            SetValue("position_x", position_x);
            SetValue("position_y", position_y);
            SetValue("plugin_key", PluginKeys.PLUGIN_TOGGLE.primary.ToString());

            settings.Save(configAbsolutePath);
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
    }

    public static class PluginKeys
    {
        public static KeyBinding PLUGIN_TOGGLE = new KeyBinding(KeyCode.F8);

        public static void Setup()
        {
            PLUGIN_TOGGLE = new KeyBinding(Parse(Settings.GetValue("plugin_key", PLUGIN_TOGGLE.primary.ToString())));
        }

        public static KeyCode Parse(string value)
        {
            return (KeyCode)Enum.Parse(typeof(KeyCode), value);
        }
    }
}
