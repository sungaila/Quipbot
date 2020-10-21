using System.Threading.Tasks;

namespace Quipbot
{
    public interface IInitializable
    {
        Task InitAsync();
    }
}