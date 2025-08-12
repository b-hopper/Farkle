using System.Threading.Tasks;

namespace Farkle.Managers
{
    /// <summary>
    /// Interface implemented by all game managers that require asynchronous
    /// initialization. Managers implementing this interface can perform
    /// any Addressables or file I/O in their InitAsync method and are
    /// guaranteed to be fully initialized before other systems attempt
    /// to use them.
    /// </summary>
    public interface IGameManager
    {
        Task InitAsync();
    }
}
