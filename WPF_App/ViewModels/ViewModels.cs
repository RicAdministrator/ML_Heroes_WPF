namespace WPF_App.ViewModels;

public sealed class MainViewModel
{
    public HeroesViewModel Heroes { get; }
    public HeroesViewModel CurrentPage { get; }

    public MainViewModel()
    {
        Heroes = new HeroesViewModel();
        CurrentPage = Heroes;
    }
}
public sealed class HeroesViewModel
{
}