using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Template.App.Flow
{
    public sealed class BootFlowHandler : IFlowHandler
    {
        public AppScreen Screen => AppScreen.Boot;

        public async UniTask EnterAsync()
        {
            // Simulate boot process
            Logging.Log("Boot process started.");
        }

        public async UniTask ExitAsync()
        {
            // Simulate exit process
            Logging.Log("Boot process completed.");
        }
    }
}