using System;
using System.Collections.Generic;
using UnityEngine;

namespace template.Audio
{
    public sealed class AudioService : IAudioService, IDisposable
    {
        const int DefaultPrewarmCount = 24;

        readonly AudioCueDatabaseSO _database;
        readonly AudioEmitterPool _pool;
        readonly Dictionary<string, float> _cooldownUntil = new(StringComparer.Ordinal);
        readonly Dictionary<string, int> _activeCounts = new(StringComparer.Ordinal);
        readonly Dictionary<string, List<AudioEmitter>> _activeByCue = new(StringComparer.Ordinal);
        readonly Dictionary<string, int> _nextClipIndices = new(StringComparer.Ordinal);
        readonly Dictionary<string, AudioClip> _lastClips = new(StringComparer.Ordinal);
        bool _disposed;

        public AudioService(AudioCueDatabaseSO database)
            : this(database, DefaultPrewarmCount)
        {
        }

        public AudioService(AudioCueDatabaseSO database, int prewarmCount)
        {
            _database = database;
            _pool = new AudioEmitterPool(prewarmCount);
            _pool.Released += HandleEmitterReleased;
        }

        public int ActiveCount
        {
            get
            {
                int total = 0;

                foreach (int count in _activeCounts.Values)
                {
                    total += count;
                }

                return total;
            }
        }

        public int GetActiveCount(string id)
        {
            id = id?.Trim();
            if (string.IsNullOrEmpty(id))
            {
                return 0;
            }

            return _activeCounts.TryGetValue(id, out int count) ? count : 0;
        }

        public bool Preload(string id)
        {
            id = id?.Trim();
            if (_disposed || _database == null || string.IsNullOrEmpty(id))
            {
                return false;
            }

            return _database.TryGet(id, out AudioCueSO cue) && PreloadCue(cue);
        }

        public void PreloadAll()
        {
            if (_disposed || _database == null)
            {
                return;
            }

            IReadOnlyList<AudioCueSO> cues = _database.Cues;

            for (int i = 0; i < cues.Count; i++)
            {
                PreloadCue(cues[i]);
            }
        }

        public AudioHandle Play(string id)
        {
            return TryPlay(id, null, Vector3.zero, false, false, out AudioHandle handle)
                ? handle
                : default;
        }

        public AudioHandle Play(string id, Vector3 position)
        {
            return TryPlay(id, null, position, false, true, out AudioHandle handle)
                ? handle
                : default;
        }

        public void Play2D(string id)
        {
            _ = Play(id);
        }

        public void Play3D(string id, Vector3 position)
        {
            _ = Play(id, position);
        }

        public AudioHandle PlayLoop2D(string id)
        {
            return TryPlay(id, null, Vector3.zero, true, false, out AudioHandle handle)
                ? handle
                : default;
        }

        public AudioHandle PlayLoop3D(string id, Transform followTarget)
        {
            Vector3 position = followTarget != null ? followTarget.position : Vector3.zero;
            return TryPlay(id, followTarget, position, true, true, out AudioHandle handle)
                ? handle
                : default;
        }

        public AudioHandle PlayLoop(string id, Transform followTarget)
        {
            return PlayLoop3D(id, followTarget);
        }

        public void Stop(AudioHandle handle)
        {
            if (!handle.IsValid)
            {
                return;
            }

            handle.Emitter.StopWithFade(handle.Version);
        }

        public bool IsPlaying(AudioHandle handle)
        {
            return handle.IsValid && handle.Emitter.IsPlaying;
        }

        public bool IsPaused(AudioHandle handle)
        {
            return handle.IsPaused;
        }

        public void StopAll()
        {
            AudioEmitter[] snapshot = GetActiveSnapshot();

            for (int i = 0; i < snapshot.Length; i++)
            {
                AudioEmitter emitter = snapshot[i];

                if (emitter != null && emitter.IsInUse)
                {
                    emitter.StopWithFade();
                }
            }
        }

        public void StopAll(string id)
        {
            id = id?.Trim();
            if (string.IsNullOrEmpty(id) || !_activeByCue.TryGetValue(id, out List<AudioEmitter> emitters))
            {
                return;
            }

            AudioEmitter[] snapshot = emitters.ToArray();

            for (int i = 0; i < snapshot.Length; i++)
            {
                AudioEmitter emitter = snapshot[i];

                if (emitter != null && emitter.IsInUse && emitter.CueId == id)
                {
                    emitter.StopWithFade();
                }
            }
        }

