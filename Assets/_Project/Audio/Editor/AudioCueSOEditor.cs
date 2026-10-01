using template.Audio;
using UnityEditor;

namespace template.Editor.Audio
{
    [CustomEditor(typeof(AudioCueSO))]
    [CanEditMultipleObjects]
    public sealed class AudioCueSOEditor : UnityEditor.Editor
    {
        SerializedProperty _id;
        SerializedProperty _clips;
        SerializedProperty _clipSelectionMode;
        SerializedProperty _output;
        SerializedProperty _loop;
        SerializedProperty _ignoreListenerPause;
        SerializedProperty _useUnscaledTime;
        SerializedProperty _volume;
        SerializedProperty _randomizeVolume;
        SerializedProperty _volumeMin;
        SerializedProperty _volumeMax;
        SerializedProperty _pitch;
        SerializedProperty _randomizePitch;
        SerializedProperty _pitchMin;
        SerializedProperty _pitchMax;
        SerializedProperty _delay;
        SerializedProperty _randomizeDelay;
        SerializedProperty _delayMin;
        SerializedProperty _delayMax;
        SerializedProperty _useFadeIn;
        SerializedProperty _fadeInDuration;
        SerializedProperty _useFadeOut;
        SerializedProperty _fadeOutDuration;
        SerializedProperty _spatialBlend;
        SerializedProperty _minDistance;
        SerializedProperty _useMaxDistance;
        SerializedProperty _maxDistance;
        SerializedProperty _rolloffMode;
        SerializedProperty _dopplerLevel;
        SerializedProperty _spread;
        SerializedProperty _cooldown;
        SerializedProperty _maxSimultaneous;
        SerializedProperty _duplicate;
        SerializedProperty _duplicateBehavior;
        SerializedProperty _occlusionMode;
        SerializedProperty _networkMode;
        SerializedProperty _priority;

        void OnEnable()
        {
            _id = serializedObject.FindProperty("id");
            _clips = serializedObject.FindProperty("clips");
            _clipSelectionMode = serializedObject.FindProperty("clipSelectionMode");
            _output = serializedObject.FindProperty("output");
            _loop = serializedObject.FindProperty("loop");
            _ignoreListenerPause = serializedObject.FindProperty("ignoreListenerPause");
            _useUnscaledTime = serializedObject.FindProperty("useUnscaledTime");
            _volume = serializedObject.FindProperty("volume");
            _randomizeVolume = serializedObject.FindProperty("randomizeVolume");
            _volumeMin = serializedObject.FindProperty("volumeMin");
            _volumeMax = serializedObject.FindProperty("volumeMax");
            _pitch = serializedObject.FindProperty("pitch");
            _randomizePitch = serializedObject.FindProperty("randomizePitch");
            _pitchMin = serializedObject.FindProperty("pitchMin");
            _pitchMax = serializedObject.FindProperty("pitchMax");
            _delay = serializedObject.FindProperty("delay");
            _randomizeDelay = serializedObject.FindProperty("randomizeDelay");
            _delayMin = serializedObject.FindProperty("delayMin");
            _delayMax = serializedObject.FindProperty("delayMax");
            _useFadeIn = serializedObject.FindProperty("useFadeIn");
            _fadeInDuration = serializedObject.FindProperty("fadeInDuration");
            _useFadeOut = serializedObject.FindProperty("useFadeOut");
            _fadeOutDuration = serializedObject.FindProperty("fadeOutDuration");
            _spatialBlend = serializedObject.FindProperty("spatialBlend");
            _minDistance = serializedObject.FindProperty("minDistance");
            _useMaxDistance = serializedObject.FindProperty("useMaxDistance");
            _maxDistance = serializedObject.FindProperty("maxDistance");
            _rolloffMode = serializedObject.FindProperty("rolloffMode");
            _dopplerLevel = serializedObject.FindProperty("dopplerLevel");
            _spread = serializedObject.FindProperty("spread");
            _cooldown = serializedObject.FindProperty("cooldown");
            _maxSimultaneous = serializedObject.FindProperty("maxSimultaneous");
            _duplicate = serializedObject.FindProperty("duplicate");
            _duplicateBehavior = serializedObject.FindProperty("duplicateBehavior");
            _occlusionMode = serializedObject.FindProperty("occlusionMode");
            _networkMode = serializedObject.FindProperty("networkMode");
            _priority = serializedObject.FindProperty("priority");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSection("Cue", () =>
            {
                EditorGUILayout.PropertyField(_id);
                EditorGUILayout.PropertyField(_output);
                EditorGUILayout.PropertyField(_networkMode);
                DrawNetworkModeHelp();
            });

            DrawSection("Clips", () =>
            {
                EditorGUILayout.PropertyField(_clips);
                EditorGUILayout.PropertyField(_clipSelectionMode);
            });

            DrawSection("Playback", () =>
            {
                EditorGUILayout.PropertyField(_loop);
                EditorGUILayout.PropertyField(_priority);
                EditorGUILayout.PropertyField(_ignoreListenerPause);
            });

            DrawSection("Volume", () =>
            {
                EditorGUILayout.PropertyField(_randomizeVolume);

                if (_randomizeVolume.boolValue)
                {
                    DrawMinMax(_volumeMin, _volumeMax);
                    return;
                }

                EditorGUILayout.PropertyField(_volume);
            });

            DrawSection("Pitch", () =>
            {
                EditorGUILayout.PropertyField(_randomizePitch);

                if (_randomizePitch.boolValue)
                {
                    DrawMinMax(_pitchMin, _pitchMax);
                    return;
                }

                EditorGUILayout.PropertyField(_pitch);
            });

            DrawSection("Timing", () =>
            {
                EditorGUILayout.PropertyField(_randomizeDelay);
                EditorGUILayout.PropertyField(_useUnscaledTime);

                if (_randomizeDelay.boolValue)
                {
                    DrawMinMax(_delayMin, _delayMax);
                }
                else
                {
                    EditorGUILayout.PropertyField(_delay);
                }

                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(_useFadeIn);
                if (_useFadeIn.boolValue)
                {
                    EditorGUILayout.PropertyField(_fadeInDuration);
                }

                EditorGUILayout.PropertyField(_useFadeOut);
                if (_useFadeOut.boolValue)
                {
                    EditorGUILayout.PropertyField(_fadeOutDuration);
                }
            });

            DrawSection("Spatial", () =>
            {
                EditorGUILayout.PropertyField(_spatialBlend);

                if (_spatialBlend.enumValueIndex != (int)AudioSpatialBlendMode.ThreeD)
                {
                    return;
                }

                EditorGUILayout.PropertyField(_minDistance);
                EditorGUILayout.PropertyField(_useMaxDistance);
                if (_useMaxDistance.boolValue)
                {
                    EditorGUILayout.PropertyField(_maxDistance);
                }

                EditorGUILayout.PropertyField(_rolloffMode);
                EditorGUILayout.PropertyField(_dopplerLevel);
                EditorGUILayout.PropertyField(_spread);
                EditorGUILayout.PropertyField(_occlusionMode);
            });

            DrawSection("Limits", () =>
            {
                EditorGUILayout.PropertyField(_cooldown);
                EditorGUILayout.PropertyField(_duplicate);

                if (_duplicate.hasMultipleDifferentValues || _duplicate.boolValue)
                {
                    EditorGUILayout.PropertyField(_maxSimultaneous);
                }

                if (_duplicate.hasMultipleDifferentValues || !_duplicate.boolValue)
                {
                    EditorGUILayout.PropertyField(_duplicateBehavior);
                }
            });

            serializedObject.ApplyModifiedProperties();
        }

