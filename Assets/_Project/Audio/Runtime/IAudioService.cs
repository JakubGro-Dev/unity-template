using UnityEngine;

namespace template.Audio
{
    public interface IAudioService
    {
        int ActiveCount { get; }

        int GetActiveCount(string id);
        bool Preload(string id);
        void PreloadAll();
        AudioHandle Play(string id);
        AudioHandle Play(string id, Vector3 position);
        void Play2D(string id);
        void Play3D(string id, Vector3 position);
        AudioHandle PlayLoop2D(string id);
        AudioHandle PlayLoop3D(string id, Transform followTarget);
        AudioHandle PlayLoop(string id, Transform followTarget);
        bool IsPlaying(AudioHandle handle);
        bool IsPaused(AudioHandle handle);
        void Stop(AudioHandle handle);
        void StopAll();
        void StopAll(string id);
        void Pause(AudioHandle handle);
        void Resume(AudioHandle handle);
        void PauseAll();
        void PauseAll(string id);
        void ResumeAll();
        void ResumeAll(string id);
        void SetVolume(AudioHandle handle, float volume);
        void SetPitch(AudioHandle handle, float pitch);
        void SetPosition(AudioHandle handle, Vector3 position);
        void SetFollowTarget(AudioHandle handle, Transform followTarget);
    }
}
