using System;
using System.Collections.Generic;
using UnityEngine;

namespace Animi.Core {
    [System.Serializable]
    public class AnimiNodeBaseBehaviour : ScriptableObject {
        [HideInInspector]
        public long hashId;

        [NonSerialized]
        internal GameObject gameObject;

        public virtual void OnEntry() { }
        public virtual void OnUpdate() { }
        public virtual void OnLeave() { }
    }
}
