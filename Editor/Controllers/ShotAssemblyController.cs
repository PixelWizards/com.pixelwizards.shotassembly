using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

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
        public string timelineName = "MasterTimeline";
        public string timelinePath = "Timeline";
        public bool useExistingTimeline = false;
        public PlayableDirector existingTimeline;
        public TimelineAsset timelineRef;
        public GameObject actorPrefab;
        public GameObject actorSceneInstance;
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
        /// Find all of the animations in the specified path & filter
        /// </summary>
        public static void RefreshAnimationList()
        {
            if( model.animationPath.Length == 0)
            {
                if (!EditorUtility.DisplayDialog("Animation Filter is Empty!", "The animation filter is empty, this will search your entire project (and may take a while), are you sure?", "Search", "Cancel"))
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
                validateLog.AppendLine("Need to specify an Actor (Skinned Mesh Renderer) to assign the animations to!");
            }
            if (model.animationList.Count < 1)
            {
                validateLog.AppendLine("No animations in search query? Need to filter for animations first!");
            }
            if( model.useExistingTimeline)
            {
                if( model.existingTimeline == null)
                {
                    validateLog.AppendLine("If you want to use an existing timeline, you need to select a playable director from the currently loaded scene");
                }
            }

            if( validateLog.Length > 0)
            {
                Debug.Log(validateLog.ToString());
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
            Scene scene;
            PlayableDirector pd;
            if( !model.useExistingTimeline)
            {
                scene = UnityUtilities.CreateNewScene(model.sceneName, model.scenePath);
                pd = UnityUtilities.CreatePlayableDirector(model.timelineName, model.timelinePath);
            }
            else
            {
                scene = SceneManager.GetActiveScene();
                pd = model.existingTimeline;
            }

            InstanceActorInScene();
            GenerateBaseTimelineTracks(pd);
           // UnityUtilities.PromptToSaveScene(scene);

            // set the scene selection to our new timeline so that we can scrub and see it
            Selection.activeObject = pd.gameObject;
        }

        /// <summary>
        /// Load the specified actor into the scene
        /// </summary>
        private static void InstanceActorInScene()
        {
            if (model.actorPrefab == null)
                return;

            var go = GameObject.Instantiate(model.actorPrefab);
            if (go != null)
                model.actorSceneInstance = go;
        }

        /// <summary>
        /// Create the playable director, timeline sequence and add all of the animation tracks to the timeline
        /// </summary>
        /// <param name="pd"></param>
        private static void GenerateBaseTimelineTracks(PlayableDirector pd)
        {
            var timelineAsset = pd.playableAsset as TimelineAsset;

            var animTrack = timelineAsset.CreateTrack<AnimationTrack>(null, "Character Rig");

            // bind the track to our new actor instance
            pd.SetGenericBinding(animTrack, model.actorSceneInstance);

            // add the clips to Timeline
            foreach (var clip in model.animationList)
            {
                UnityUtilities.AddClipToAnimationTrack(animTrack, clip);
            }

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
            UnityUtilities.ExportPackage(model.scenePath, model.sceneName);
        }
    }
}