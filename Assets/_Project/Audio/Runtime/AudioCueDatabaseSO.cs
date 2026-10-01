using System;
using System.Collections.Generic;
using UnityEngine;

namespace template.Audio
{
    [CreateAssetMenu(fileName = "AudioCueDatabase", menuName = "template/Audio/Cue Database")]
    public sealed class AudioCueDatabaseSO : ScriptableObject
    {
        [SerializeField] List<AudioCueSO> cues = new();

        readonly Dictionary<string, AudioCueSO> _cuesById = new(StringComparer.Ordinal);
        bool _isBuilt;

        public IReadOnlyList<AudioCueSO> Cues => cues;

        void OnEnable()
        {
            Rebuild();
        }

        void OnValidate()
        {
            Rebuild();
        }

        public bool TryGet(string id, out AudioCueSO cue)
        {
            id = id?.Trim();
            if (!_isBuilt)
            {
                Rebuild();
            }

            if (string.IsNullOrEmpty(id))
            {
                cue = null;
                return false;
            }

            return _cuesById.TryGetValue(id, out cue);
        }

        public AudioCueSO GetRequired(string id)
        {
            if (TryGet(id, out AudioCueSO cue))
            {
                return cue;
            }

            throw new InvalidOperationException($"Audio cue '{id}' is not registered.");
        }

        void Rebuild()
        {
            cues ??= new List<AudioCueSO>();
            _cuesById.Clear();
            _isBuilt = true;

            for (int i = 0; i < cues.Count; i++)
            {
                AudioCueSO cue = cues[i];

                if (cue == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(cue.Id))
                {
                    LogEmptyId(cue);
                    continue;
                }

                if (_cuesById.TryGetValue(cue.Id, out AudioCueSO existing))
                {
                    LogDuplicate(cue.Id, existing, cue);
                    continue;
                }

                _cuesById.Add(cue.Id, cue);
            }
        }

        static void LogDuplicate(string id, AudioCueSO existing, AudioCueSO duplicate)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning(
                $"AudioCueDatabaseSO: duplicate cue id '{id}' ignored. Existing: {existing.name}, duplicate: {duplicate.name}.",
                duplicate);
#endif
        }

        static void LogEmptyId(AudioCueSO cue)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"AudioCueDatabaseSO: cue '{cue.name}' has an empty id and was ignored.", cue);
#endif
        }
    }
}
