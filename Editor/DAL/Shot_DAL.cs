using System.Collections.Generic;

namespace PixelWizards.ShotAssembly
{
    /// <summary>
    /// an individual clip
    /// </summary>
    public class Clip
    {
        /// <summary>
        /// the name of the animation clip
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// the corresponding Maya asset
        /// </summary>
        public string asset { get; set; }
        /// <summary>
        /// which instance in the scene this is (may use the same actor with multiple instances)
        /// </summary>
        public int instance { get; set; }
        /// <summary>
        /// the rig version (for tracking purposes)
        /// </summary>
        public int maya_rig { get; set; }
        /// <summary>
        /// the name of the Unity actor
        /// </summary>
        public string unity_rig { get; set; }
    }

    /// <summary>
    /// the top-level container for our shot configs
    /// </summary>
    public class Shot
    {
        /// <summary>
        /// the name of the shot - this will be used to create the timeline / playable director
        /// </summary>
        public string shot_name { get; set; }
        /// <summary>
        /// start / end range of the shot
        /// </summary>
        public List<int> shot_range { get; set; }
        /// <summary>
        /// length of the shot in frames
        /// </summary>
        public int length { get; set; }
        /// <summary>
        /// the list of actors / clips that belong to this shot
        /// </summary>
        public List<Clip> clips { get; set; }
    }
}