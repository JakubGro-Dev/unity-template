using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using System;

namespace Template.App.Flow
{
    public class SceneLoader : ISceneLoader
    {
        public async UniTask AsyncLoadScene(AppScreen screen)
        {
            if (!Application.CanStreamedLevelBeLoaded(screen.ToString()))
                throw new InvalidOperationException($"Scene '{screen}' is not included in Build Settings or cannot be loaded.");

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(screen.ToString());

            while (!asyncLoad.isDone)
            {
                await UniTask.Yield();
            }

            return;
        }
    }
}