/*
 Copyright (c) 2016 Gerry Iles (Padishar)

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
*/

using System;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using ToolbarControl_NS;
using ClickThroughFix;


namespace ShowFPS
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class Graph : MonoBehaviour
    {
        public static Graph instance;

        const String testFilename = "test.cfg";

        const int GraphX = 10;
        const int GraphY = 36;
        const int LabelX = 10;
        const int LabelY = 18;
        const int LabelHeight = 20;
        int LabelWidth = Settings.GRAPHWIDTH;
        int WndWidth = Settings.GRAPHWIDTH + 8;
        int WndHeight = Settings.GRAPHHEIGHT + 42;

        int numScales = 15;   // Number of entries in the scale array

        Rect windowPos;
        Rect windowDragRect;
        Rect helpWinPos;
        int windowId = 0;
        string windowTitle;
        bool showUI = true;
        bool showHelp = false;
        Rect labelRect;
        Rect graphRect;

        const int FPS = 0;
        const int FPS_AVG = 1;
        const int SYMRATE = 2;

        internal float[,] fpsValues;

        Texture2D texGraph;

        int valIndex = 0;           // The current index into the values array
        int lastRendered = 0;       // The last index of the values array that has been rendered into the texture

        string guiStr;              // The string at the top of the window (only updated when required)


        bool fullUpdate = true;     // Flag to force re-render of entire texture (e.g. when changing scale)

        bool startVisible = false;

        internal KeyCode keyToggleWindow;
        internal KeyCode keyScaleUp;
        internal KeyCode keyScaleDown;

        int scaleIndex = 4;         // Index of the current vertical scale
        static float[] valCycle;


        Color[] blackLine;
        Color[] redLine;
        Color[] yellowLine;
        Color[] greenLine;
        Color[] blueLine;
        Color[] greyLine;

        static internal GUIStyle labelStyle;

        private ResizeHandle resizeHandle;

        static GUIStyle resizeBoxStyle;

        static GUIStyle greenFont, redFont, redButtonFont;

        float? movingAvg;
        float curFPS;
        const float SYM_MULT = 0.5f;


        public static bool IsOpen()
        {
            return instance != null ? instance.showUI : false;
        }

        ToolbarControl toolbarControl = null;

        internal const string MODID = "ShowFPS_NS";
        internal const string MODNAME = "ShowFPS";
        internal void InitToolbar()
        {
            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(Toggle, Toggle,
                    ApplicationLauncher.AppScenes.SPACECENTER |
                    ApplicationLauncher.AppScenes.FLIGHT |
                    ApplicationLauncher.AppScenes.MAPVIEW |
                    ApplicationLauncher.AppScenes.VAB |
                    ApplicationLauncher.AppScenes.SPH |
                    ApplicationLauncher.AppScenes.TRACKSTATION,
                    MODID,
                    "ShowFPSButton",
                    "ShowFPS/PluginData/fps-38",
                    "ShowFPS/PluginData/fps-24",
                    MODNAME
                );
            }

            labelStyle = new GUIStyle(GUI.skin.label);

            // resize button
            resizeBoxStyle = new GUIStyle(GUI.skin.box);
            resizeBoxStyle.fontSize = 10;
            resizeBoxStyle.normal.textColor = XKCDColors.LightGrey;

            greenFont = new GUIStyle(GUI.skin.label);
            redFont = new GUIStyle(GUI.skin.label);
            redButtonFont = new GUIStyle(GUI.skin.button);
            redFont.fontStyle = FontStyle.Bold;
            redFont.normal.textColor = Color.red;

            redButtonFont.fontStyle = FontStyle.Bold;
            redButtonFont.normal.textColor =
                redButtonFont.hover.textColor = Color.red;
            greenFont.normal.textColor = Color.green;

            GameEvents.onShowUI.Add(ShowGUI);
            GameEvents.onHideUI.Add(HideGUI);
        }

        bool isKSPGUIActive = true;
        void ShowGUI()
        {
            isKSPGUIActive = true;
        }

        void HideGUI()
        {
            isKSPGUIActive = false;
        }



        internal void InitGraphWindow()
        {
            LabelWidth = Settings.GraphWidth;
            WndWidth = Settings.GraphWidth + 8;
            WndHeight = Settings.GraphHeight + 42;

            keyToggleWindow = Settings.keyToggleWindow;
            keyScaleUp = Settings.keyScaleUp;
            keyScaleDown = Settings.keyScaleDown;

            windowPos.Set(Settings.winX, Settings.winY, WndWidth, WndHeight);
            windowDragRect.Set(0, 0, WndWidth, WndHeight);
            labelRect.Set(LabelX, LabelY, LabelWidth, LabelHeight);
            graphRect.Set(GraphX, GraphY, Settings.GraphWidth, Settings.GraphHeight);

            texGraph = new Texture2D(Settings.GraphWidth, Settings.GraphHeight, TextureFormat.ARGB32, false);

            yellowLine = new Color[Settings.GraphHeight];
            redLine = new Color[Settings.GraphHeight];
            greenLine = new Color[Settings.GraphHeight];
            blueLine = new Color[Settings.GraphHeight];
            greyLine = new Color[Settings.GraphHeight];
            blackLine = new Color[Settings.GraphHeight];
            for (int i = 0; i < blackLine.Length; i++)
            {
                blackLine[i] = Color.black;
                blackLine[i].a = Alpha;

                yellowLine[i] = Color.yellow;
                redLine[i] = Color.red;
                greenLine[i] = Color.green;
                blueLine[i] = Color.blue;
                greyLine[i] = Color.grey;
            }

            for (int i = 0; i < Settings.GraphWidth; i++)
                texGraph.SetPixels(i, 0, 1, Settings.GraphHeight, blackLine);

            this.resizeHandle = new ResizeHandle();

            RedrawGraph();
        }

        void Toggle()
        {
            showUI = !showUI;
            if (!showUI)
            {
                showHelp = false;
                SaveWinSettings();
            }
            else
            {
                showPerfectSym = Settings.showPerfectSym;
                periodicRescale = Settings.periodicRescale;
                Alpha = Settings.alpha;
                FPSCounter.frequency = Settings.frequency;
                instance.InitGraphWindow();
            }
        }

        void Awake()
        {
            DontDestroyOnLoad(this);

            instance = this;
            windowId = Guid.NewGuid().GetHashCode();
            windowTitle = "Show FPS";

            valCycle = new float[] { 5, 10, 15, 30, 40, 50, 60, 70, 80, 90, 100, 120, 150, 200 };
            numScales = valCycle.Length;

            helpWinPos.Set(40, 40, 500, 100);

            showUI = startVisible;

            // Force a full update of the graph texture
            fullUpdate = true;

            fpsValues = new float[Screen.width, 3];
            Array.Clear(fpsValues, 0, Settings.GraphWidth * 3);
        }


        void Start()
        {
            areaStyle = new GUIStyle(HighLogic.Skin.textArea);
            areaStyle.richText = true;
        }

        internal void AddFPSValue(float fps, float symRate)
        {
            if (movingAvg == null)
                movingAvg = fps;
            else
                movingAvg = movingAvg * .9f + fps * .1f;

            curFPS = fpsValues[valIndex, FPS] = fps;
            fpsValues[valIndex, SYMRATE] = symRate * SYM_MULT;

            if (!resizeHandle.resizing)
            {
                fpsValues[valIndex, FPS_AVG] = (float)movingAvg;

                valIndex = (valIndex + 1) % Settings.GraphWidth;
                if (valIndex == lastRendered)
                    fullUpdate = true;
            }
        }

        void UpdateGuiStr()
        {
            guiStr = "Scale: " + valCycle[scaleIndex].ToString("F0") + "fps" + "        Current FPS: " + curFPS.ToString("F1") +
                "        Moving Average FPS: " + ((float)movingAvg).ToString("F1");
        }

        void Update()
        {
            if (GameSettings.MODIFIER_KEY.GetKey())
            {
                if (Input.GetKeyDown(keyToggleWindow))
                {
                    showUI = !showUI;
                    if (!showUI)
                        showHelp = false;
                }
                if (Input.GetKeyDown(keyScaleUp))
                {
                    // Increase scale
                    scaleIndex = (scaleIndex + 1) % numScales;
                    fullUpdate = true;
                }
                if (Input.GetKeyDown(keyScaleDown))
                {
                    // Decrease scale
                    scaleIndex = (scaleIndex + numScales - 1) % numScales;
                    fullUpdate = true;
                }
            }

            if (!showUI)
                return;

            if (fullUpdate)
            {
                fullUpdate = false;
                lastRendered = (valIndex + 1) % Settings.GraphWidth;
            }

            // If we want to update this time
            if (lastRendered != valIndex)
            {

                // We're going to wrap this back round to the start so copy the value so 
                int startlastRend = lastRendered;

                // Update the columns from lastRendered to valIndex wrapping round at the end
                if (startlastRend >= valIndex)
                {
                    for (int x = startlastRend; x < Settings.GraphWidth; x++)
                    {
                        //DrawColumn(texGraph, x, (int)((double)fpsValues[x, 0] * Settings.GraphHeight / scale), greenLine,  blackLine);
                        DrawData(x);
                    }

                    startlastRend = 0;
                }

                for (int x = startlastRend; x < valIndex; x++)
                {
                    //DrawColumn(texGraph, x, (int)((double)fpsValues[x, 0] * Settings.GraphHeight / scale), greenLine,  blackLine);
                    DrawData(x);
                }

                if (valIndex < Settings.GraphWidth)
                    texGraph.SetPixels(valIndex, 0, 1, Settings.GraphHeight, blueLine);
                if (valIndex != Settings.GraphWidth - 2)
                    texGraph.SetPixels((valIndex + 1) % Settings.GraphWidth, 0, 1, Settings.GraphHeight, blackLine);
                texGraph.Apply();

                lastRendered = valIndex;
            }
        }

        void DrawData(int x)
        {
            float scale = valCycle[scaleIndex];
            float heightAdj = Settings.GraphHeight / scale;

            if (fpsValues[x, FPS] > 0)
                DrawLine(texGraph, x, (int)((double)fpsValues[x, FPS] * heightAdj), greenLine, blackLine);
            if (fpsValues[x, FPS_AVG] > 0)
                DrawLine(texGraph, x, (int)((double)fpsValues[x, FPS_AVG] * heightAdj), yellowLine, null);
            if (showPerfectSym)
                DrawLine(texGraph, x, (int)((double)SYM_MULT * Settings.GraphHeight), greyLine, null);
            if (fpsValues[x, SYMRATE] > 0)
                DrawLine(texGraph, x, (int)((double)fpsValues[x, SYMRATE] * Settings.GraphHeight), redLine, null);

        }
        void RedrawGraph()
        {
            for (int x = 0; x < Settings.GraphWidth; x++)
            {
                DrawData(x);
            }
        }

