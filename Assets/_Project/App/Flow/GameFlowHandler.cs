using Cysharp.Threading.Tasks;

namespace Template.App.Flow
{
    public sealed class GameFlowHandler : IFlowHandler
    {
        public AppScreen Screen => AppScreen.Game;
        public UniTask EnterAsync() => UniTask.CompletedTask;
        public UniTask ExitAsync() => UniTask.CompletedTask;
    }
}
