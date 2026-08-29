using System.Collections.ObjectModel;
using WPF_App.Api;
using WPF_App.Infrastructure;
using WPF_App.Models;

namespace WPF_App.ViewModels;

public abstract class ListViewModel : ObservableObject
{
    private bool isLoading;
    private string errorMessage = string.Empty;
    private string statusMessage = string.Empty;

    public bool IsLoading { get => isLoading; protected set => SetProperty(ref isLoading, value); }
    public string ErrorMessage { get => errorMessage; protected set => SetProperty(ref errorMessage, value); }
    public string StatusMessage { get => statusMessage; protected set => SetProperty(ref statusMessage, value); }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public bool HasStatus => !string.IsNullOrWhiteSpace(StatusMessage);

    protected void SetError(Exception exception)
    {
        ErrorMessage = exception.Message;
        OnPropertyChanged(nameof(HasError));
    }

    protected void ClearMessages()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(HasStatus));
    }

    protected void SetStatus(string message)
    {
        ErrorMessage = string.Empty;
        StatusMessage = message;
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(HasStatus));
    }
}

public sealed class MainViewModel : ObservableObject
{
    private object currentPage;
    public HeroesViewModel Heroes { get; }
    public RolesViewModel Roles { get; }
    public object CurrentPage { get => currentPage; private set => SetProperty(ref currentPage, value); }
    public RelayCommand ShowHeroesCommand { get; }
    public RelayCommand ShowRolesCommand { get; }

    public MainViewModel(HeroesApiClient apiClient)
    {
        Heroes = new HeroesViewModel(apiClient);
        Roles = new RolesViewModel(apiClient);
        currentPage = Heroes;
        ShowHeroesCommand = new RelayCommand(() => CurrentPage = Heroes);
        ShowRolesCommand = new RelayCommand(() => CurrentPage = Roles);
    }

    public async Task LoadAsync()
    {
        await Task.WhenAll(Heroes.LoadAsync(), Roles.LoadAsync());
    }
}

public sealed class RoleOptionViewModel : ObservableObject
{
    private bool isSelected;

    public int Id { get; }
    public string Name { get; }
    public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }

    public RoleOptionViewModel(Role role)
    {
        Id = role.Id;
        Name = role.RoleName;
    }
}

public sealed class HeroItemViewModel
{
    private readonly Action<HeroItemViewModel> update;
    private readonly Action<HeroItemViewModel> delete;
    public int Id { get; }
    public string Name { get; }
    public string ImageUrl { get; }
    public string Roles { get; }
    public string Description { get; }
    public RelayCommand UpdateCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public HeroItemViewModel(Hero hero, Action<HeroItemViewModel> update, Action<HeroItemViewModel> delete)
    {
        Id = hero.Id;
        Name = hero.Name;
        ImageUrl = hero.ImageUrl;
        Roles = hero.Roles;
        Description = hero.Description;
        this.update = update;
        this.delete = delete;
        UpdateCommand = new RelayCommand(() => update(this));
        DeleteCommand = new RelayCommand(() => this.delete(this));
    }
}

public sealed class HeroesViewModel : ListViewModel
{
    private readonly HeroesApiClient apiClient;
    private int? heroId;
    private string name = string.Empty;
    private string imageUrl = string.Empty;
    private string description = string.Empty;
    private string validationMessage = string.Empty;
    private bool isEditing;

