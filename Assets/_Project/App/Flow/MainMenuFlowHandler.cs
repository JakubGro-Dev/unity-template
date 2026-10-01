using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Template.App.Flow
{
    public sealed class MainMenuFlowHandler : IFlowHandler
    {
        public AppScreen Screen => AppScreen.MainMenu;
        readonly ISceneLoader _sceneLoader;

        public MainMenuFlowHandler(ISceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public async UniTask EnterAsync()
        {
            // Simulate boot process
            Logging.Log("MainMenu process started.");

            Logging.Log("Loading Main Menu Scene.");
            await _sceneLoader.AsyncLoadScene(AppScreen.MainMenu);
            Logging.Log("Loading finished.");
        }

        public async UniTask ExitAsync()
        {
            // Simulate exit process
            Logging.Log("MainMenu process completed.");
        }
    }
}