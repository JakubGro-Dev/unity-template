using System;
using UnityEngine;
using UnityEngine.Audio;

namespace template.Audio
{
    [CreateAssetMenu(fileName = "AudioCue", menuName = "template/Audio/Cue")]
    public sealed class AudioCueSO : ScriptableObject
    {
        const float UnlimitedMaxDistance = 100000f;

        [SerializeField] string id = string.Empty;
        [SerializeField] AudioClip[] clips = Array.Empty<AudioClip>();
        [SerializeField] AudioClipSelectionMode clipSelectionMode = AudioClipSelectionMode.Random;
        [SerializeField] AudioMixerGroup output = null;
        [SerializeField] bool loop = false;
        [SerializeField] bool ignoreListenerPause = false;
        [SerializeField] bool useUnscaledTime = false;
        [SerializeField] float volume = 1f;
        [SerializeField] bool randomizeVolume = false;
        [SerializeField, HideInInspector] float volumeMin = 1f;
        [SerializeField, HideInInspector] float volumeMax = 1f;
        [SerializeField] float pitch = 1f;
        [SerializeField] bool randomizePitch = false;
        [SerializeField, HideInInspector] float pitchMin = 1f;
        [SerializeField, HideInInspector] float pitchMax = 1f;
        [SerializeField] float delay;
        [SerializeField] bool randomizeDelay = false;
        [SerializeField, HideInInspector] float delayMin;
        [SerializeField, HideInInspector] float delayMax;
        [SerializeField] bool useFadeIn = false;
        [SerializeField, HideInInspector] float fadeInDuration;
        [SerializeField] bool useFadeOut = false;
        [SerializeField, HideInInspector] float fadeOutDuration;
        [SerializeField] AudioSpatialBlendMode spatialBlend = AudioSpatialBlendMode.ThreeD;
        [SerializeField] float minDistance = 1f;
        [SerializeField] bool useMaxDistance = true;
        [SerializeField, HideInInspector] float maxDistance = 30f;
        [SerializeField] AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        [SerializeField] float dopplerLevel = 1f;
        [SerializeField] float spread;
        [SerializeField] float cooldown;
        [SerializeField] int maxSimultaneous;
        [SerializeField] bool duplicate = true;
        [SerializeField] AudioDuplicateBehavior duplicateBehavior = AudioDuplicateBehavior.Ignore;
        [SerializeField] AudioOcclusionMode occlusionMode = AudioOcclusionMode.None;
        [SerializeField] AudioNetworkMode networkMode = AudioNetworkMode.LocalOnly;
        [SerializeField] int priority = 128;

        public string Id => id?.Trim() ?? string.Empty;
        public AudioClip[] Clips => clips;
        public AudioClipSelectionMode ClipSelectionMode => clipSelectionMode;
        public AudioMixerGroup Output => output;
        public bool Loop => loop;
        public bool IgnoreListenerPause => ignoreListenerPause;
        public bool UseUnscaledTime => useUnscaledTime;
        public float VolumeMin => randomizeVolume ? volumeMin : volume;
        public float VolumeMax => randomizeVolume ? volumeMax : volume;
        public float PitchMin => randomizePitch ? pitchMin : pitch;
        public float PitchMax => randomizePitch ? pitchMax : pitch;
        public float DelayMin => randomizeDelay ? delayMin : delay;
        public float DelayMax => randomizeDelay ? delayMax : delay;
        public float FadeInDuration => useFadeIn ? fadeInDuration : 0f;
        public float FadeOutDuration => useFadeOut ? fadeOutDuration : 0f;
        public bool Spatial => spatialBlend == AudioSpatialBlendMode.ThreeD;
        public float SpatialBlend => Spatial ? 1f : 0f;
        public float MinDistance => minDistance;
        public float MaxDistance => useMaxDistance ? maxDistance : UnlimitedMaxDistance;
        public AudioRolloffMode RolloffMode => rolloffMode;
        public float DopplerLevel => dopplerLevel;
        public float Spread => spread;
        public float Cooldown => cooldown;
        public int MaxSimultaneous => maxSimultaneous;
        public bool Duplicate => duplicate;
        public AudioDuplicateBehavior DuplicateBehavior => duplicateBehavior;
        public AudioOcclusionMode OcclusionMode => occlusionMode;
        public AudioNetworkMode NetworkMode => networkMode;
        public int Priority => priority;

        void OnValidate()
        {
            id = Id;
            clips ??= Array.Empty<AudioClip>();

            volume = Mathf.Clamp01(volume);
            volumeMin = Mathf.Clamp01(volumeMin);
            volumeMax = Mathf.Clamp01(volumeMax);
            if (volumeMax < volumeMin)
            {
                volumeMax = volumeMin;
            }

            pitch = Mathf.Clamp(pitch, 0.01f, 3f);
            pitchMin = Mathf.Clamp(pitchMin, 0.01f, 3f);
            pitchMax = Mathf.Clamp(pitchMax, 0.01f, 3f);
            if (pitchMax < pitchMin)
            {
                pitchMax = pitchMin;
            }

            delay = Mathf.Max(0f, delay);
            delayMin = Mathf.Max(0f, delayMin);
            delayMax = Mathf.Max(0f, delayMax);
            if (delayMax < delayMin)
            {
                delayMax = delayMin;
            }

            fadeInDuration = Mathf.Max(0f, fadeInDuration);
            fadeOutDuration = Mathf.Max(0f, fadeOutDuration);
            minDistance = Mathf.Max(0.01f, minDistance);
            maxDistance = Mathf.Max(minDistance, maxDistance);
            dopplerLevel = Mathf.Max(0f, dopplerLevel);
            spread = Mathf.Clamp(spread, 0f, 360f);
            cooldown = Mathf.Max(0f, cooldown);
            maxSimultaneous = Mathf.Max(0, maxSimultaneous);
            priority = Mathf.Clamp(priority, 0, 256);
        }
    }
}