    public ObservableCollection<HeroItemViewModel> Heroes { get; } = [];
    public ObservableCollection<RoleOptionViewModel> Roles { get; } = [];
    public int? HeroId { get => heroId; private set => SetProperty(ref heroId, value); }
    public string Name { get => name; set => SetProperty(ref name, value); }
    public string ImageUrl { get => imageUrl; set => SetProperty(ref imageUrl, value); }
    public string Description { get => description; set => SetProperty(ref description, value); }
    public string ValidationMessage { get => validationMessage; private set => SetProperty(ref validationMessage, value); }
    public bool IsEditing { get => isEditing; private set => SetProperty(ref isEditing, value); }
    public string FormTitle => HeroId.HasValue ? "Update Hero" : "Add Hero";
    public RelayCommand AddCommand { get; }
    public RelayCommand CancelCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }

    public HeroesViewModel(HeroesApiClient apiClient)
    {
        this.apiClient = apiClient;
        AddCommand = new RelayCommand(BeginAdd);
        CancelCommand = new RelayCommand(Cancel);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public async Task LoadAsync()
    {
        try
        {
            var roles = await apiClient.GetRolesAsync();
            Roles.Clear();
            foreach (var role in roles) Roles.Add(new RoleOptionViewModel(role));
            await RefreshHeroesAsync();
        }
        catch (Exception exception) { }
        finally { }
    }

    private async Task RefreshHeroesAsync()
    {
        var heroes = await apiClient.GetHeroesAsync();
        Heroes.Clear();
        foreach (var hero in heroes) Heroes.Add(new HeroItemViewModel(hero, BeginUpdate, DeleteAsync));
    }

    private void BeginAdd()
    {
        ClearMessages();
        ResetForm();
        IsEditing = true;
    }

    private void BeginUpdate(HeroItemViewModel item)
    {
        ClearMessages();
        HeroId = item.Id;
        Name = item.Name;
        ImageUrl = item.ImageUrl;
        Description = item.Description;
        var selectedNames = item.Roles.Split(" / ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var role in Roles)
        {
            role.IsSelected = selectedNames.Any(name => string.Equals(name, role.Name, StringComparison.OrdinalIgnoreCase));
        }
        OnPropertyChanged(nameof(FormTitle));
        IsEditing = true;
    }

    private async Task SaveAsync()
    {
        ValidationMessage = Validate();
        if (!string.IsNullOrWhiteSpace(ValidationMessage)) return;

        try
        {
            await apiClient.SaveHeroAsync(HeroId, new HeroRequest
            {
                Name = Name.Trim(),
                ImageUrl = ImageUrl.Trim(),
                Description = Description.Trim(),
                Roles = Roles.Where(role => role.IsSelected).Select(role => role.Id).ToArray()
            });
            var wasUpdate = HeroId.HasValue;
            ResetForm();
            IsEditing = false;
            SetStatus(wasUpdate ? "Hero was updated successfully." : "Hero was added successfully.");
            await RefreshHeroesAsync();
        }
        catch (Exception exception) { SetError(exception); }
    }

    private string Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) return "Name is required.";
        if (Heroes.Any(hero => string.Equals(hero.Name, Name.Trim(), StringComparison.OrdinalIgnoreCase) && hero.Id != HeroId)) return "Hero with this name already exists.";
        return Roles.Any(role => role.IsSelected) ? string.Empty : "At least one role must be selected.";
    }

    private async void DeleteAsync(HeroItemViewModel item)
    {
        ClearMessages();
        try
        {
            await apiClient.DeleteHeroAsync(item.Id);
            SetStatus("Hero was deleted successfully.");
            await RefreshHeroesAsync();
        }
        catch (Exception exception) { SetError(exception); }
    }

    private void Cancel()
    {
        ResetForm();
        IsEditing = false;
    }

    private void ResetForm()
    {
        HeroId = null;
        Name = string.Empty;
        ImageUrl = string.Empty;
        Description = string.Empty;
        ValidationMessage = string.Empty;
        foreach (var role in Roles) role.IsSelected = false;
        OnPropertyChanged(nameof(FormTitle));
    }
}

public sealed class RoleItemViewModel
{
    private readonly Action<RoleItemViewModel> update;
    public int Id { get; }
    public string RoleName { get; }
    public string LogoUrl { get; }
    public string PrimaryFunction { get; }
    public string KeyAttributes { get; }
    public RelayCommand UpdateCommand { get; }

    public RoleItemViewModel(Role role, Action<RoleItemViewModel> update)
    {
        Id = role.Id; RoleName = role.RoleName; LogoUrl = role.LogoUrl; PrimaryFunction = role.PrimaryFunction; KeyAttributes = role.KeyAttributes;
        UpdateCommand = new RelayCommand(() => update(this));
    }
}

