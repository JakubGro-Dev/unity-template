using Reflex.Core;
using Reflex.Enums;
using UnityEngine;
using Template.App.Flow;
using template.Audio;

namespace Template.Bootstrap
{
    public sealed class RootInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private LoggingSettings _loggingSettings;
        [SerializeField] private AudioCueDatabaseSO audioCueDatabase;
        [SerializeField, Min(0)] private int audioEmitterPrewarmCount = 24;

        public void InstallBindings(ContainerBuilder builder)
        {
            Logging.Initialize(_loggingSettings);

            if (audioCueDatabase == null)
            {
                throw new System.InvalidOperationException("RootInstaller: AudioCueDatabaseSO is not assigned.");
            }

            builder.RegisterValue(audioCueDatabase);
            builder.RegisterFactory(
                _ => new AudioService(audioCueDatabase, audioEmitterPrewarmCount),
                new[] { typeof(IAudioService) },
                Lifetime.Singleton,
                Reflex.Enums.Resolution.Lazy
            );

            builder.RegisterType( // Change Scene
                typeof(SceneLoader),
                new[] { typeof(ISceneLoader) },
                Lifetime.Singleton,
                Reflex.Enums.Resolution.Eager
            );
            
            builder.RegisterType( // AppFlow
                typeof(AppFlow),
                new[] { typeof(IAppFlow) },
                Lifetime.Singleton,
                Reflex.Enums.Resolution.Lazy
            );

            builder.RegisterType( // Boot Scene
                typeof(BootFlowHandler),
                new[] { typeof(IFlowHandler) },
                Lifetime.Singleton,
                Reflex.Enums.Resolution.Lazy
            );

            builder.RegisterType( // Boot Scene
                typeof(MainMenuFlowHandler),
                new[] { typeof(IFlowHandler) },
                Lifetime.Singleton,
                Reflex.Enums.Resolution.Lazy
            );
            builder.RegisterType(
                typeof(GameFlowHandler),
                new[] { typeof(IFlowHandler) },
                Lifetime.Singleton,
                Reflex.Enums.Resolution.Lazy
            );
            
        }
    }
}
