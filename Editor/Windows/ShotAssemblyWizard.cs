using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Control = PixelWizards.ShotAssembly.ShotAssemblyController;
using Loc = PixelWizards.ShotAssembly.ShotAssemblyLoc;                                 // string localization table

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
        public const string FOLDOUT_TIMELINE = "Timeline";
        public const string FOLDOUT_ACTOR = "Actors";
        public const string FOLDOUT_ANIM = "Animations";
        public const string TOGGLE_EXISTINGTIMELINE = "Use Existing Timeline";
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

        public const string BUTTON_STEP1 = "Step 1: Search Anims";
        public const string BUTTON_STEP2 = "Step 2: Assemble Sequence";
        public const string BUTTON_STEP3 = "Step 3: Export .unitypackage";

    }

    /// <summary>
    /// The main scene Assembly Window
    /// </summary>
    public class ShotAssemblyWizard : EditorWindow
    {
        private class WizardState
        {
            public bool sceneState = false;
            public bool timelinestate = false;
            public bool actorState = false;
            public bool animState = false;
        }

        private static WizardState state = new WizardState();

        private static Vector2 minWindowSize = new Vector2(250, 300);
        private static Vector2 scrollPosition = Vector2.zero;

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

                state.timelinestate = EditorGUILayout.Foldout(state.timelinestate, Loc.FOLDOUT_TIMELINE);
                if( state.timelinestate)
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
            }
          //  GUILayout.EndVertical();
        }
        
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
                        Control.model.useExistingTimeline = EditorGUILayout.Toggle(Loc.TOGGLE_EXISTINGTIMELINE, Control.model.useExistingTimeline);
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
        private void DoProcessUI()
        {
            // process the files
            GUILayout.BeginVertical(GUILayout.MinWidth(350f));
            {
                GUILayout.Space(10f);
                if (GUILayout.Button(Loc.BUTTON_STEP1, GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                {
                    Control.RefreshAnimationList();
                }
                GUILayout.Space(5f);
                if (GUILayout.Button(Loc.BUTTON_STEP2, GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                {
                    Control.Process();
                }
                GUILayout.Space(5f);
                if (GUILayout.Button(Loc.BUTTON_STEP3, GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                {
                    Control.Export();
                }
            }
            GUILayout.BeginVertical();
        }
    }
}