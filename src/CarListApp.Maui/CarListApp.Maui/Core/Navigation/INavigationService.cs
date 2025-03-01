using System.Threading.Tasks;
using System.Collections.Generic;

namespace CarListApp.Maui.Core.Navigation
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route, bool isAbsolute = false);
        Task NavigateBackAsync();
        Task InitializeAsync();
        Task NavigateToLoginAsync();
        Task NavigateToMainAsync();
        Task GoToAsync(string route);
        Task GoToAsync(string route, IDictionary<string, object> parameters);
    }
} 