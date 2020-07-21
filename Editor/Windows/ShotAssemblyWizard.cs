using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Control = PixelWizards.ShotAssembly.ShotAssemblyController;
using Loc = PixelWizards.ShotAssembly.ShotAssemblyLoc;                                 // string localization table
using API = PixelWizards.ShotAssembly.ShotAssemblyAPI;

namespace PixelWizards.ShotAssembly
{
    /// <summary>
    /// Localization table
    /// </summary>
    public class ShotAssemblyLoc
    {
        public const string MENUITEMPATH = "Window/Sequencing/Shot Assembly Wizard";
        public const string WINDOWTITLE = "Shot Assembly Wizard";
        public const string WINDOW_HEADER = "Shot Assembly";
        public const string HELP_HEADER = "This wizard is designed to help you easily assemble a sequence of animation clips for review";
        public const string ERROR_NOPREFAB = "Need to specify an Actor (Skinned Mesh Renderer) to assign the animations to!";
        public const string ERROR_NOANIM = "No animations in search query? Need to filter for animations first!";
        public const string ERROR_NOTIMELINE = "If you want to use an existing timeline, you need to select a playable director from the currently loaded scene";
        public const string ERROR_TITLE = "Error";
        public const string BUTTON_OK = "Ok";
        public const string BUTTON_SEARCH = "Search";
        public const string BUTTON_CANCEL = "Cancel";
        public const string WARNING_ANIMFILTEREMPTY_TITLE = "Animation Filter is Empty!";
        public const string WARNING_ANIMFILTEREMPTY_MSG = "The animation filter is empty, this will search your entire project (and may take a while), are you sure?";
        public const string FOLDOUT_TIMELINE = "Timeline & Scene";
        public const string FOLDOUT_ACTOR = "Actors";
        public const string FOLDOUT_ANIM = "Animations";
        public const string TOGGLE_CREATENEWSCENE = "Create a new Scene";
        public const string TOOLTIP_CREATENEWSCENE = "Create a new empty scene instead of adding the timeline to the existing scene";
        public const string TOGGLE_EXISTINGTIMELINE = "Use Existing Timeline";
        public const string TOOLTIP_EXISTINGTIMELINE = "Use an existing playable director in the open scene instead of creating a new one";
        public const string LABEL_USEEXISTING = "Use Existing Timeline";
        public const string LABEL_SELECTTIMELINE = "Select Timeline";
        public const string TOOLTIP_SELECTTIMELINE = "Choose a GameObject with a Playable Director in the currently loaded scene";
        public const string LABEL_CREATETIMELINE = "Create New Timeline";
        public const string LABEL_TIMELINENAME = "Timeline Name";
        public const string TOOLTIP_TIMELINENAME = "Enter a name for the timeline you wish to create, or leave empty to auto-generate a name";
        public const string LABEL_TIMELINEPATH = "Timeline Path";
        public const string TOOLTIP_TIMELINEPATH = "Add a path to save the output Timeline asset";
        public const string LABEL_ACTORPREFAB = "Actor Prefab";
        public const string TOOLTIP_ACTORPREFAB = "Add a reference to the Actor prefab to be used";
        public const string LABEL_ANIMBASEPATH = "Anim Base Path:";
        public const string TOOLTIP_ANIMBASEPATH = "Enter a path to the animations you wish to load";
        public const string LABEL_ANIMFILTER = "Filter";
        public const string TOOLTIP_ANIMFILTER = "Enter a search filter for the animation names to search.";
        public const string LABEL_ANIMTRACKSETTING = "One Anim per Track";
        public const string TOOLTIP_ANIMTRACKSETTING = "Instead of adding all animations to one track for the character, instead create multiple instances and add one animation per track (for example, crowd anims)";
        public const string BUTTON_STEP1 = "Step 1: Search Anims";
        public const string TOOLTIP_BUTTONSTEP1 = "Enter a search path and/or filter for animations then run the search query.";
        public const string BUTTON_STEP2 = "Step 2: Assemble Sequence";
        public const string TOOLTIP_BUTTONSTEP2 = "Generates the timeline sequence, instances the actor prefab and applies all of the filtered animations to the selected actor";
        public const string BUTTON_STEP3 = "Step 3: Export .unitypackage";
        public const string TOOLTIP_BUTTONSTEP3 = "Optionally exports the resulting scene as a .unitypackage as part of an animreview / pipeline publish step";

        public const string BUTTON_GENERATORCONFIG = "Path to Config: ";
        public const string HELP_GENERATORHEADER = "Browse to a shot config.";
        public const string BUTTON_GENERATESHOT = "Generate Shot";

