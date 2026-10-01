using System.Collections;
using UnityEngine;

namespace template.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioEmitter : MonoBehaviour
    {
        const float OccludedVolumeScale = 0.55f;
        const float OccludedCutoffFrequency = 1800f;
        const float OcclusionCheckInterval = 0.1f;

        AudioEmitterPool _pool;
        AudioSource _source;
        AudioLowPassFilter _lowPassFilter;
        AudioPlaybackRequest _request;
        Transform _followTarget;
        Transform _listener;
        Coroutine _routine;
        string _cueId;
        float _baseVolume;
        float _volumeMultiplier = 1f;
        float _fadeScale = 1f;
        float _occlusionScale = 1f;
        float _nextOcclusionCheckTime;
        bool _inUse;
        bool _paused;
        int _handleVersion;

        public string CueId => _cueId;
        public bool IsInUse => _inUse;
        public bool IsPlaying => _inUse && !_paused && _source != null && _source.isPlaying;
        public bool IsPaused => _paused;
        public int HandleVersion => _handleVersion;

        internal void Initialize(AudioEmitterPool pool)
        {
            _pool = pool;
            _source = GetComponent<AudioSource>();
            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }

            _source.playOnAwake = false;
        }

        internal AudioHandle Play(AudioPlaybackRequest request)
        {
            StopRoutine();

            _request = request;
            _followTarget = request.FollowTarget;
            _cueId = request.CueId;
            _baseVolume = request.Volume;
            _volumeMultiplier = 1f;
            _fadeScale = request.FadeInDuration > 0f ? 0f : 1f;
            _occlusionScale = 1f;
            _nextOcclusionCheckTime = 0f;
            _inUse = true;
            _paused = false;
            _handleVersion++;

            gameObject.SetActive(true);
            ConfigureSource(request);
            ApplyFollowTarget();
            ApplyVolume();
            DisableLowPass();

            _routine = StartCoroutine(PlaybackRoutine());
            return new AudioHandle(this, _handleVersion, _cueId);
        }

        internal void StopWithFade(int handleVersion)
        {
            if (handleVersion != _handleVersion)
            {
                return;
            }

            StopWithFade();
        }

        internal void StopWithFade()
        {
            if (!_inUse)
            {
                return;
            }

            _paused = false;
            _source.UnPause();

            float fadeOutDuration = _request != null ? _request.FadeOutDuration : 0f;

            if (fadeOutDuration <= 0f)
            {
                StopImmediate();
                return;
            }

            StopRoutine();
            _routine = StartCoroutine(FadeOutAndRelease(fadeOutDuration));
        }

        internal void StopImmediate()
        {
            if (!_inUse)
            {
                return;
            }

            _paused = false;
            _source.UnPause();
            StopRoutine();
            ReleaseToPool();
        }

        internal void Pause(int handleVersion)
        {
            if (!MatchesHandle(handleVersion))
            {
                return;
            }

            Pause();
        }

        internal void Resume(int handleVersion)
        {
            if (!MatchesHandle(handleVersion))
            {
                return;
            }

            Resume();
        }

        internal void SetVolumeMultiplier(float volume, int handleVersion)
        {
            if (!MatchesHandle(handleVersion))
            {
                return;
            }

            _volumeMultiplier = Mathf.Clamp01(volume);
            ApplyVolume();
        }

        internal void SetPitch(float pitch, int handleVersion)
        {
            if (!MatchesHandle(handleVersion))
            {
                return;
            }

            float clampedPitch = Mathf.Clamp(pitch, 0.01f, 3f);
            _source.pitch = clampedPitch;

            if (_request != null)
            {
                _request.Pitch = clampedPitch;
            }
        }

        internal void SetPosition(Vector3 position, int handleVersion)
        {
            if (!MatchesHandle(handleVersion) || _request == null)
            {
                return;
            }

            _followTarget = null;
            _request.FollowTarget = null;
            _request.Position = position;
            ApplyFollowTarget();
        }

        internal void SetFollowTarget(Transform followTarget, int handleVersion)
        {
            if (!MatchesHandle(handleVersion) || _request == null)
            {
                return;
            }

            _followTarget = followTarget;
            _request.FollowTarget = followTarget;
            _request.Position = followTarget != null ? followTarget.position : transform.position;
            ApplyFollowTarget();
        }

        internal void Pause()
        {
            if (!_inUse || _paused)
            {
                return;
            }

            _paused = true;

            if (_source.isPlaying)
            {
                _source.Pause();
            }
        }

        internal void Resume()
        {
            if (!_inUse || !_paused)
            {
                return;
            }

            _paused = false;
            _source.UnPause();
        }

        internal void ResetForPool()
        {
            StopRoutine();

            if (_source != null)
            {
                _source.Stop();
                _source.clip = null;
                _source.loop = false;
                _source.outputAudioMixerGroup = null;
                _source.ignoreListenerPause = false;
            }

            _request = null;
            _followTarget = null;
            _listener = null;
            _cueId = string.Empty;
            _baseVolume = 0f;
            _volumeMultiplier = 1f;
            _fadeScale = 1f;
            _occlusionScale = 1f;
            _nextOcclusionCheckTime = 0f;
            _inUse = false;
            _paused = false;
            DisableLowPass();
            gameObject.SetActive(false);
        }

        bool MatchesHandle(int handleVersion)
        {
            return _inUse && handleVersion == _handleVersion;
        }

        void ConfigureSource(AudioPlaybackRequest request)
        {
            _source.Stop();
            _source.clip = request.Clip;
            _source.loop = request.Loop;
            _source.outputAudioMixerGroup = request.Output;
            _source.ignoreListenerPause = request.IgnoreListenerPause;
            _source.pitch = request.Pitch;
            _source.priority = request.Priority;
            _source.spatialBlend = request.Spatial ? request.SpatialBlend : 0f;
            _source.minDistance = request.MinDistance;
            _source.maxDistance = request.MaxDistance;
            _source.rolloffMode = request.RolloffMode;
            _source.dopplerLevel = request.DopplerLevel;
            _source.spread = request.Spread;
        }

        IEnumerator PlaybackRoutine()
        {
            if (_request.Delay > 0f)
            {
                float delayElapsed = 0f;

                while (_inUse && delayElapsed < _request.Delay)
                {
                    if (_paused)
                    {
                        yield return WaitWhilePaused();
                        continue;
                    }

                    Tick();
                    delayElapsed += DeltaTime;
                    yield return null;
                }
            }

            if (!_inUse)
            {
                yield break;
            }

            _source.Play();

            if (_request.FadeInDuration > 0f)
            {
                yield return FadeTo(1f, _request.FadeInDuration);
            }

            if (!_inUse)
            {
                yield break;
            }

            if (_request.Loop)
            {
                while (_inUse)
                {
                    if (_paused)
                    {
                        yield return WaitWhilePaused();
                        continue;
                    }

                    Tick();
                    yield return null;
                }

                yield break;
            }

            if (_request.FadeOutDuration > 0f)
            {
                float fadeStartTime = Mathf.Max(0f, _request.Clip.length - (_request.FadeOutDuration * Mathf.Abs(_source.pitch)));

                while (_inUse && (_source.isPlaying || _paused) && _source.time < fadeStartTime)
                {
                    if (_paused)
                    {
                        yield return WaitWhilePaused();
                        continue;
                    }

                    Tick();
                    yield return null;
                }

                if (_inUse && (_source.isPlaying || _paused))
                {
                    yield return FadeTo(0f, _request.FadeOutDuration);
                }

                FinishAndRelease();
                yield break;
            }

            while (_inUse && (_source.isPlaying || _paused))
            {
                if (_paused)
                {
                    yield return WaitWhilePaused();
                    continue;
                }

                Tick();
                yield return null;
            }

            FinishAndRelease();
        }

        IEnumerator FadeOutAndRelease(float duration)
        {
            yield return FadeTo(0f, duration);
            FinishAndRelease();
        }

        IEnumerator FadeTo(float target, float duration)
        {
            float start = _fadeScale;
            float elapsed = 0f;

            if (duration <= 0f)
            {
                _fadeScale = target;
                ApplyVolume();
                yield break;
            }

            while (_inUse && elapsed < duration)
            {
                if (_paused)
                {
                    yield return WaitWhilePaused();
                    continue;
                }

                Tick();
                elapsed += DeltaTime;
                _fadeScale = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
                ApplyVolume();
                yield return null;
            }

            _fadeScale = target;
            ApplyVolume();
        }

        IEnumerator WaitWhilePaused()
        {
            while (_inUse && _paused)
            {
                Tick();
                yield return null;
            }
        }

        float DeltaTime => _request != null && _request.UseUnscaledTime
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        float CurrentTime => _request != null && _request.UseUnscaledTime
            ? Time.unscaledTime
            : Time.time;

        void Tick()
        {
            ApplyFollowTarget();
            UpdateOcclusion();
        }

        void ApplyFollowTarget()
        {
            if (_followTarget != null)
            {
                transform.position = _followTarget.position;
                return;
            }

            transform.position = _request.Position;
        }

        void UpdateOcclusion()
        {
            if (_request == null ||
                _request.OcclusionMode != AudioOcclusionMode.SimpleRaycastLowpass ||
                !_request.Spatial)
            {
                SetOccluded(false);
                return;
            }

            float currentTime = CurrentTime;

            if (currentTime < _nextOcclusionCheckTime)
            {
                return;
            }

            _nextOcclusionCheckTime = currentTime + OcclusionCheckInterval;

            Transform listener = ResolveListener();
            if (listener == null)
            {
                SetOccluded(false);
                return;
            }

            Vector3 origin = listener.position;
            Vector3 target = transform.position;
            Vector3 delta = target - origin;
            float distance = delta.magnitude;

            if (distance <= 0.01f)
            {
                SetOccluded(false);
                return;
            }

            bool blocked = Physics.Raycast(
                origin,
                delta / distance,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            SetOccluded(blocked);
        }

        Transform ResolveListener()
        {
            if (_listener != null)
            {
                return _listener;
            }

            AudioListener audioListener = FindAnyObjectByType<AudioListener>(FindObjectsInactive.Exclude);
            if (audioListener != null)
            {
                _listener = audioListener.transform;
                return _listener;
            }

            if (Camera.main != null)
            {
                _listener = Camera.main.transform;
            }

            return _listener;
        }

        void SetOccluded(bool occluded)
        {
            float nextScale = occluded ? OccludedVolumeScale : 1f;

            if (!Mathf.Approximately(_occlusionScale, nextScale))
            {
                _occlusionScale = nextScale;
                ApplyVolume();
            }

            if (occluded)
            {
                AudioLowPassFilter filter = GetLowPassFilter();
                filter.cutoffFrequency = OccludedCutoffFrequency;
                filter.enabled = true;
                return;
            }

            DisableLowPass();
        }

        AudioLowPassFilter GetLowPassFilter()
        {
            if (_lowPassFilter == null)
            {
                _lowPassFilter = gameObject.GetComponent<AudioLowPassFilter>();
            }

            if (_lowPassFilter == null)
            {
                _lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
            }

            return _lowPassFilter;
        }

        void DisableLowPass()
        {
            if (_lowPassFilter != null)
            {
                _lowPassFilter.enabled = false;
            }
        }

        void ApplyVolume()
        {
            _source.volume = Mathf.Clamp01(_baseVolume * _volumeMultiplier * _fadeScale * _occlusionScale);
        }

        void FinishAndRelease()
        {
            _routine = null;
            ReleaseToPool();
        }

        void ReleaseToPool()
        {
            if (!_inUse)
            {
                return;
            }

            _source.Stop();
            _paused = false;
            _inUse = false;
            _pool?.Release(this);
        }

        void StopRoutine()
        {
            if (_routine == null)
            {
                return;
            }

            StopCoroutine(_routine);
            _routine = null;
        }
    }
}
