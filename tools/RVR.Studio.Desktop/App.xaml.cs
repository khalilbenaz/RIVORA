namespace RVR.Studio.Desktop;

/// <summary>
/// Main MAUI Application class for RVR Studio Desktop.
/// </summary>
public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new MainPage();
    }
}
