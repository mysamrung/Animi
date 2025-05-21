using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animi.Core
{
    [System.Serializable]
    public class AnimiEdgeMotionBehaviour : ScriptableObject
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

        [AnimiHideInspector]
        public AnimiMotionBehaviour from;

        [AnimiHideInspector]
        public AnimiMotionBehaviour to;

        public virtual void OnEntry() { }
        public virtual void OnUpdate() { }
        public virtual void OnLeave() { }
    }
}
