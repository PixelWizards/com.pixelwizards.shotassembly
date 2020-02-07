using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
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

                state.timelinestate = EditorGUILayout.Foldout(state.timelinestate, "Timeline");
                if( state.timelinestate)
                {
                    DoTimelineUI();
                    GUILayout.Space(10f);
                }
                state.actorState = EditorGUILayout.Foldout(state.actorState, "Actors");
                if (state.actorState)
                {
                    DoActorUI();
                    GUILayout.Space(10f);
                }
                state.animState = EditorGUILayout.Foldout(state.animState, "Animations");
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
                        GUILayout.Space(5f);
                        GUILayout.BeginHorizontal();
                        {
                            var content = new GUIContent
                            {
                                text = "Timeline Name",
                                tooltip = "Enter a name for the timeline you wish to create, or leave empty to auto-generate a name"
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
                                text = "Timeline Path",
                                tooltip = "Add a path to save the output Timeline asset"
                            };
                            GUILayout.Label(content, GUILayout.Width(100f));
                            GUILayout.Label("Assets/", GUILayout.Width(45f));
                            Control.model.timelinePath = GUILayout.TextField(Control.model.timelinePath, GUILayout.ExpandWidth(true));
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
                                text = "Actor Prefab",
                                tooltip = "Add a reference to the Actor prefab to be used"
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
                                text = "Anim Base Path:",
                                tooltip = "Enter a path to the animations you wish to load"
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
                                text = "Filter",
                                tooltip = "Enter a search filter for the animation names to search."
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
                if (GUILayout.Button("Step 1: Search Anims", GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                {
                    Control.RefreshAnimationList();
                }
                GUILayout.Space(5f);
                if (GUILayout.Button("Step 2: Assemble Sequence", GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                {
                    Control.Process();
                }
                GUILayout.Space(5f);
                if (GUILayout.Button("Step 3: Export .unitypackage", GUILayout.ExpandWidth(true), GUILayout.Height(35f)))
                {
                    Control.Export();
                }
            }
            GUILayout.BeginVertical();
        }
    }
}