namespace CarListApp.Maui.Core.Http
{
    public interface IHttpClientFactory
    {
        Task<HttpClient> CreateClient(bool requiresAuth = true);
    }
} 