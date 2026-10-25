using System.Windows;
using System.Windows.Controls;

namespace Dashboard_WPF
{
    public partial class MainWindow : Window
    {
        private const double ExpandedSidebarWidth = 220;
        private const double CollapsedSidebarWidth = 64;
        private bool _isSidebarCollapsed;

        public MainWindow()
        {
            InitializeComponent();
        }

        // ----- Title bar controls -----

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ----- Sidebar -----

        private void ToggleSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            _isSidebarCollapsed = !_isSidebarCollapsed;
            SidebarColumn.Width = new GridLength(_isSidebarCollapsed ? CollapsedSidebarWidth : ExpandedSidebarWidth);

            var labelVisibility = _isSidebarCollapsed ? Visibility.Collapsed : Visibility.Visible;
            UserNameText.Visibility = labelVisibility;
            UserRoleText.Visibility = labelVisibility;

            foreach (var label in FindNavLabels())
            {
                label.Visibility = labelVisibility;
            }
        }

        private System.Collections.Generic.IEnumerable<TextBlock> FindNavLabels()
        {
            yield return (TextBlock)((StackPanel)HomeNavItem.Content).Children[1];
            yield return (TextBlock)((StackPanel)UserNavItem.Content).Children[1];
            yield return (TextBlock)((StackPanel)SettingNavItem.Content).Children[1];
            yield return (TextBlock)((StackPanel)SignOutButton.Content).Children[1];
        }

        // ----- Navigation -----

        private void NavItem_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton { Tag: string section })
            {
                return;
            }

            (PageTitleText.Text, PageSubtitleText.Text) = section switch
            {
                "Home" => ("Home", "Welcome back, here's what's happening today."),
                "User" => ("User", "Manage user accounts and permissions."),
                "Setting" => ("Setting", "Configure application preferences."),
                _ => (section, string.Empty),
            };

            // TODO: swap PageContent.Content for the real view associated with `section`,
            // e.g. PageContent.Content = new HomeView();
        }

        // ----- Sign out -----

        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to sign out?",
                "Sign Out",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // TODO: clear session/auth state and navigate to the login window.
                Close();
            }
        }
    }
}