public sealed class RolesViewModel : ListViewModel
{
    private readonly HeroesApiClient apiClient;
    private int? roleId;
    private string roleName = string.Empty;
    private string logoUrl = string.Empty;
    private string primaryFunction = string.Empty;
    private string keyAttributes = string.Empty;
    private string validationMessage = string.Empty;
    private bool isEditing;

    public ObservableCollection<RoleItemViewModel> Roles { get; } = [];
    public int? RoleId { get => roleId; private set => SetProperty(ref roleId, value); }
    public string RoleName { get => roleName; set => SetProperty(ref roleName, value); }
    public string LogoUrl { get => logoUrl; set => SetProperty(ref logoUrl, value); }
    public string PrimaryFunction { get => primaryFunction; set => SetProperty(ref primaryFunction, value); }
    public string KeyAttributes { get => keyAttributes; set => SetProperty(ref keyAttributes, value); }
    public string ValidationMessage { get => validationMessage; private set => SetProperty(ref validationMessage, value); }
    public bool IsEditing { get => isEditing; private set => SetProperty(ref isEditing, value); }
    public string FormTitle => RoleId.HasValue ? "Update Role" : "Add Role";
    public RelayCommand AddCommand { get; }
    public RelayCommand CancelCommand { get; }
    public AsyncRelayCommand SaveCommand { get; }

    public RolesViewModel(HeroesApiClient apiClient)
    {
        this.apiClient = apiClient;
        AddCommand = new RelayCommand(BeginAdd);
        CancelCommand = new RelayCommand(Cancel);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public async Task LoadAsync()
    {
        try { await RefreshRolesAsync(); }
        catch (Exception exception) { }
        finally { }
    }

    private async Task RefreshRolesAsync()
    {
        var roles = await apiClient.GetRolesAsync();
        Roles.Clear();
        foreach (var role in roles) Roles.Add(new RoleItemViewModel(role, BeginUpdate));
    }

    private void BeginAdd()
    {
        ClearMessages();
        ResetForm();
        IsEditing = true;
    }

    private void BeginUpdate(RoleItemViewModel item)
    {
        ClearMessages();
        RoleId = item.Id; RoleName = item.RoleName; LogoUrl = item.LogoUrl; PrimaryFunction = item.PrimaryFunction; KeyAttributes = item.KeyAttributes;
        OnPropertyChanged(nameof(FormTitle)); IsEditing = true;
    }

    private async Task SaveAsync()
    {
        ValidationMessage = Validate();
        if (!string.IsNullOrWhiteSpace(ValidationMessage)) return;

        try
        {
            await apiClient.SaveRoleAsync(RoleId, new RoleRequest { Role = RoleName.Trim(), LogoUrl = LogoUrl.Trim(), PrimaryFunction = PrimaryFunction.Trim(), KeyAttributes = KeyAttributes.Trim() });
            var wasUpdate = RoleId.HasValue;
            var savedRoleName = RoleName.Trim();
            ResetForm(); IsEditing = false;
            SetStatus(wasUpdate ? $"\"{savedRoleName}\" was updated successfully." : $"\"{savedRoleName}\" was added successfully.");
            await RefreshRolesAsync();
        }
        catch (Exception exception) { SetError(exception); }
    }

    private string Validate()
    {
        if (string.IsNullOrWhiteSpace(RoleName)) return "Role is required.";
        return Roles.Any(role => string.Equals(role.RoleName, RoleName.Trim(), StringComparison.OrdinalIgnoreCase) && role.Id != RoleId) ? "Role already exists." : string.Empty;
    }

    private void Cancel()
    {
        ResetForm(); IsEditing = false;
    }

    private void ResetForm()
    {
        RoleId = null; RoleName = string.Empty; LogoUrl = string.Empty; PrimaryFunction = string.Empty; KeyAttributes = string.Empty; ValidationMessage = string.Empty;
        OnPropertyChanged(nameof(FormTitle));
    }
}