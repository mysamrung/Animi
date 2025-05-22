using System.Collections.Generic;
using UnityEngine;

namespace Animi.Core {

    [System.Serializable]
    public class AnimiConnectedLineData {
        public long fromHashId;
        public List<long> toHashId = new List<long>();
    }

    public class AnimiData : ScriptableObject{
        public List<AnimiNodeBaseBehaviour> nodeDataObjects = new List<AnimiNodeBaseBehaviour>();
        public List<AnimiEdgeBaseBehaviour> edgeDataObjects = new List<AnimiEdgeBaseBehaviour>();
    }
}