#if false
        void DrawColumn(Texture2D tex, int x, int y, Color[] fgcol, Color[] bgcol)
        {
            if (y > Settings.GraphHeight - 1)
                y = Settings.GraphHeight - 1;
            tex.SetPixels(x, 0, 1, y + 1, fgcol);
            if (y < Settings.GraphHeight - 1)
                tex.SetPixels(x, y + 1, 1, Settings.GraphHeight - 1 - y, bgcol);
        }
#endif

        const int LINE_WIDTH = 3;
        void DrawLine(Texture2D tex, int x, int y, Color[] fgcol, Color[] bgcol)
        {
            if (y > Settings.GraphHeight - LINE_WIDTH)
                y = Settings.GraphHeight - LINE_WIDTH;

            if (bgcol != null) tex.SetPixels(x, 0, 1, Settings.GraphHeight, bgcol);
            tex.SetPixels(x, y, 1, LINE_WIDTH, fgcol);
        }

        void OnGUI()
        {
            if (isKSPGUIActive)
            {
                if (showUI)
                {
                    windowPos = ClickThruBlocker.GUILayoutWindow(windowId, windowPos, WindowGUI, windowTitle);
                    // do this here since if it's done within the window you only recieve events that are inside of the window
                    this.resizeHandle.DoResize(ref this.windowPos);
                }

                if (showHelp)
                    helpWinPos = ClickThruBlocker.GUILayoutWindow(windowId + 1, helpWinPos, helpWin, "ShowFPS Help");
            }
        }

        bool showPerfectSym = false;
        bool periodicRescale = false;
        double lastRescaleTime = 0;

        void WindowGUI(int windowID)
        {
            if (GUI.Button(new Rect(4, 3f, 22f, 15f), new GUIContent("x"), redButtonFont))
                showUI = false;

            if (GUI.Button(new Rect(windowPos.width - 22, 2, 18, 15), "?"))
                showHelp = !showHelp;
            UpdateGuiStr();
            GUILayout.BeginHorizontal();
            GUILayout.Label(guiStr); //, labelStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh"))
                instance.InitGraphWindow();
            GUILayout.Space(10);
            if (GUILayout.Button("Clear"))
            {
                Array.Clear(fpsValues, 0, Settings.GraphWidth * 3);
                valIndex = lastRendered = 0;
                instance.InitGraphWindow();
            }
            GUILayout.Space(10);
            if (GUILayout.Button("Rescale", GUILayout.Width(70)) || (periodicRescale && Planetarium.fetch.time - lastRescaleTime >= 60f))
            {
                float maxHeight = 5;
                int oldScaleIndex = scaleIndex;
                lastRescaleTime = Planetarium.fetch.time;
                for (int i = 0; i < Settings.GraphWidth; i++)
                    maxHeight = Math.Max(maxHeight, fpsValues[i, FPS]);

                for (int i = 0; i < valCycle.Length; i++)
                    if (maxHeight + 1 >= valCycle[i])
                        scaleIndex = i + 1;
                    else break;
                scaleIndex = Math.Min(valCycle.Length - 1, scaleIndex);
                if (oldScaleIndex != scaleIndex)
                    RedrawGraph();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            var oShowPerfectSym = showPerfectSym;
            var oPeriodicRescale = periodicRescale;

            showPerfectSym = GUILayout.Toggle(showPerfectSym, "Show Max Symrate");
            GUILayout.FlexibleSpace();
            periodicRescale = GUILayout.Toggle(periodicRescale, "Periodic auto-rescale");
            GUILayout.EndHorizontal();
            if (!resizeHandle.resizing)
            {
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical();
                GUILayout.Label(valCycle[scaleIndex].ToString("F0"), greenFont);
                GUILayout.FlexibleSpace();
                GUILayout.Label((valCycle[scaleIndex] * .75).ToString("F0"), greenFont);
                GUILayout.FlexibleSpace();
                GUILayout.Label((valCycle[scaleIndex] * .5).ToString("F0"), greenFont);
                GUILayout.FlexibleSpace();
                GUILayout.Label((valCycle[scaleIndex] * .25).ToString("F0"), greenFont);
                GUILayout.FlexibleSpace();
                GUILayout.Label(" 0", greenFont);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Box(texGraph, labelStyle);
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                GUILayout.Label(" ");
                GUILayout.FlexibleSpace();
                GUILayout.Label(" ");
                GUILayout.FlexibleSpace();
                GUILayout.Label("1", redFont);
                GUILayout.FlexibleSpace();
                GUILayout.Label("0.5",redFont );
                GUILayout.FlexibleSpace();
                GUILayout.Label("0", redFont);
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Transparency:", GUILayout.Width(130));
            var oAlpha = Alpha;
            Alpha = GUILayout.HorizontalSlider(Alpha, 0.1f, 1f, GUILayout.Width(130));
            if (oAlpha != Alpha)
            {
                blackLine = new Color[Settings.GraphHeight];
                for (int i = 0; i < blackLine.Length; i++)
                    blackLine[i].a = Alpha;
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label("Frequency (" + FPSCounter.frequency.ToString("F2") + "s):", GUILayout.Width(130));
            var oFreq = FPSCounter.frequency;
            FPSCounter.frequency = GUILayout.HorizontalSlider(FPSCounter.frequency, 0.25f, 1f, GUILayout.Width(130));

            GUILayout.EndHorizontal();

            this.resizeHandle.Draw(ref this.windowPos);

            GUI.DragWindow(windowDragRect);
            if (oShowPerfectSym != showPerfectSym ||
                oPeriodicRescale != periodicRescale ||
                oAlpha != Alpha ||
                oFreq != FPSCounter.frequency)
            {
                SaveWinSettings();
            }
        }

        void SaveWinSettings()
        {
                Settings.showPerfectSym = showPerfectSym;
                Settings.periodicRescale = periodicRescale;
                Settings.alpha = Alpha;
                Settings.frequency = FPSCounter.frequency;
                Settings.winX = windowPos.x;
                Settings.winY = windowPos.y;
                Settings.SaveConfig();
        }

        float Alpha = 1;
        static GUIStyle areaStyle;

        const string helpText1 =
            "<B><color=yellow>General Controls</color></B>\n\n" +
            "<B>Mod-KeypadMultiply</B> toggles the display of the window.\n" +
            "<B>Mod-KeypadPlus</B> increases the vertical scale of the graph.\n" +
            "<B>Mod-KeypadMinus</B> decreases the vertical scale of the graph.\n\n" +

            "<B><color=yellow>Legend</color>\n\n</B>\b" +
            "<color=green>Green</color>      FPS﻿\n" +
            "<color=yellow>Yellow</color>     FPS Average﻿\n" +
            "<color=red>Red</color>        Simulation Rate\n" +
            "<color=grey>Grey</color>      Max Sim Rate";

        void helpWin(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.TextArea(helpText1, areaStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close"))
                showHelp = false;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUI.DragWindow();
        }


        private class ResizeHandle
        {
            internal bool resizing;
            private Vector2 lastPosition = new Vector2(0, 0);
            private const float resizeBoxSize = 18;
            private const float resizeBoxMargin = 2;

            internal void Draw(ref Rect winRect)
            {

                var resizer = new Rect(winRect.width - resizeBoxSize - resizeBoxMargin, winRect.height - resizeBoxSize - resizeBoxMargin, resizeBoxSize, resizeBoxSize);
                GUI.Box(resizer, "//", resizeBoxStyle);

                if (!Event.current.isMouse)
                {
                    return;
                }

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
                    resizer.Contains(Event.current.mousePosition))
                {
                    this.resizing = true;
                    this.lastPosition.x = Input.mousePosition.x;
                    this.lastPosition.y = Input.mousePosition.y;

                    Event.current.Use();
                }
            }

            internal void DoResize(ref Rect winRect)
            {
                if (!this.resizing)
                {
                    return;
                }

                if (Input.GetMouseButton(0))
                {
                    var deltaX = Input.mousePosition.x - this.lastPosition.x;
                    var deltaY = Input.mousePosition.y - this.lastPosition.y;

                    //Event.current.delta does not make resizing very smooth.

                    this.lastPosition.x = Input.mousePosition.x;
                    this.lastPosition.y = Input.mousePosition.y;

                    winRect.width += deltaX;
                    winRect.height -= deltaY;


                    Settings.GraphWidth = (int)winRect.width - 8;
                    Settings.GraphHeight = (int)winRect.height - 42;

                    if (Event.current.isMouse)
                    {
                        Event.current.Use();
                    }
                }

                if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                {
                    this.resizing = false;

                    Event.current.Use();
                    instance.InitGraphWindow();
                }
            }
        } // ResizeHandle


    }
}