        public void Pause(AudioHandle handle)
        {
            if (handle.IsValid)
            {
                handle.Emitter.Pause(handle.Version);
            }
        }

        public void Resume(AudioHandle handle)
        {
            if (handle.IsValid)
            {
                handle.Emitter.Resume(handle.Version);
            }
        }

        public void PauseAll()
        {
            AudioEmitter[] snapshot = GetActiveSnapshot();

            for (int i = 0; i < snapshot.Length; i++)
            {
                AudioEmitter emitter = snapshot[i];

                if (emitter != null && emitter.IsInUse)
                {
                    emitter.Pause();
                }
            }
        }

        public void PauseAll(string id)
        {
            id = id?.Trim();
            if (string.IsNullOrEmpty(id) || !_activeByCue.TryGetValue(id, out List<AudioEmitter> emitters))
            {
                return;
            }

            AudioEmitter[] snapshot = emitters.ToArray();

            for (int i = 0; i < snapshot.Length; i++)
            {
                AudioEmitter emitter = snapshot[i];

                if (emitter != null && emitter.IsInUse && emitter.CueId == id)
                {
                    emitter.Pause();
                }
            }
        }

        public void ResumeAll()
        {
            AudioEmitter[] snapshot = GetActiveSnapshot();

            for (int i = 0; i < snapshot.Length; i++)
            {
                AudioEmitter emitter = snapshot[i];

                if (emitter != null && emitter.IsInUse)
                {
                    emitter.Resume();
                }
            }
        }

        public void ResumeAll(string id)
        {
            id = id?.Trim();
            if (string.IsNullOrEmpty(id) || !_activeByCue.TryGetValue(id, out List<AudioEmitter> emitters))
            {
                return;
            }

            AudioEmitter[] snapshot = emitters.ToArray();

            for (int i = 0; i < snapshot.Length; i++)
            {
                AudioEmitter emitter = snapshot[i];

                if (emitter != null && emitter.IsInUse && emitter.CueId == id)
                {
                    emitter.Resume();
                }
            }
        }

        public void SetVolume(AudioHandle handle, float volume)
        {
            if (handle.IsValid)
            {
                handle.Emitter.SetVolumeMultiplier(volume, handle.Version);
            }
        }

        public void SetPitch(AudioHandle handle, float pitch)
        {
            if (handle.IsValid)
            {
                handle.Emitter.SetPitch(pitch, handle.Version);
            }
        }

        public void SetPosition(AudioHandle handle, Vector3 position)
        {
            if (handle.IsValid)
            {
                handle.Emitter.SetPosition(position, handle.Version);
            }
        }

