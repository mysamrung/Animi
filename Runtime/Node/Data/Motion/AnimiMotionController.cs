using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animi.Core
{
    [RequireComponent(typeof(Animator))]
    public class AnimiMotionController : MonoBehaviour
    {
        private PlayableGraph playableGraph;

        private ScriptPlayable<AnimiCrossfadeQueuePlayable> animiCrossfadeQueuePlayable;

        private void Awake()
        {
            playableGraph = PlayableGraph.Create();

            animiCrossfadeQueuePlayable = ScriptPlayable<AnimiCrossfadeQueuePlayable>.Create(playableGraph);

            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());
            playableOutput.SetSourcePlayable(animiCrossfadeQueuePlayable);
        }

        public void CrossFade(AnimiEdgeMotionBehaviour transition)
        {
            var animiCrossfadeQueue = animiCrossfadeQueuePlayable.GetBehaviour();
            animiCrossfadeQueue.CrossFade(transition, animiCrossfadeQueuePlayable, playableGraph);

            playableGraph.Play();
        }
    }
}