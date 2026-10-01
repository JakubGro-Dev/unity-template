using Cysharp.Threading.Tasks;

namespace Template.App.Flow
{
    public interface IFlowHandler
    {
        AppScreen Screen { get; }
        UniTask EnterAsync();
        UniTask ExitAsync();
    }
}