        public void SetFollowTarget(AudioHandle handle, Transform followTarget)
        {
            if (handle.IsValid)
            {
                handle.Emitter.SetFollowTarget(followTarget, handle.Version);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _pool.Released -= HandleEmitterReleased;
            _pool.Dispose();
            _cooldownUntil.Clear();
            _activeCounts.Clear();
            _activeByCue.Clear();
            _nextClipIndices.Clear();
            _lastClips.Clear();
        }

        bool TryPlay(
            string id,
            Transform followTarget,
            Vector3 position,
            bool forceLoop,
            bool spatialPlayback,
            out AudioHandle handle)
        {
            handle = default;

            id = id?.Trim();
            if (_disposed || string.IsNullOrEmpty(id))
            {
                return false;
            }

            if (_database == null)
            {
                LogMissingDatabase(id);
                return false;
            }

            if (!_database.TryGet(id, out AudioCueSO cue))
            {
                LogMissingCue(id);
                return false;
            }

            if (IsCoolingDown(cue))
            {
                return false;
            }

            int activeCount = _activeCounts.TryGetValue(id, out int count) ? count : 0;
            bool restart = !cue.Duplicate && activeCount > 0 &&
                           cue.DuplicateBehavior == AudioDuplicateBehavior.Restart;

            if (!cue.Duplicate && activeCount > 0 && !restart)
            {
                return false;
            }

            if (cue.Duplicate && cue.MaxSimultaneous > 0 && activeCount >= cue.MaxSimultaneous)
            {
                return false;
            }

            if (!TrySelectClip(cue, out AudioClip clip))
            {
                LogMissingClip(id);
                return false;
            }

            AudioPlaybackRequest request = BuildRequest(cue, clip, followTarget, position, forceLoop, spatialPlayback);

            if (restart)
            {
                StopAllImmediate(id);
            }

            AudioEmitter emitter = _pool.Get();

            RegisterActive(id, emitter);
            ApplyCooldown(cue);
            handle = emitter.Play(request);
            return true;
        }

        AudioPlaybackRequest BuildRequest(
            AudioCueSO cue,
            AudioClip clip,
            Transform followTarget,
            Vector3 position,
            bool forceLoop,
            bool spatialPlayback)
        {
            bool spatial = spatialPlayback && cue.Spatial;

            return new AudioPlaybackRequest
            {
                CueId = cue.Id,
                Clip = clip,
                Output = cue.Output,
                Position = position,
                FollowTarget = followTarget,
                Loop = forceLoop || cue.Loop,
                IgnoreListenerPause = cue.IgnoreListenerPause,
                UseUnscaledTime = cue.UseUnscaledTime,
                Volume = Mathf.Clamp01(UnityEngine.Random.Range(cue.VolumeMin, cue.VolumeMax)),
                Pitch = UnityEngine.Random.Range(cue.PitchMin, cue.PitchMax),
                Delay = UnityEngine.Random.Range(cue.DelayMin, cue.DelayMax),
                FadeInDuration = cue.FadeInDuration,
                FadeOutDuration = cue.FadeOutDuration,
                Spatial = spatial,
                SpatialBlend = spatial ? cue.SpatialBlend : 0f,
                MinDistance = cue.MinDistance,
                MaxDistance = cue.MaxDistance,
                RolloffMode = cue.RolloffMode,
                DopplerLevel = cue.DopplerLevel,
                Spread = cue.Spread,
                OcclusionMode = cue.OcclusionMode,
                Priority = cue.Priority
            };
        }

        static bool PreloadCue(AudioCueSO cue)
        {
            if (cue == null)
            {
                return false;
            }

            AudioClip[] clips = cue.Clips;
            if (clips == null || clips.Length == 0)
            {
                return false;
            }

            bool hasClip = false;
            bool allAvailable = true;

            for (int i = 0; i < clips.Length; i++)
            {
                AudioClip clip = clips[i];

                if (clip == null)
                {
                    continue;
                }

                hasClip = true;

                if (clip.loadState == AudioDataLoadState.Loaded || clip.loadState == AudioDataLoadState.Loading)
                {
                    continue;
                }

                allAvailable &= clip.LoadAudioData();
            }

            return hasClip && allAvailable;
        }

        bool TrySelectClip(AudioCueSO cue, out AudioClip clip)
        {
            clip = null;
            AudioClip[] clips = cue.Clips;
            int validCount = CountValidClips(clips);

            if (validCount == 0)
            {
                return false;
            }

            switch (cue.ClipSelectionMode)
            {
                case AudioClipSelectionMode.Sequential:
                    return TrySelectSequentialClip(cue, clips, out clip);
                case AudioClipSelectionMode.RandomNoImmediateRepeat:
                    return TrySelectRandomNoImmediateRepeatClip(cue, clips, validCount, out clip);
                default:
                    clip = GetValidClipAt(clips, UnityEngine.Random.Range(0, validCount));
                    return clip != null;
            }
        }

        bool TrySelectSequentialClip(AudioCueSO cue, AudioClip[] clips, out AudioClip clip)
        {
            clip = null;

            if (clips == null || clips.Length == 0)
            {
                return false;
            }

            int startIndex = _nextClipIndices.TryGetValue(cue.Id, out int nextIndex)
                ? Mathf.Clamp(nextIndex, 0, clips.Length - 1)
                : 0;

            for (int i = 0; i < clips.Length; i++)
            {
                int index = (startIndex + i) % clips.Length;

                if (clips[index] == null)
                {
                    continue;
                }

                clip = clips[index];
                _nextClipIndices[cue.Id] = (index + 1) % clips.Length;
                _lastClips[cue.Id] = clip;
                return true;
            }

            return false;
        }

        bool TrySelectRandomNoImmediateRepeatClip(AudioCueSO cue, AudioClip[] clips, int validCount, out AudioClip clip)
        {
            clip = null;

            if (validCount == 1)
            {
                clip = GetValidClipAt(clips, 0);
                _lastClips[cue.Id] = clip;
                return clip != null;
            }

            _lastClips.TryGetValue(cue.Id, out AudioClip previous);

            for (int i = 0; i < 8; i++)
            {
                AudioClip candidate = GetValidClipAt(clips, UnityEngine.Random.Range(0, validCount));

                if (candidate != null && candidate != previous)
                {
                    clip = candidate;
                    _lastClips[cue.Id] = clip;
                    return true;
                }
            }

            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null && clips[i] != previous)
                {
                    clip = clips[i];
                    _lastClips[cue.Id] = clip;
                    return true;
                }
            }

