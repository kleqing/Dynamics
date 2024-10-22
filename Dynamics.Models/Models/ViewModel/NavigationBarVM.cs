namespace Dynamics.Models.Models.ViewModel;

public class NavigationBarVM
{
    public string Type { get; set; } // Vertical, horizontal with dash, horizontal without dash (Need to setup for them)
    public List<string>? NavigationItems { get; set; }
    public string? ActiveItem { get; set; }
}