using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Dashboard_WPF.Database;
using MySqlConnector;

namespace Dashboard_WPF
{
    public partial class RolesView : UserControl
    {
        public ObservableCollection<RoleRow> Roles { get; } = new();
        private RoleRow? _editingRole;

        public RolesView()
        {
            InitializeComponent();
            DataContext = this;
            _ = LoadRolesAsync();
        }

        private async Task LoadRolesAsync()
        {
            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                await using MySqlCommand command = new("SELECT RoleId, RoleName FROM `Role` ORDER BY RoleId", connection);
                await using MySqlDataReader reader = await command.ExecuteReaderAsync();

                Roles.Clear();
                while (await reader.ReadAsync())
                {
                    Roles.Add(new RoleRow
                    {
                        RoleId = reader["RoleId"].ToString() ?? string.Empty,
                        RoleName = reader["RoleName"].ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load roles.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(RoleIdTextBox.Text.Trim(), out int roleId) ||
                string.IsNullOrWhiteSpace(RoleNameTextBox.Text))
            {
                MessageBox.Show("Please enter a valid Role ID and Role Name.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                string sql = _editingRole == null
                    ? "INSERT INTO `Role` (RoleId, RoleName) VALUES (@RoleId, @RoleName)"
                    : "UPDATE `Role` SET RoleId=@RoleId, RoleName=@RoleName WHERE RoleId=@OriginalRoleId";

                await using MySqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@RoleId", roleId);
                command.Parameters.AddWithValue("@RoleName", RoleNameTextBox.Text.Trim());
                if (_editingRole != null)
                    command.Parameters.AddWithValue("@OriginalRoleId", _editingRole.RoleId);

                await command.ExecuteNonQueryAsync();
                await LoadRolesAsync();
                MessageBox.Show("Role saved successfully.", "Roles", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save the role.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearFields();

        private void ClearFields()
        {
            _editingRole = null;
            RoleIdTextBox.Clear();
            RoleNameTextBox.Clear();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not RoleRow role)
                return;

            _editingRole = role;
            RoleIdTextBox.Text = role.RoleId;
            RoleNameTextBox.Text = role.RoleName;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not RoleRow role)
                return;

            if (MessageBox.Show("Delete this role?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                await using MySqlCommand command = new("DELETE FROM `Role` WHERE RoleId=@RoleId", connection);
                command.Parameters.AddWithValue("@RoleId", role.RoleId);
                await command.ExecuteNonQueryAsync();
                await LoadRolesAsync();
                if (ReferenceEquals(_editingRole, role))
                    ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not delete the role.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class RoleRow
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
