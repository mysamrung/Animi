using UnityEngine;

namespace Animi.Core {
    [RequireComponent(typeof(AnimiMotionController))]
    public class AnimiSystem : MonoBehaviour {
        public AnimiMotionController animiMotionController;
        public AnimiData animiData;

        public void Start()
        {
            foreach (var node in animiData.nodeDataObjects)
            {
                node.gameObject = this.gameObject;
                node.OnInitialize();
            }

            animiData.nodeDataObjects[0].OnEntry();
        }
    }
}