        public const string LABEL_SCENENAME = "Scene Name";
        public const string LABEL_SCENEPATH = "Scene Path:";
        public const string LABEL_CREATENEWTIMELINE = "Create new Timeline";
        public const string HELP_NEWSCENEYOUMUSTCREATENEWTIMELINE = "If you are creating a new scene, you MUST create a new Timeline as well";

    }

    /// <summary>
    /// The main scene Assembly Window
    /// </summary>
    public class ShotAssemblyWizard : EditorWindow
    {
        /// <summary>
        /// the foldout toggle states - open them by default
        /// </summary>
        private class WizardState
        {
            public bool sceneState = true;
            public bool timelinestate = true;
            public bool actorState = true;
            public bool animState = true;
        }

        private enum TABBAR
        {
            TAB_WIZARD =0,
            TAB_GENERATOR =1,
        }

        private static WizardState state = new WizardState();

        private static Vector2 minWindowSize = new Vector2(250, 300);
        private static Vector2 scrollPosition = Vector2.zero;
        private string[] tabbar = { "Wizard", "Load Config" };
        private int activeTab = 0;
        private float leftColumnWidth = 100f;

        [MenuItem(Loc.MENUITEMPATH)]
        private static void ShowWindow()
        {
            var thisWindow = GetWindow<ShotAssemblyWizard>(false, Loc.WINDOWTITLE, true);
            thisWindow.minSize = minWindowSize;
            thisWindow.Reset();
        }

        private void OnEnable()
        {
            Reset();
        }

        private void Reset()
        {
            Control.Init();
        }

        private void OnGUI()
        {
            GUILayout.Space(10f);
           // GUILayout.BeginVertical();
            {
                GUILayout.Space(5f);
                GUILayout.Label(Loc.WINDOW_HEADER, EditorStyles.boldLabel);
                GUILayout.Space(5f);
                GUILayout.Label(Loc.HELP_HEADER, EditorStyles.helpBox);

                GUILayout.Space(10f);

                DoTopTabs();
                GUILayout.Space(10f);

                switch ( activeTab)
                {
                    case (int)TABBAR.TAB_WIZARD:
                        {
                            state.timelinestate = EditorGUILayout.Foldout(state.timelinestate, Loc.FOLDOUT_TIMELINE);
                            if (state.timelinestate)
                            {
                                DoTimelineUI();
                                GUILayout.Space(10f);
                            }
                            state.actorState = EditorGUILayout.Foldout(state.actorState, Loc.FOLDOUT_ACTOR);
                            if (state.actorState)
                            {
                                DoActorUI();
                                GUILayout.Space(10f);
                            }
                            state.animState = EditorGUILayout.Foldout(state.animState, Loc.FOLDOUT_ANIM);
                            if (state.animState)
                            {
                                DoAnimUI();
                                GUILayout.Space(10f);
                            }
                            DoProcessUI();
                            break;
                        }
                    case (int)TABBAR.TAB_GENERATOR:
                        {
                            DoGeneratorUI();
                            break;
                        }
                }
            }
          //  GUILayout.EndVertical();
        }

        private void DoTopTabs()
        {
            activeTab = GUILayout.Toolbar(activeTab, tabbar, GUILayout.Height(35f));
        }

        private void DoGeneratorUI()
        {
            // pick shot config
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Space(5f);
                GUILayout.BeginVertical();
                {
                    GUILayout.Label(Loc.HELP_GENERATORHEADER, EditorStyles.helpBox);
                    GUILayout.Space(5f);

                    // browse for the config
                    RenderFileBrowserField(Loc.BUTTON_GENERATORCONFIG, ref Control.shotGen.generatorConfig, "Assets");
                    GUILayout.Space(5f);
                }
                GUILayout.Space(5f);
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);

            // configure the scene
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Space(5f);
                GUILayout.BeginVertical();
                {
                    Control.shotGen.createNewScene = GUILayout.Toggle(Control.shotGen.createNewScene, Loc.TOGGLE_CREATENEWSCENE);
                    GUILayout.Space(5f);

                    GUILayout.Label(Loc.TOOLTIP_CREATENEWSCENE, EditorStyles.helpBox);
                    GUILayout.Space(5f);

                    RenderTextField(Loc.LABEL_SCENENAME, ref Control.shotGen.timelineName);
                    GUILayout.Space(5f);

                    RenderPathBrowserField(Loc.LABEL_SCENEPATH, ref Control.shotGen.scenePath, "Assets");
                    GUILayout.Space(5f);
                }
                GUILayout.Space(5f);
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);

