using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using Loc = PixelWizards.ShotAssembly.ShotAssemblyLoc;                                 // string localization table
using API = PixelWizards.ShotAssembly.ShotAssemblyAPI;

namespace PixelWizards.ShotAssembly
{
    /// <summary>
    /// central data model for everything we need
    /// </summary>
    public class ShotAssemblyModel
    {
        public string sceneName = "AnimReview";
        public string scenePath = "Scenes";
        public SceneAsset sceneRef;
        public bool createNewScene = false;
        public string timelineName = "MasterTimeline";
        public string timelinePath = "Timeline";
        public bool useExistingTimeline = false;
        public PlayableDirector existingTimeline;
        public TimelineAsset timelineRef;
        public GameObject actorPrefab;
        public bool oneTrackPerAnim = false;            // if we want to instance the character tracks vertically instead of horizontally (one track per anim instead of all anims on one track)
        public string animationPath = "";
        public string animFilter;

        public List<AnimationClip> animationList = new List<AnimationClip>();
    }

    public class ShotAssemblyController
    {
        public static ShotAssemblyModel model = new ShotAssemblyModel();

        /// <summary>
        /// Initializes the data model when the window is opened
        /// </summary>
        public static void Init()
        {
            model = new ShotAssemblyModel();
        }

        /// <summary>
        /// Test method, generate shot
        /// </summary>
        [MenuItem("Assets/GenerateShot")]
        public static void GenerateShot()
        {
            API.GenerateShot("F:/projects/Adam.ShotAssembly/Assets/ADAM-AD1-Desert/ADAM-AD1-0010.json", false, "Timelines");
        }

        /// <summary>
        /// Find all of the animations in the specified path & filter
        /// </summary>
        public static void RefreshAnimationList()
        {
            if( model.animationPath.Length == 0)
            {
                if (!EditorUtility.DisplayDialog(Loc.WARNING_ANIMFILTEREMPTY_TITLE, Loc.WARNING_ANIMFILTEREMPTY_MSG, Loc.BUTTON_SEARCH, Loc.BUTTON_CANCEL))
                    return;
            }
            model.animationList = GetAnimations();
        }

        /// <summary>
        /// Finds all of the animationClips in the project, optionally filtered
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<AnimationClip> GetAnimations()
        {
            var searchPath = "Assets";
            if( model.animationPath.Length != 0)
            {
                searchPath += "/" + model.animationPath;
            }
            var animList = new List<AnimationClip>();
            var guids = AssetDatabase.FindAssets( model.animFilter + " t:AnimationClip", new[] { searchPath });
            foreach( var guid in guids)
            {
                var animPath = AssetDatabase.GUIDToAssetPath(guid);
                var anim = (AnimationClip)AssetDatabase.LoadAssetAtPath( animPath, typeof(AnimationClip));
               // Debug.Log("Anim: " + anim.name);
                animList.Add(anim);
            }

            return animList;
        }

        /// <summary>
        /// dbl checks to see that the user filled in the appropriate info: TODO: pop a modal to display the error output instead of console log
        /// </summary>
        /// <returns></returns>
        public static bool ValidateData()
        {
            var validateLog = new StringBuilder();

            if( model.actorPrefab == null)
            {
                validateLog.AppendLine(Loc.ERROR_NOPREFAB);
            }
            if (model.animationList.Count < 1)
            {
                validateLog.AppendLine(Loc.ERROR_NOANIM);
            }
            if( model.useExistingTimeline)
            {
                if( model.existingTimeline == null)
                {
                    validateLog.AppendLine(Loc.ERROR_NOTIMELINE);
                }
            }

            if( validateLog.Length > 0)
            {
               // Debug.Log(validateLog.ToString());
                EditorUtility.DisplayDialog(Loc.ERROR_TITLE, validateLog.ToString(), Loc.BUTTON_OK);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Run the shot assembly task with the given input
        /// </summary>
        public static void Process()
        {
            if( !ValidateData())
            {
                return;
            }
            PlayableDirector pd;
            // chcek if we're using an existing timeline or creating a new one
            if( !model.useExistingTimeline)
            {
                // do we need to make a new scene too?
                if( model.createNewScene)
                {
                    UnityUtilities.CreateNewScene(model.sceneName, model.scenePath);
                }
                pd = UnityUtilities.CreatePlayableDirector(model.timelineName, model.timelinePath);
            }
            else
            {
                pd = model.existingTimeline;
            }

            GenerateBaseTimelineTracks(pd);

            // set the scene selection to our new timeline so that we can scrub and see it
            Selection.activeObject = pd.gameObject;
        }

        /// <summary>
        /// Load the specified actor into the scene
        /// </summary>
        private static GameObject InstanceActorInScene()
        {
            if (model.actorPrefab == null)
                return null;

            return Object.Instantiate(model.actorPrefab);
        }

        /// <summary>
        /// Create the playable director, timeline sequence and add all of the animation tracks to the timeline
        /// </summary>
        /// <param name="pd"></param>
        private static void GenerateBaseTimelineTracks(PlayableDirector pd)
        {
            var timelineAsset = pd.playableAsset as TimelineAsset;

            if( model.oneTrackPerAnim)
            {
                // for each animation in the list, we want to instance an actor, create a track and bind the clip 
                foreach (var clip in model.animationList)
                {
                    var go = InstanceActorInScene();

                    var animTrack = timelineAsset.CreateTrack<AnimationTrack>(null, go.name );

                    // bind the track to our new actor instance
                    pd.SetGenericBinding(animTrack, go);

                    UnityUtilities.AddClipToAnimationTrack(animTrack, clip);
                }
            }
            else
            {
                var animTrack = timelineAsset.CreateTrack<AnimationTrack>(null, "Character Rig");

                // bind the track to our new actor instance
                var go = InstanceActorInScene();
                pd.SetGenericBinding(animTrack, go);

                // add the clips to Timeline
                foreach (var clip in model.animationList)
                {
                    UnityUtilities.AddClipToAnimationTrack(animTrack, clip);
                }
            }
            
            // add cinemachine stuff
            //var mainCam = GameObject.FindObjectOfType<Camera>();
            //var brain = mainCam.gameObject.AddComponent<CinemachineBrain>();

            //var go = new GameObject();
            //go.name = "LookDev VCam";
            //var vcam = go.AddComponent<CinemachineVirtualCamera>();
            //vcam.m_Follow = model.actorSceneInstance.transform;
            //vcam.m_LookAt = model.actorSceneInstance.transform;
            //// set the Body & Aim for the vcam
            //var vcamAim = vcam.AddCinemachineComponent<CinemachineComposer>();
            //var vcamBody = vcam.AddCinemachineComponent<CinemachineFramingTransposer>();

            //// and add a cinemachine track so we can see what we're animating
            //var cmTrack = timelineAsset.CreateTrack<CinemachineTrack>(null,  "Brain");

            //pd.SetGenericBinding(cmTrack, brain);
            
            //var cmClip = cmTrack.CreateDefaultClip();
            //cmClip.duration = pd.duration;
            //cmClip.displayName = go.name;
            //var cmAsset = cmClip.asset as CinemachineShot;
            //cmAsset.VirtualCamera.exposedName = "LookdevVCam";
            //pd.SetReferenceValue("LookdevVCam", vcam);
        }

        /// <summary>
        /// Exports the resulting scene out to a .unitypackage, including all dependencies
        /// </summary>
        public static void Export()
        {
            // save the scene first
            UnityUtilities.PromptToSaveScene(SceneManager.GetActiveScene());

            UnityUtilities.ExportPackage(model.scenePath, model.sceneName);
        }
    }
}