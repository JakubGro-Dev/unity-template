using UnityEngine;

namespace template.Audio
{
    public enum AudioNetworkMode
    {
        [InspectorName("Local Only")]
        LocalOnly,

        [InspectorName("Owner Predicted")]
        OwnerPredicted,

        [InspectorName("Observed One Shot")]
        ObservedOneShot,

        [InspectorName("Server Authoritative")]
        ServerAuthoritative,

        [InspectorName("Networked Loop State")]
        NetworkedLoopState
    }
}
