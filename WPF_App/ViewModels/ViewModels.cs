using System.Collections.ObjectModel;
using WPF_App.Api;
using WPF_App.Infrastructure;
using WPF_App.Models;

namespace WPF_App.ViewModels;

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
        Roles = new RolesViewModel();
        currentPage = Heroes;
        ShowHeroesCommand = new RelayCommand(() => CurrentPage = Heroes);
        ShowRolesCommand = new RelayCommand(() => CurrentPage = Roles);
    }
}

public sealed class HeroesViewModel
{
    private readonly HeroesApiClient apiClient;

    public ObservableCollection<Hero> Heroes { get; } = [];

    public HeroesViewModel(HeroesApiClient apiClient)
    {
        this.apiClient = apiClient;
    }

    public async Task LoadAsync()
    {
        var heroes = await apiClient.GetHeroesAsync();
        Heroes.Clear();
        foreach (var hero in heroes)
        {
            Heroes.Add(hero);
        }
    }
}

public sealed class RolesViewModel 
{ }