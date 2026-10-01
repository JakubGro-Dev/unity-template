using System;
using System.Collections.Generic;
using UnityEngine;

namespace template.Audio
{
    public sealed class AudioEmitterPool : IDisposable
    {
        readonly Stack<AudioEmitter> _available = new();
        readonly HashSet<AudioEmitter> _active = new();
        readonly List<AudioEmitter> _all = new();
        readonly int _prewarmCount;
        Transform _root;

        public event Action<AudioEmitter> Released;

        public AudioEmitterPool(int prewarmCount)
        {
            _prewarmCount = Mathf.Max(0, prewarmCount);
            EnsureRoot();
            Prewarm();
        }

        public AudioEmitter Get()
        {
            EnsureRoot();

            while (_available.Count > 0)
            {
                AudioEmitter emitter = _available.Pop();

                if (emitter == null)
                {
                    continue;
                }

                _active.Add(emitter);
                return emitter;
            }

            AudioEmitter created = CreateEmitter();
            _active.Add(created);
            return created;
        }

        internal void Release(AudioEmitter emitter)
        {
            if (emitter == null || !_active.Remove(emitter))
            {
                return;
            }

            Released?.Invoke(emitter);
            emitter.ResetForPool();
            _available.Push(emitter);
        }

        public void Dispose()
        {
            for (int i = 0; i < _all.Count; i++)
            {
                if (_all[i] != null)
                {
                    DestroyObject(_all[i].gameObject);
                }
            }

            _all.Clear();
            _available.Clear();
            _active.Clear();

            if (_root != null)
            {
                DestroyObject(_root.gameObject);
                _root = null;
            }
        }

        void Prewarm()
        {
            for (int i = 0; i < _prewarmCount; i++)
            {
                AudioEmitter emitter = CreateEmitter();
                emitter.ResetForPool();
                _available.Push(emitter);
            }
        }

        AudioEmitter CreateEmitter()
        {
            GameObject emitterObject = new("AudioEmitter");
            emitterObject.transform.SetParent(_root, false);

            AudioEmitter emitter = emitterObject.AddComponent<AudioEmitter>();
            emitter.Initialize(this);
            _all.Add(emitter);
            return emitter;
        }

        void EnsureRoot()
        {
            if (_root != null)
            {
                return;
            }

            GameObject rootObject = new("AudioEmitters");

            if (Application.isPlaying)
            {
                UnityEngine.Object.DontDestroyOnLoad(rootObject);
            }

            _root = rootObject.transform;
        }

        static void DestroyObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
                return;
            }

            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
