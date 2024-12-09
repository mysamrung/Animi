using System.Collections.Generic;
using UnityEngine;

namespace Animi.Core {
    [System.Serializable]
    public class AnimiMotionBehaviour : AnimiNodeBaseBehaviour {
        public string nodeName = "Motion Node";

        public AnimationClip animationClip;
        public AnimationCurve blendCurve;
        public float blendDuration;
        public float exitTime;
        public void OnEntry() { }
        public void OnUpdate() { }
        public void OnLeave() { }
    }
}