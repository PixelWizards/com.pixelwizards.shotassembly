using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PixelWizards.ShotAssembly
{
    /// <summary>
    /// Command line API
    /// </summary>
    public static class ShotAssemblyAPI
    {

        /// <summary>
        /// Test method, generate shot
        /// </summary>
        [MenuItem("Assets/GenerateShot")]
        public static void GenerateShot()
        {
            // create the scene with the first one
            GenerateShot("F:/projects/Adam.ShotAssembly/Assets/ADAM-AD1-Desert/ADAM-AD1-0010.json", false, "Timelines", false, "ADAM-Ep2-Desert", "Scenes");
            GenerateShot("F:/projects/Adam.ShotAssembly/Assets/ADAM-AD1-Desert/ADAM-AD1-0020.json", false, "Timelines");
            GenerateShot("F:/projects/Adam.ShotAssembly/Assets/ADAM-AD1-Desert/ADAM-AD1-0030.json", false, "Timelines");
            GenerateShot("F:/projects/Adam.ShotAssembly/Assets/ADAM-AD1-Desert/ADAM-AD1-0040.json", false, "Timelines");
            GenerateShot("F:/projects/Adam.ShotAssembly/Assets/ADAM-AD1-Desert/ADAM-AD1-0050.json", false, "Timelines");
            GenerateShot("F:/projects/Adam.ShotAssembly/Assets/ADAM-AD1-Desert/ADAM-AD1-0060.json", false, "Timelines");
            GenerateShot("F:/projects/Adam.ShotAssembly/Assets/ADAM-AD1-Desert/ADAM-AD1-0070.json", false, "Timelines");
        }

        /// <summary>
        /// Generate a shot, optionally create a new scene and timeline. Note: if you create a new scene, you MUST create a new timeline as well
        /// </summary>
        /// <param name="fileName">absolute path to the shot assembly config</param>
        /// <param name="useExistingTimeline">should we use an existing timeline or not?</param>
        /// <param name="timelineName">name of the GameObject that has our PlayableDirector that we want to use (will create a new one if useExistingTimeline is false</param>
        /// <param name="timelinePath">if we are creating a new timeline, this is the relative path in the project where we want to save the asset (relative to the project's Assets/ folder)</param>
        /// <param name="useExistingScene">are we using an existing scene? if not the shot will be generated in the currently open scene</param>
        /// <param name="sceneName">if we are NOT using an existing scene, what should the scene be called?</param>
        /// <param name="scenePath">if we are NOT using an existing scene, where should the scene be saved?</param>
        public static void GenerateShot(string fileName, bool useExistingTimeline, string timelinePath, bool useExistingScene, string sceneName, string scenePath)
        {
            if (useExistingScene)
            {
                UnityUtilities.CreateNewScene(sceneName, scenePath);
            }

            GenerateShot(fileName, useExistingScene,timelinePath);
        }

        /// <summary>
        /// Import a shot assembly config file and generate the resulting shot
        /// </summary>
        /// <param name="fileName">absolute path to the shot assembly config</param>
        /// <param name="useExistingTimeline">should we use an existing timeline or not?</param>
        /// <param name="timelineName">name of the GameObject that has our PlayableDirector that we want to use (will create a new one if useExistingTimeline is false</param>
        /// <param name="timelinePath">if we are creating a new timeline, this is the relative path in the project where we want to save the asset (relative to the project's Assets/ folder)</param>
        public static void GenerateShot(string fileName, bool useExistingTimeline, string timelinePath )
        {
            PlayableDirector pd;

            // make sure we can find the file
            if (File.Exists(fileName))
            {
                // parse the shot config
                var shotInfo = GetShotInfo(fileName);
                // create our timeline / playable director
                pd = GetOrAddPlayableDirector(shotInfo.shot_name, timelinePath, useExistingTimeline);
                // instance the actors and create their tracks
                GenerateTimelineTracks(shotInfo, pd);
            }
        }

        /// <summary>
        /// Parses a shot config 
        /// </summary>
        /// <param name="fileName">Absolute path to the shot config</param>
        /// <returns>the Shot config, if found</returns>
        public static Shot GetShotInfo(string fileName)
        {
            Shot newShot = new Shot();
            if (File.Exists(fileName))
            {
                var file = File.ReadAllText(fileName);
                newShot = JsonConvert.DeserializeObject<Shot>(file);
            }

            return newShot;
        }

        /// <summary>
        /// Either finds an existing timeline named 'timelineName' (if useExistingTimeline is true), or
        /// creates a new PlayableDirector and timeline asset. The timeline asset will be saved into 'timelinePath'
        /// </summary>
        /// <param name="timelineName">Name of the PlayableDirector and TimelineAsset</param>
        /// <param name="timelinePath">Path to save the timeline asset</param>
        /// <param name="useExistingTimeline">Whether we are looking for an existing timeline or creating a new one</param>
        /// <returns></returns>
        public static PlayableDirector GetOrAddPlayableDirector(string timelineName, string timelinePath, bool useExistingTimeline)
        {
            PlayableDirector pd = default;

            if (useExistingTimeline)
            {
                // see if we can find the timeline in the scene
                var go = GameObject.Find(timelineName);
                if (go != null)
                {
                    pd = go.GetComponent<PlayableDirector>();
                    if (pd == null)
                    {
                        Debug.Log("Could not find PlayableDirector for Timeline : " + timelineName);
                    }
                }
            }
            else
            {
                pd = UnityUtilities.CreatePlayableDirector(timelineName, timelinePath);
            }

            return pd;
        }

        /// <summary>
        /// Load the specified actor into the scene
        /// </summary>
        private static GameObject InstanceActorInScene(string prefabName)
        {
            var prefabGuid = AssetDatabase.FindAssets(prefabName);
            if (prefabGuid.Length == 0)
            {
                Debug.Log("Could not find unity_rig: " + prefabName + " for clip");
            }

            var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid[0]);
            var prefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
            var instance = Object.Instantiate(prefab);
            instance.name = prefabName; // get rid of the stupid (Clone) at the end of the name
            return instance;
        }
        
        /// <summary>
        /// For each clip in the shot, instance the actor in the scene, create a timeline track, and bind the animation to the track
        /// </summary>
        /// <param name="shotInfo">our shot config</param>
        /// <param name="pd">the playable director that we are using</param>
        public static void GenerateTimelineTracks(Shot shotInfo, PlayableDirector pd)
        {
            var timelineAsset = pd.playableAsset as TimelineAsset;
            var parentGo = new GameObject();
            parentGo.name = shotInfo.shot_name;

            foreach ( var clip in shotInfo.clips)
            {
                var prefabInstance = InstanceActorInScene(clip.unity_rig);
                prefabInstance.transform.parent = parentGo.transform;

                var animTrack = timelineAsset.CreateTrack<AnimationTrack>(null, prefabInstance.name);

                // bind the track to our new actor instance
                pd.SetGenericBinding(animTrack, prefabInstance);

                var anim_clip = FindAnimationClip(clip.name);
                UnityUtilities.AddClipToAnimationTrack(animTrack, anim_clip);
            }
        }

        /// <summary>
        /// the name of the animation clip that we are searching for, searching the entire asset database
        /// </summary>
        /// <param name="clip_name">name of the animation clip we are looking for</param>
        /// <returns>The first matching animation clip</returns>
        public static AnimationClip FindAnimationClip(string clip_name)
        {
            AnimationClip clip = default;

            var searchPath = "Assets";

            var guids = AssetDatabase.FindAssets(clip_name + " t:AnimationClip", new[] { searchPath });

            // should hopefully only find one that we care about
            var animPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            clip = (AnimationClip)AssetDatabase.LoadAssetAtPath(animPath, typeof(AnimationClip));
            return clip;
        }

    }
}