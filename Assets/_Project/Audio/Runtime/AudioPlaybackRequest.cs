using UnityEngine;
using UnityEngine.Audio;

namespace template.Audio
{
    internal sealed class AudioPlaybackRequest
    {
        public string CueId;
        public AudioClip Clip;
        public AudioMixerGroup Output;
        public Vector3 Position;
        public Transform FollowTarget;
        public bool Loop;
        public bool IgnoreListenerPause;
        public bool UseUnscaledTime;
        public float Volume;
        public float Pitch;
        public float Delay;
        public float FadeInDuration;
        public float FadeOutDuration;
        public bool Spatial;
        public float SpatialBlend;
        public float MinDistance;
        public float MaxDistance;
        public AudioRolloffMode RolloffMode;
        public float DopplerLevel;
        public float Spread;
        public AudioOcclusionMode OcclusionMode;
        public int Priority;
    }
}
