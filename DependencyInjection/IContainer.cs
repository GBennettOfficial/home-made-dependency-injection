
namespace DependencyInjection
{
    public interface IContainer
    {
        IContainer BranchScope();
        T Inject<T>(string key = "");
        void RegisterScoped<T>(Func<T> factory, string key = "");
        void RegisterSingleton<T>(Func<T> factory, string key = "");
        void RegisterTransient<T>(Func<T> factory, string key = "");
    }
}