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
        // generate this type of Timeline Track
        public static Scene CreateNewScene( string sceneName, string scenePath)
        {
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            newScene.name = sceneName;
            //EditorSceneManager.SaveScene(newScene, "Assets/" + scenePath + "/" + sceneName + ".unity");

            return newScene;
        }

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
        /// <param name="name"></param>
        /// <param name="path"></param>
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
        /// assign a timeline asset to a given gameobject (MUST have playable director)
        /// </summary>
        /// <param name="go"></param>
        /// <param name="ta"></param>
        public static void SetTimelinePlayableAsset( GameObject go, TimelineAsset ta)
        {
            var dir = go.GetComponent<PlayableDirector>();
            if( dir == null)
            {
                Debug.LogError("GameObject : " + go.name + " does not have a playable director component?");
                return;
            }
            dir.playableAsset = ta;
        }


        /// <summary>
        /// Prompt the user to save the new scene
        /// </summary>
        /// <param name="thisScene"></param>
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
            var path = EditorUtility.SaveFilePanel("Export Package", sceneName + ".unitypackage", "unitypackage", "Save the exported package.");

            if (path.Length != 0)
            {
                AssetDatabase.ExportPackage(path, scenePath + sceneName, ExportPackageOptions.IncludeDependencies);
            }
        }
    }
}