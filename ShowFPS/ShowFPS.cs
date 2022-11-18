//  ShowFPS.cs
//
//  Author:
//       Elián Hanisch <lambdae2@gmail.com>
//
//  Copyright (c) 2013-2016 Elián Hanisch
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ShowFPS
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class ShowFPS : MonoBehaviour
    {
        static FPSCounter instance;

        void Awake()
        {
            if (instance == null)
            {
                Settings.LoadConfig();
                instance = gameObject.AddComponent<FPSCounter>();
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnDestroy()
        {
            if (instance != null)
            {
                Settings.SaveConfig();
            }
        }
    }

    /* Code adapted from the example in http://wiki.unity3d.com/index.php?title=FramesPerSecond 
     * written by Annop "Nargus" Prapasapong. */
    [RequireComponent(typeof(Text))]
    public class FPSCounter : MonoBehaviour
    {
        new bool enabled = false;
        internal static float frequency = 0.5f;

        float curFPS;


        bool drag;

        Text guiText;

        void Awake()
        {
            StartCoroutine(FPS());
            guiText = gameObject.GetComponent<Text>();
            guiText.enabled = false;
        }

        void OnMouseDown()
        {
            //Debug.Log("[ShowFPS: OnMouseDown");
            drag = true;
        }

        void OnMouseUp()
        {
            //Debug.Log("[ShowFPS: OnMouseUp");
            drag = false;

            Settings.position_x = x;
            Settings.position_y = y;
            Settings.SaveConfig();
        }

        float x, y;
#if false
        int cnt = 0;
#endif
        void Update()
        {
#if fasle
            if (cnt++ == 100)
            {
                Debug.Log("[ShowFPS]: x, y: " +  Settings.position_x + ", " + Settings.position_y);
                cnt = 0;
            }
#endif
            if (drag)
            {
                x = Input.mousePosition.x ;
                y = (Screen.height - Input.mousePosition.y);
                //Debug.Log("ShowFPS.update, mouse x, y: " + x + ", " + y);
                guiText.transform.position = new Vector3(x , 0f);
            }
            else
            {
                x = Settings.position_x;
                y = Settings.position_y;
            }
            if (PluginKeys.PLUGIN_TOGGLE.GetKeyDown())
            {
#if true
                if (Input.GetKey(KeyCode.LeftControl)
                        || Input.GetKey(KeyCode.RightControl))
                {
                    if (!enabled)
                    {
                        return;
                    }
                }
                else
                {
                    enabled = !enabled;
                    guiText.enabled = enabled;

                    guiText.useGUILayout = false;
                }
#endif
            }
            if (enabled)
            {
                if (drag || fpsPos.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))
                {
                    bool b = Input.GetMouseButton(0);

                    if (!drag && b)
                    {
                        OnMouseDown();
                    }
                    else
                        if (drag && !b)
                        OnMouseUp();
                    drag = b;

                }
                else
                {
                    if (drag)
                    {
                        Settings.position_x = x;
                        Settings.position_y = y;

                        Settings.SaveConfig();

                        drag = false;
                    }
                }
            }
        }
        void DrawOutline(int offset, Rect r, string t, int strength, GUIStyle style, Color outColor, Color inColor)
        {
            Color backup = style.normal.textColor;
            style.normal.textColor = outColor;

            float yOffset = (r.height) * offset;

            style.normal.textColor = inColor;
            GUI.Label(new Rect(r.x, r.y + yOffset, r.width, r.height), t, style);
            style.normal.textColor = backup;
        }

        private const int LEFT = 10;
        private const int TOP = 20;
        private const int WIDTH = 50;
        private const int HEIGHT = 10;

        private Rect fpsPos = new Rect(LEFT, TOP, WIDTH, HEIGHT);
        GUIStyle timeLabelStyle = null;
        public void OnGUI()
        {
            if (enabled)
            {
                if (timeLabelStyle == null)
                {
                    timeLabelStyle = new GUIStyle(GUI.skin.label);
                }
                Vector2 size = timeLabelStyle.CalcSize(new GUIContent(curFPS.ToString("F2")));

                //fpsPos.Set(x, y , 200f, size.y);
                fpsPos.Set(x, y, size.x, size.y);

                if (curFPS > 60)
                    DrawOutline(0, fpsPos, Math.Round(curFPS).ToString("F0") + " fps", 1, timeLabelStyle, Color.black, Color.white);
                else
                    DrawOutline(0, fpsPos, Math.Round(curFPS, 1).ToString("F1") + " fps", 1, timeLabelStyle, Color.black, Color.white);
            }

        }

        IEnumerator FPS()
        {
            for (; ; )
            {
                if (!enabled) // double the wait if not enabled
                {
                    yield return new WaitForSeconds(frequency);
                }

                // Capture frame-per-second
                int lastFrameCount = Time.frameCount;
                float lastTime = Time.realtimeSinceStartup;
                yield return new WaitForSeconds(frequency);

                float timeSpan = Time.realtimeSinceStartup - lastTime;
                int frameCount = Time.frameCount - lastFrameCount;

                // Display it
                curFPS = frameCount / timeSpan;
                var symRate = Math.Min(Time.maximumDeltaTime / Time.deltaTime, 1f);

                //var symRate = frameCount / (timeSpan / Planetarium.fetch.fixedDeltaTime) ;
                //Debug.Log("curFPS: " + curFPS.ToString("F2") + ", frameCount: " + frameCount + ",  timeSpan: " + timeSpan +
                //    ", deltaTime: " + Time.deltaTime + ", maximumDeltaTime: " + Time.maximumDeltaTime + ", symRate: " + symRate + ", 1/symrate: " + (1 / symRate).ToString());
                Graph.instance.AddFPSValue(curFPS, (float)symRate);
            }
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

