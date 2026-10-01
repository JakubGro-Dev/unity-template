using UnityEngine;

[CreateAssetMenu(fileName = "LoggingSettings", menuName = "Template/Logging Settings")]
public sealed class LoggingSettings : ScriptableObject
{
    [field: SerializeField] public bool DisplayLogging { get; private set; } = true;
}
