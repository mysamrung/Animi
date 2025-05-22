using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animi.Core
{
    [System.Serializable]
    public class AnimiEdgeMotionBehaviour : AnimiEdgeBaseBehaviour
    {
        public enum BlendType
        {
            Linear,
            Cubic,
        }

        public BlendType blendType;
        public float blendDuration;
        public float exitTime;
        public bool hasExitTime;

    }
}
