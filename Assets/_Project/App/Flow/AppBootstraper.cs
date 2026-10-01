using System;
using UnityEngine;
using Template.App.Flow;
using Reflex.Attributes;

namespace Template.App
{
    public sealed class AppBootstraper : MonoBehaviour
    {
        [Inject] private IAppFlow _appFlow;
        public void Start()
        {
            // Starts the sequence of appflow.
            Run();
        }

        private async void Run()
        {
            try
            {
                await _appFlow.StartAsync();
            }
            catch (Exception exception)
            {
                Logging.Log($"Exception occurred during app flow execution: {exception.Message}\n{exception.StackTrace}");
            }
        }
    }
}