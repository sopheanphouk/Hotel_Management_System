
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

            SidebarColumn.Width = new GridLength(
                _isSidebarCollapsed
                    ? CollapsedSidebarWidth
                    : ExpandedSidebarWidth);

            Visibility labelVisibility = _isSidebarCollapsed
                ? Visibility.Collapsed
                : Visibility.Visible;

            UserNameText.Visibility = labelVisibility;
            UserRoleText.Visibility = labelVisibility;

            foreach (TextBlock label in FindNavLabels())
            {
                label.Visibility = labelVisibility;
            }
        }

        private IEnumerable<TextBlock> FindNavLabels()
        {
            foreach (RadioButton item in FindVisualChildren<RadioButton>(this))
            {
                if (item.Content is StackPanel panel &&
                    panel.Children.Count > 1 &&
                    panel.Children[1] is TextBlock label)
                {
                    yield return label;
                }
            }

            if (SignOutButton.Content is StackPanel signOutPanel &&
                signOutPanel.Children.Count > 1 &&
                signOutPanel.Children[1] is TextBlock signOutLabel)
            {
                yield return signOutLabel;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(
            DependencyObject parent) where T : DependencyObject
        {
            if (parent == null)
                yield break;

            int count = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                    yield return result;

                foreach (T descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        // ----- Navigation -----


        private void NavItem_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton item ||
                item.Tag is not string section)
            {
                return;
            }

            // Update page title only when the control exists.
            if (PageTitleText != null)
                PageTitleText.Text = section;

            if (PageSubtitleText != null)
            {
                PageSubtitleText.Text = section switch
                {
                    "Dashboard" => "Welcome back! Here's what's happening today.",
                    "Room Types" => "Manage your room types and pricing.",
                    "Rooms" => "Manage hotel rooms and room availability.",
                    "Guests" => "Manage guest information.",
                    "Reservations" => "Manage guest reservations.",
                    "Bookings" => "View and manage bookings.",
                    "Invoices" => "Manage guest invoices.",
                    "Payments" => "Track and manage payments.",
                    "Housekeeping" => "Manage room cleaning and status.",
                    "Staff" => "Manage hotel staff members.",
                    "Roles" => "Manage staff roles and permissions.",
                    _ => string.Empty
                };
            }

            // The content control may not be ready during initialization.
            if (PageContent == null)
                return;

            switch (section)
            {
                case "Room Types":
                    PageContent.Content = new RoomTypesView();
                    break;

                case "Dashboard":
                    PageContent.Content = null;
                    break;

                default:
                    PageContent.Content = null;
                    break;
            }
        }


        // ----- Sign out -----

        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to sign out?",
                "Sign Out",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Add login/session handling here when it is implemented.
                Close();
            }
        }
    }
}
