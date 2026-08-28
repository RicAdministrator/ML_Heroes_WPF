namespace WPF_App.ViewModels;

using WPF_App.Infrastructure;

public sealed class MainViewModel : ObservableObject
{
    private object currentPage;
    public HeroesViewModel Heroes { get; }
    public RolesViewModel Roles { get; }
    public object CurrentPage { get => currentPage; private set => SetProperty(ref currentPage, value); }
    public RelayCommand ShowHeroesCommand { get; }
    public RelayCommand ShowRolesCommand { get; }

    public MainViewModel()
    {
        Heroes = new HeroesViewModel();
        Roles = new RolesViewModel();
        currentPage = Heroes;
        ShowHeroesCommand = new RelayCommand(() => CurrentPage = Heroes);
        ShowRolesCommand = new RelayCommand(() => CurrentPage = Roles);
    }
}

public sealed class HeroesViewModel
{
}

public sealed class RolesViewModel 
{ }