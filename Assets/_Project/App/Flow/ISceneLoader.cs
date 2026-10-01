using Cysharp.Threading.Tasks;

namespace Template.App.Flow
{
    public interface ISceneLoader
    {
        UniTask AsyncLoadScene(AppScreen screen);
    }
}