        static void DrawSection(string title, System.Action draw)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            draw.Invoke();
            EditorGUILayout.EndVertical();
        }

        static void DrawMinMax(SerializedProperty min, SerializedProperty max)
        {
            EditorGUILayout.PropertyField(min);
            EditorGUILayout.PropertyField(max);
        }

        void DrawNetworkModeHelp()
        {
            if (_networkMode.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox(
                    "Network Mode controls who should request or hear this cue once network audio is wired.",
                    MessageType.Info);
                return;
            }

            AudioNetworkMode mode = (AudioNetworkMode)_networkMode.enumValueIndex;
            EditorGUILayout.HelpBox(GetNetworkModeHelp(mode), MessageType.Info);
        }

        static string GetNetworkModeHelp(AudioNetworkMode mode)
        {
            return mode switch
            {
                AudioNetworkMode.LocalOnly =>
                    "Local Only: Play only on this client. Use for UI clicks, menu music, local notifications, local-only ambience, and player feedback that nobody else should hear.",
                AudioNetworkMode.OwnerPredicted =>
                    "Owner Predicted: The owning player hears it immediately, then the network can confirm or broadcast it. Use for responsive local player actions like footsteps, jumps, tool starts, or quick interaction sounds.",
                AudioNetworkMode.ObservedOneShot =>
                    "Observed One Shot: A short sound should be heard by other observing clients near the event. Use for impacts, object drops, hammer hits, explosions, doors, or non-looping world SFX.",
                AudioNetworkMode.ServerAuthoritative =>
                    "Server Authoritative: The server decides if the sound is allowed, then tells clients to play it. Use for gameplay-important sounds that clients should not be able to fake or spam.",
                AudioNetworkMode.NetworkedLoopState =>
                    "Networked Loop State: Start, stop, and follow loop state should be replicated. Use for drill loops, vehicle idle, machines running, alarms, generators, or ambience attached to networked objects.",
                _ =>
                    "Network Mode controls how this cue should behave once network audio is wired."
            };
        }
    }
}
