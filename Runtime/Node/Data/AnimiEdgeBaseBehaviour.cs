using System.Collections.Generic;
using UnityEngine;

namespace Animi.Core
{
    [System.Serializable]
    public class AnimiEdgeBaseBehaviour : ScriptableObject
    {
        [AnimiHideInspector]
        public AnimiNodeBaseBehaviour from;

        [AnimiHideInspector]
        public AnimiNodeBaseBehaviour to;

        public virtual void OnEntry() { }
        public virtual void OnUpdate() { }
        public virtual void OnLeave() { }
    }
}