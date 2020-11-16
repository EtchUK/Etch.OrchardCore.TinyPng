using System.IO;
using System.Threading.Tasks;

namespace Etch.OrchardCore.TinyPNG.Services
{
    public interface ITinyPngService
    {
        Task<Stream> OptimiseAsync(Stream stream, string fileName);
    }
}
