using Cursep.Views;
using System.Windows;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Если нужно кастомное управление запуском
        var loginWindow = new LoginWindow();
        loginWindow.Show();

        // Или просто оставьте стандартное поведение
        base.OnStartup(e);
    }
}