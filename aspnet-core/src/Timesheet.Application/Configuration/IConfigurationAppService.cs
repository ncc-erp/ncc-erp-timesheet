using System.Threading.Tasks;
using Ncc.Configuration.Dto;

namespace Ncc.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
