using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

namespace template.Audio
{
    [DisallowMultipleComponent]
    public sealed class AudioCuePlayer : MonoBehaviour
    {
        [SerializeField] string cueId = string.Empty;
        [SerializeField] Transform positionSource = null;
        [SerializeField] bool playOnEnable = false;
        [SerializeField] bool playOnStart = false;
        [SerializeField] bool stopLoopOnDisable = true;

        [Inject] AudioCueDatabaseSO _audioCueDatabase = null;
        [Inject] IAudioService _audioService = null;

        readonly List<AudioHandle> _loopHandles = new();

        void OnValidate()
        {
            cueId = cueId?.Trim() ?? string.Empty;
        }

        void OnEnable()
        {
            if (playOnEnable)
            {
                Play();
            }
        }

        void Start()
        {
            if (playOnStart)
            {
                Play();
            }
        }

        void OnDisable()
        {
            if (stopLoopOnDisable)
            {
                Stop();
            }
        }

        void OnDestroy()
        {
            Stop();
        }

        public void Play()
        {
            Play(cueId);
        }

        public void Play(string id)
        {
            id = id?.Trim();
            if (_audioService == null || string.IsNullOrEmpty(id))
            {
                return;
            }

            RemoveInvalidLoopHandles();
            bool loopCue = _audioCueDatabase != null &&
                           _audioCueDatabase.TryGet(id, out AudioCueSO cue) && cue.Loop;
            Transform source = positionSource != null ? positionSource : transform;
            AudioHandle handle = loopCue
                ? _audioService.PlayLoop(id, source)
                : _audioService.Play(id, source.position);

            if (loopCue && handle.IsValid)
            {
                _loopHandles.Add(handle);
            }
        }

        public void Stop()
        {
            for (int i = 0; i < _loopHandles.Count; i++)
            {
                _audioService?.Stop(_loopHandles[i]);
            }

            _loopHandles.Clear();
        }

        public void Stop(string id)
        {
            id = id?.Trim();
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            for (int i = _loopHandles.Count - 1; i >= 0; i--)
            {
                AudioHandle handle = _loopHandles[i];
                if (!handle.IsValid || handle.CueId == id)
                {
                    if (handle.IsValid)
                    {
                        _audioService?.Stop(handle);
                    }

                    _loopHandles.RemoveAt(i);
                }
            }
        }

        void RemoveInvalidLoopHandles()
        {
            for (int i = _loopHandles.Count - 1; i >= 0; i--)
            {
                if (!_loopHandles[i].IsValid)
                {
                    _loopHandles.RemoveAt(i);
                }
            }
        }
    }
}
