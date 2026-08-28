using System.Text.Json.Serialization;

namespace WPF_App.Models;

public sealed class Hero
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class Role
{
    public int Id { get; set; }
    [JsonPropertyName("role")]
    public string RoleName { get; set; } = string.Empty;
    [JsonPropertyName("logo_url")]
    public string LogoUrl { get; set; } = string.Empty;
    [JsonPropertyName("primary_function")]
    public string PrimaryFunction { get; set; } = string.Empty;
    [JsonPropertyName("key_attributes")]
    public string KeyAttributes { get; set; } = string.Empty;
}

public sealed class HeroRequest
{
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("image_url")]
    public string ImageUrl { get; set; } = string.Empty;
    public int[] Roles { get; set; } = [];
    public string Description { get; set; } = string.Empty;
}

public sealed class RoleRequest
{
    public string Role { get; set; } = string.Empty;
    [JsonPropertyName("logo_url")]
    public string LogoUrl { get; set; } = string.Empty;
    [JsonPropertyName("primary_function")]
    public string PrimaryFunction { get; set; } = string.Empty;
    [JsonPropertyName("key_attributes")]
    public string KeyAttributes { get; set; } = string.Empty;
}