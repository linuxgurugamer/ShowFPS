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
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            //if (instance != null)
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
        float frequency = 0.5f;

        float curFPS;
        float minFPS;

        bool benchmark = false;
        float benchStartTime;
        float benchTime;
        float benchFrames;
        float benchStartFrames;

        bool drag;
        float offset_x;
        float offset_y;

        Text guiText;

        void Awake()
        {
            StartCoroutine(FPS());
            guiText = gameObject.GetComponent<Text>();
            guiText.enabled = false;
        }

        void OnMouseDown()
        {
            drag = true;
            offset_x = guiText.transform.position.x - Input.mousePosition.x / Screen.width;
            offset_y = guiText.transform.position.y - (Screen.height - Input.mousePosition.y) / Screen.height;
        }

        void OnMouseUp()
        {
            drag = false;

            Settings.position_x = Input.mousePosition.x / Screen.width;
            Settings.position_y = (Screen.height - Input.mousePosition.y) / Screen.height;
        }

        void resetBenchmark()
        {
            minFPS = curFPS;
            benchStartTime = Time.realtimeSinceStartup;
            benchStartFrames = Time.frameCount;
        }
        float x, y;

        void Update()
        {
            if (drag)
            {
                x = Input.mousePosition.x / Screen.width;
                y = (Screen.height - Input.mousePosition.y) / Screen.height;
                guiText.transform.position = new Vector3(x + offset_x, y + offset_y, 0f);
            }
            else
            {
                x = Settings.position_x;
                y = Settings.position_y;
            }
            if (PluginKeys.PLUGIN_TOGGLE.GetKeyDown())
            {
                if (Input.GetKey(KeyCode.LeftControl)
                        || Input.GetKey(KeyCode.RightControl))
                {
                    if (!enabled)
                    {
                        return;
                    }
                    benchmark = !benchmark;
                    if (benchmark)
                    {
                        resetBenchmark();
                    }
                }
                else
                {
                    enabled = !enabled;
                    guiText.enabled = enabled;

                    guiText.useGUILayout = false;
                    if (!enabled)
                    {
                        benchmark = false;
                    }
                }
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
                else drag = false;
            }
        }
        void DrawOutline(int offset, Rect r, string t, GUIStyle style, Color outColor, Color inColor)
        {
            Color backup = style.normal.textColor;
            style.normal.textColor = outColor;

            float yOffset = (r.height) * offset;

            style.normal.textColor = inColor;
            //GUI.Label(r, t, style);
            GUI.Label(new Rect(r.x, r.y + yOffset, r.width, r.height), t, style);
            style.normal.textColor = backup;
        }

        private const int LEFT = 10;
        private const int TOP = 20;
        private const int WIDTH = 50;
        private const int HEIGHT = 10;

        //[Persistent]
        //int timeSize = 14;
        private Rect fpsPos = new Rect(LEFT, TOP, WIDTH, HEIGHT);
        GUIStyle timeLabelStyle = null;
        public void OnGUI()
        {
            if (enabled)
            {
                if (timeLabelStyle == null)
                {
                    timeLabelStyle = new GUIStyle(GUI.skin.label);
                    //gametimeX = Mathf.Clamp(gametimeX, 0, Screen.width);
                    //gametimeY = Mathf.Clamp(gametimeY, 0, Screen.height);
                    timeLabelStyle.fontSize = Settings.fontSize; // timeSize;
                }
                Vector2 size = timeLabelStyle.CalcSize(new GUIContent(curFPS.ToString("F2")));

                fpsPos.Set(x * Screen.width + offset_x, y * Screen.height + offset_y, 200f, size.y);

                if (!benchmark)
                {
                    if (curFPS > 60)
                        DrawOutline(0, fpsPos, Math.Round(curFPS).ToString("F0") + " fps", timeLabelStyle, Color.black, Color.white);
                    else
                        DrawOutline(0, fpsPos, Math.Round(curFPS, 2).ToString("F2") + " fps", timeLabelStyle, Color.black, Color.white);
                }
                else
                {
                    DrawOutline(0, fpsPos, "FPS: " + Math.Round(curFPS, 2).ToString("F2"), timeLabelStyle, Color.black, Color.white); DrawOutline(1, fpsPos, "Avg: " + (benchFrames / benchTime).ToString("F1"), timeLabelStyle, Color.black, Color.white);
                    DrawOutline(2, fpsPos, "Min: " + minFPS.ToString("F1"), timeLabelStyle, Color.black, Color.white);
                }
            }

        }

        IEnumerator FPS()
        {
            for (; ; )
            {
                if (!enabled)
                {
                    yield return new WaitForSeconds(frequency * 5);
                }

                // Capture frame-per-second
                int lastFrameCount = Time.frameCount;
                float lastTime = Time.realtimeSinceStartup;
                yield return new WaitForSeconds(frequency);
                float timeSpan = Time.realtimeSinceStartup - lastTime;
                int frameCount = Time.frameCount - lastFrameCount;

                // Display it
                curFPS = frameCount / timeSpan;
                if (benchmark)
                {
                    benchTime = Time.realtimeSinceStartup - benchStartTime;
                    benchFrames = Time.frameCount - benchStartFrames;
                    if (curFPS < minFPS)
                    {
                        minFPS = curFPS;
                    }
                }
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