            return false;
        }

        static int CountValidClips(AudioClip[] clips)
        {
            if (clips == null)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                {
                    count++;
                }
            }

            return count;
        }

        static AudioClip GetValidClipAt(AudioClip[] clips, int validIndex)
        {
            if (clips == null)
            {
                return null;
            }

            int current = 0;

            for (int i = 0; i < clips.Length; i++)
            {
                AudioClip clip = clips[i];

                if (clip == null)
                {
                    continue;
                }

                if (current == validIndex)
                {
                    return clip;
                }

                current++;
            }

            return null;
        }

        bool IsCoolingDown(AudioCueSO cue)
        {
            return cue.Cooldown > 0f &&
                   _cooldownUntil.TryGetValue(cue.Id, out float cooldownEndTime) &&
                   GetCueTime(cue) < cooldownEndTime;
        }

        static float GetCueTime(AudioCueSO cue)
        {
            return cue.UseUnscaledTime ? Time.unscaledTime : Time.time;
        }

        void ApplyCooldown(AudioCueSO cue)
        {
            if (cue.Cooldown <= 0f)
            {
                return;
            }

            _cooldownUntil[cue.Id] = GetCueTime(cue) + cue.Cooldown;
        }

        void StopAllImmediate(string id)
        {
            if (!_activeByCue.TryGetValue(id, out List<AudioEmitter> emitters))
            {
                return;
            }

            AudioEmitter[] snapshot = emitters.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                AudioEmitter emitter = snapshot[i];

                if (emitter != null && emitter.IsInUse && emitter.CueId == id)
                {
                    emitter.StopImmediate();
                }
            }
        }

        AudioEmitter[] GetActiveSnapshot()
        {
            List<AudioEmitter> snapshot = new(ActiveCount);

            foreach (List<AudioEmitter> emitters in _activeByCue.Values)
            {
                for (int i = 0; i < emitters.Count; i++)
                {
                    AudioEmitter emitter = emitters[i];

                    if (emitter != null && emitter.IsInUse)
                    {
                        snapshot.Add(emitter);
                    }
                }
            }

            return snapshot.ToArray();
        }

        void RegisterActive(string id, AudioEmitter emitter)
        {
            _activeCounts[id] = _activeCounts.TryGetValue(id, out int count) ? count + 1 : 1;

            if (!_activeByCue.TryGetValue(id, out List<AudioEmitter> emitters))
            {
                emitters = new List<AudioEmitter>();
                _activeByCue[id] = emitters;
            }

            emitters.Add(emitter);
        }

        void HandleEmitterReleased(AudioEmitter emitter)
        {
            string id = emitter.CueId;

            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            if (_activeCounts.TryGetValue(id, out int count))
            {
                count--;

                if (count <= 0)
                {
                    _activeCounts.Remove(id);
                }
                else
                {
                    _activeCounts[id] = count;
                }
            }

            if (_activeByCue.TryGetValue(id, out List<AudioEmitter> emitters))
            {
                emitters.Remove(emitter);

                if (emitters.Count == 0)
                {
                    _activeByCue.Remove(id);
                }
            }
        }

        static void LogMissingDatabase(string id)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"AudioService: cannot play '{id}' because no AudioCueDatabaseSO is registered.");
#endif
        }

        static void LogMissingCue(string id)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"AudioService: cue '{id}' is not registered.");
#endif
        }

        static void LogMissingClip(string id)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"AudioService: cue '{id}' has no playable clips.");
#endif
        }
    }
}
