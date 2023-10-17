using System.Threading.Tasks;
using Deploy.Editor.Data;

namespace Deploy.Editor.BackEnds
{
    public interface ICicdBackend
    {
        Task<bool> BuildAndDeploy(DeployContext context);
    }
}