using Cysharp.Threading.Tasks;

namespace Template.App.Flow
{
    public interface IAppFlow
    {
        AppScreen Current { get; }
        UniTask StartAsync();
        UniTask GoToAsync(AppScreen target);
    }
}