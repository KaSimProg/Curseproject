using System.Windows;
using System.Windows.Controls;
using Cursep.Models;
using Cursep.Views;
using System.Diagnostics;

namespace Cursep.Views
{
    public partial class MainWindow : Window
    {
        private User _currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            Debug.WriteLine($"=== MainWindow СОЗДАН. ID: {this.GetHashCode()} ===");
            Debug.WriteLine($"Для пользователя: {user.Username}");
            Debug.WriteLine($"Всего окон приложения: {Application.Current.Windows.Count}");

            // Выведем список всех окон
            foreach (Window window in Application.Current.Windows)
            {
                Debug.WriteLine($"Окно: {window.GetType().Name}, ID: {window.GetHashCode()}");
            }

            txtCurrentUser.Text = $"{user.FullName} ({user.Role.RoleName})";

            LoadRolePanel();

            btnLogout.Click += (s, e) =>
            {
                Debug.WriteLine($"=== Выход из системы ===");
                Application.Current.Shutdown();
            };
        }

        private void LoadRolePanel()
        {
            Debug.WriteLine($"=== Загрузка панели для роли: {_currentUser.RoleID} ===");

            switch (_currentUser.RoleID)
            {
                case 1: // Администратор
                    MainFrame.Navigate(new AdminPanel(_currentUser));
                    break;
                case 2: // Менеджер
                    MainFrame.Navigate(new ManagerPanel(_currentUser));
                    break;
                case 3: // Кладовщик
                    MainFrame.Navigate(new StorekeeperPanel(_currentUser));
                    break;
                default:
                    MessageBox.Show("Неизвестная роль пользователя");
                    break;
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            base.OnClosed(e);
            Debug.WriteLine($"=== MainWindow ЗАКРЫТ. ID: {this.GetHashCode()} ===");
        }
    }
}