            // configure the timeline
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Space(5f);
                GUILayout.BeginVertical();
                {
                    if (Control.shotGen.createNewScene)
                    {
                        GUILayout.Label(Loc.LABEL_CREATENEWTIMELINE, EditorStyles.boldLabel);
                        GUILayout.Space(5f);
                        GUILayout.Label(Loc.HELP_NEWSCENEYOUMUSTCREATENEWTIMELINE, EditorStyles.helpBox);
                        GUILayout.Space(5f);
                        Control.shotGen.useExistingTimeline = false;
                    }
                    else
                    {
                        // timeline path
                        Control.shotGen.useExistingTimeline = GUILayout.Toggle(Control.shotGen.useExistingTimeline, Loc.TOGGLE_EXISTINGTIMELINE);
                        GUILayout.Space(5f);

                        GUILayout.Label(Loc.TOOLTIP_EXISTINGTIMELINE, EditorStyles.helpBox);
                        GUILayout.Space(5f);
                    }

                    RenderTextField(Loc.LABEL_TIMELINENAME, ref Control.shotGen.timelineName);
                    GUILayout.Space(5f);

                    RenderPathBrowserField(Loc.LABEL_TIMELINEPATH, ref Control.shotGen.timelinePath, "Assets");
                    GUILayout.Space(5f);
                }
                GUILayout.Space(5f);
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5f);

            // and generate the thing
            if ( GUILayout.Button(Loc.BUTTON_GENERATESHOT, GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
            {
                API.GenerateShot(Control.shotGen.generatorConfig, Control.shotGen.useExistingTimeline, Control.shotGen.timelineName, Control.shotGen.timelinePath, Control.shotGen.createNewScene, Control.shotGen.sceneName, Control.shotGen.scenePath);
            }
        }

        /// <summary>
        /// Renders the Timeline section of the UI
        /// </summary>
        private void DoTimelineUI()
        {
            // <TimelineSettings>
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Space(10f);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        {
                            var content = new GUIContent
                            {
                                text = Loc.TOGGLE_EXISTINGTIMELINE,
                                tooltip = Loc.TOOLTIP_EXISTINGTIMELINE
                            };
                            Control.model.useExistingTimeline = EditorGUILayout.Toggle(content, Control.model.useExistingTimeline);
                        }
                        
                        GUILayout.Space(5f);

                        if (Control.model.useExistingTimeline)
                        {
                            GUILayout.Label(Loc.LABEL_USEEXISTING, EditorStyles.boldLabel);
                            GUILayout.BeginHorizontal();
                            {
                                var content = new GUIContent
                                {
                                    text = Loc.LABEL_SELECTTIMELINE,
                                    tooltip = Loc.TOOLTIP_SELECTTIMELINE
                                };
                                GUILayout.Label(content, GUILayout.Width(100f));
                                Control.model.existingTimeline = EditorGUILayout.ObjectField(Control.model.existingTimeline, typeof(PlayableDirector), true, GUILayout.ExpandWidth(true)) as PlayableDirector;
                            }
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.Label(Loc.LABEL_CREATETIMELINE, EditorStyles.boldLabel);
                            GUILayout.BeginHorizontal();
                            {
                                var content = new GUIContent
                                {
                                    text = Loc.LABEL_TIMELINENAME,
                                    tooltip = Loc.TOOLTIP_TIMELINENAME
                                };
                                GUILayout.Label(content, GUILayout.Width(100f));
                                Control.model.timelineName = GUILayout.TextField(Control.model.timelineName, GUILayout.ExpandWidth(true));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.Space(5f);

                            GUILayout.BeginHorizontal();
                            {
                                var content = new GUIContent
                                {
                                    text = Loc.LABEL_TIMELINEPATH,
                                    tooltip = Loc.TOOLTIP_TIMELINEPATH
                                };
                                GUILayout.Label(content, GUILayout.Width(100f));
                                GUILayout.Label("Assets/", GUILayout.Width(45f));
                                Control.model.timelinePath = GUILayout.TextField(Control.model.timelinePath, GUILayout.ExpandWidth(true));
                            }
                            GUILayout.EndHorizontal();

                            GUILayout.Space(5f);

                            GUILayout.BeginHorizontal();
                            {
                                var content = new GUIContent
                                {
                                    text = Loc.TOGGLE_CREATENEWSCENE,
                                    tooltip = Loc.TOOLTIP_CREATENEWSCENE
                                };
                                Control.model.createNewScene = EditorGUILayout.Toggle(content, Control.model.createNewScene);
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.Space(5f);
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(5f);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
            // </TimelineSettings>
        }

        /// <summary>
        /// Renders the Actor / Prefab selection UI
        /// </summary>
        private void DoActorUI()
        {
            // <ActorSettings>
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Space(10f);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(5f);
                        GUILayout.BeginHorizontal();
                        {
                            var content = new GUIContent
                            {
                                text = Loc.LABEL_ACTORPREFAB,
                                tooltip = Loc.TOOLTIP_ACTORPREFAB
                            };
                            GUILayout.Label(content, GUILayout.Width(100f));
                            Control.model.actorPrefab = EditorGUILayout.ObjectField(Control.model.actorPrefab, typeof(GameObject), false, GUILayout.ExpandWidth(true)) as GameObject;
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5f);
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(5f);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
            // </ActorSettings>
        }

        /// <summary>
        /// Renders the Animation filter / path section of the UI
        /// </summary>
        private void DoAnimUI()
        {
            // <AnimationSettings>
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Space(10f);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Space(5f);
                        GUILayout.BeginHorizontal();
                        {
                            var content = new GUIContent
                            {
                                text = Loc.LABEL_ANIMBASEPATH,
                                tooltip = Loc.TOOLTIP_ANIMBASEPATH
                            };
                            GUILayout.Label(content, GUILayout.Width(100f));
                            GUILayout.Label("Assets/", GUILayout.Width(45f));
                            Control.model.animationPath = GUILayout.TextField(Control.model.animationPath, GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5f);
                        GUILayout.BeginHorizontal();
                        {
                            var content = new GUIContent
                            {
                                text = Loc.LABEL_ANIMFILTER,
                                tooltip = Loc.TOOLTIP_ANIMFILTER
                            };
                            GUILayout.Label(content, GUILayout.Width(100f));
                            Control.model.animFilter = GUILayout.TextField(Control.model.animFilter, GUILayout.ExpandWidth(true));
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5f);
                        GUILayout.BeginHorizontal();
                        {
                            var content = new GUIContent
                            {
                                text = Loc.LABEL_ANIMTRACKSETTING,
                                tooltip = Loc.TOOLTIP_ANIMTRACKSETTING
                            };
                            Control.model.oneTrackPerAnim = EditorGUILayout.Toggle(content, Control.model.oneTrackPerAnim);
                            GUILayout.Space(5f);
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5f);
                    }
                    GUILayout.EndVertical();
                    GUILayout.Space(5f);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
            // </AnimationSettings>

            // show a list of animations that match the filter in the specified folder
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5f);
                GUILayout.BeginHorizontal(GUI.skin.textArea);
                {
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandWidth(true), GUILayout.Height(200f));
                    {
                        foreach (var anim in Control.model.animationList)
                        {
                            GUILayout.Label(anim.name, GUILayout.ExpandWidth(true), GUILayout.Width(460f));
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5f);
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Renders the bottom action step buttons
        /// </summary>
        private void DoProcessUI()
        {
            // process the files
            GUILayout.BeginVertical(GUILayout.MinWidth(350f));
            {
                GUILayout.Space(10f);
                var content = new GUIContent
                {
                    text = Loc.BUTTON_STEP1,
                    tooltip = Loc.TOOLTIP_BUTTONSTEP1
                };
                if (GUILayout.Button(content, GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                {
                    Control.RefreshAnimationList();
                }
                GUILayout.Space(5f);

                content = new GUIContent
                {
                    text = Loc.BUTTON_STEP2,
                    tooltip = Loc.TOOLTIP_BUTTONSTEP2
                };
                if (GUILayout.Button(content, GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                {
                    Control.Process();
                }
                GUILayout.Space(5f);

                content = new GUIContent
                {
                    text = Loc.BUTTON_STEP3,
                    tooltip = Loc.TOOLTIP_BUTTONSTEP3
                };
                if (GUILayout.Button(content, GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                {
                    Control.Export();
                }
            }
            GUILayout.BeginVertical();
        }


        private void RenderFileBrowserField(string loc, ref string field, string startingPath)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(loc, GUILayout.Width(leftColumnWidth));
                field = GUILayout.TextField(field, GUILayout.MaxWidth(position.width - 145f));
                if (GUILayout.Button("...", GUILayout.Width(35f)))
                {
                    field = EditorUtility.OpenFilePanel("Browse for Config", startingPath, "");
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2f);
        }

        private void RenderTextField(string loc, ref string field)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(loc, GUILayout.Width(leftColumnWidth));
                field = GUILayout.TextField(field, GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2f);
        }

        private void RenderPathBrowserField(string loc, ref string field, string startingPath)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(loc, GUILayout.Width(leftColumnWidth));
                field = GUILayout.TextField(field, GUILayout.MaxWidth(position.width - 145f));
                if (GUILayout.Button("...", GUILayout.Width(35f)))
                {
                    field = EditorUtility.OpenFolderPanel("Browse for Path", startingPath, "");
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2f);
        }
    }
}