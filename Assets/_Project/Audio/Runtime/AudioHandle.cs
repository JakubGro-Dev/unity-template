using System;

namespace template.Audio
{
    public readonly struct AudioHandle : IEquatable<AudioHandle>
    {
        internal readonly AudioEmitter Emitter;
        internal readonly int Version;

        public string CueId { get; }
        public bool IsValid => Emitter != null && Emitter.HandleVersion == Version && Emitter.IsInUse;
        public bool IsPaused => IsValid && Emitter.IsPaused;

        internal AudioHandle(AudioEmitter emitter, int version, string cueId)
        {
            Emitter = emitter;
            Version = version;
            CueId = cueId;
        }

        public bool Equals(AudioHandle other)
        {
            return Emitter == other.Emitter && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            return obj is AudioHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Emitter != null ? Emitter.GetHashCode() : 0) * 397) ^ Version;
            }
        }

        public static bool operator ==(AudioHandle left, AudioHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AudioHandle left, AudioHandle right)
        {
            return !left.Equals(right);
        }
    }
}
