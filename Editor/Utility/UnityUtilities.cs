using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace PixelWizards.ShotAssembly
{
    public class UnityUtilities
    {
        /// <summary>
        /// Create a new scene, and optionally save it at the given path
        /// </summary>
        /// <param name="sceneName">name of the new scene</param>
        /// <param name="scenePath">path we want to save it to</param>
        /// <param name="autoSave">whether we want to automatically save it upon creation</param>
        /// <returns></returns>
        public static Scene CreateNewScene( string sceneName, string scenePath, bool autoSave = false)
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            newScene.name = sceneName;

            if( autoSave)
            {
                EditorSceneManager.SaveScene(newScene, "Assets/" + scenePath + "/" + sceneName + ".unity");
            }

            return newScene;
        }

        /// <summary>
        /// Create a new GameObject with a Playable director in the current scene. Automatically creates the timeline asset and saves it to the given folder
        /// </summary>
        /// <param name="timelineName">name of the playable director and timeline asset</param>
        /// <param name="timelinePath">folder to save the timeline asset itself</param>
        /// <returns></returns>
        public static PlayableDirector CreatePlayableDirector(string timelineName, string timelinePath)
        {
            // create the gameobject
            var go = new GameObject();
            go.name = timelineName;

            // add the playable director
            var pd = go.AddComponent<PlayableDirector>();

            // create the timeline asset
            var ta = CreateTimelineAsset(timelineName, timelinePath);

            // assign the asset to the playable director
            SetTimelinePlayableAsset(go, ta);

            return pd;
        }

        /// <summary>
        /// Adds a given animation clip to the specified timeline track
        /// </summary>
        /// <param name="track">the timeline track to add the clip to</param>
        /// <param name="clip">the animation clip to add</param>
        public static void AddClipToAnimationTrack(TrackAsset track, AnimationClip clip)
        {
            var newClip = track.CreateDefaultClip();
            newClip.duration = clip.length;
            newClip.displayName = clip.name;
            var animAsset = newClip.asset as AnimationPlayableAsset;
            animAsset.clip = clip;
        }

        /// <summary>
        /// create the timeline asset at the given path
        /// </summary>
        /// <param name="name">the name of the timeline asset to create</param>
        /// <param name="path">the path to save the asset into</param>
        /// <returns></returns>
        public static TimelineAsset CreateTimelineAsset(string name, string path)
        {
            if (!AssetDatabase.IsValidFolder("Assets/" + path))
            {
                AssetDatabase.CreateFolder("Assets", path);
            }
            var ta = ScriptableObjectUtility.CreateAssetType<TimelineAsset>("Assets/" + path, name);
            return ta;
        }

        /// <summary>
        /// assign a timeline asset to a given gameobject, will create a playable director on the gameobject if it doesn't already have one
        /// </summary>
        /// <param name="go"></param>
        /// <param name="ta"></param>
        public static void SetTimelinePlayableAsset( GameObject go, TimelineAsset ta)
        {
            var dir = go.GetComponent<PlayableDirector>();
            if( dir == null)
            {
                dir = go.AddComponent<PlayableDirector>();
            }
            dir.playableAsset = ta;
        }


        /// <summary>
        /// Prompt the user to save the new scene
        /// </summary>
        /// <param name="thisScene">the scene we want to save</param>
        public static void PromptToSaveScene(Scene thisScene)
        {
            var path = EditorUtility.SaveFilePanelInProject("Save Scene",  thisScene.name + ".unity", "unity", "Please save the new scene.");

            if (path.Length != 0)
            {
                EditorSceneManager.SaveScene(thisScene, path);
            }
        }

        /// <summary>
        /// Export the given asset at scenePath to a .unitypackage
        /// </summary>
        /// <param name="scenePath">Full path in the project to the asset that we want to export to a .unitypackage</param>
        /// <param name="sceneName">name of the asset we want to export (used for the filename)</param>
        public static void ExportPackage(string scenePath, string sceneName)
        {
            var savePath = EditorUtility.SaveFilePanel("Export Package", Application.dataPath, sceneName, "unitypackage");

            if (savePath.Length != 0)
            {
                var assetPath = "Assets/" + scenePath + "/" + sceneName + ".unity";
                AssetDatabase.ExportPackage(assetPath, savePath, ExportPackageOptions.IncludeDependencies);
            }
        }
    }
}