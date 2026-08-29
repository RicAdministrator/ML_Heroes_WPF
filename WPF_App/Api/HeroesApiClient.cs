using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using WPF_App.Models;

namespace WPF_App.Api;

public sealed class HeroesApiClient
{
    private readonly HttpClient httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public HeroesApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
    public async Task<IReadOnlyList<Hero>> GetHeroesAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<Hero>>("api/heroes", cancellationToken) ?? [];

    public async Task<IReadOnlyList<Role>> GetRolesAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<Role>>("api/roles", cancellationToken) ?? [];

    public async Task<IReadOnlyList<HeroRole>> GetHeroRolesAsync(CancellationToken cancellationToken = default) =>
        await GetAsync<List<HeroRole>>("api/hero_roles", cancellationToken) ?? [];

    public Task SaveHeroAsync(int? id, HeroRequest request, CancellationToken cancellationToken = default) =>
        SendAsync(id is null ? HttpMethod.Post : HttpMethod.Put, id is null ? "api/heroes" : $"api/heroes/{id}", request, cancellationToken);

    public Task DeleteHeroAsync(int id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, $"api/heroes/{id}", null, cancellationToken);

    public Task SaveRoleAsync(int? id, RoleRequest request, CancellationToken cancellationToken = default) =>
        SendAsync(id is null ? HttpMethod.Post : HttpMethod.Put, id is null ? "api/roles" : $"api/roles/{id}", request, cancellationToken);

    public Task DeleteRoleAsync(int id, CancellationToken cancellationToken = default) =>
        SendAsync(HttpMethod.Delete, $"api/roles/{id}", null, cancellationToken);

    private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private async Task SendAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        using var response = await httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detail = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"The server returned {(int)response.StatusCode} {response.ReasonPhrase}. {detail}".Trim());
    }
}