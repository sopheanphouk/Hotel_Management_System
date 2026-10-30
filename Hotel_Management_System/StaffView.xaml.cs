using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Dashboard_WPF.Database;
using MySqlConnector;

namespace Dashboard_WPF
{
    public partial class StaffView : UserControl
    {
        public ObservableCollection<StaffRow> StaffMembers { get; } = new();
        private StaffRow? _editingStaff;

        public StaffView()
        {
            InitializeComponent();
            DataContext = this;
            CreatedAtPicker.SelectedDate = DateTime.Today;
            _ = LoadStaffAsync();
        }

        private async Task LoadStaffAsync()
        {
            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                await using MySqlCommand command = new("SELECT StaffId, FullName, Username, PasswordHash, RoleId, IsActive, CreatedAt FROM Staff ORDER BY StaffId", connection);
                await using MySqlDataReader reader = await command.ExecuteReaderAsync();

                StaffMembers.Clear();
                while (await reader.ReadAsync())
                {
                    StaffMembers.Add(new StaffRow
                    {
                        Id = Convert.ToInt32(reader["StaffId"]),
                        FullName = reader["FullName"].ToString() ?? string.Empty,
                        Username = reader["Username"].ToString() ?? string.Empty,
                        PasswordHash = reader["PasswordHash"].ToString() ?? string.Empty,
                        RoleId = reader["RoleId"].ToString() ?? string.Empty,
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load staff records.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(RoleIdTextBox.Text.Trim(), out int roleId) ||
                string.IsNullOrWhiteSpace(FullNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(UsernameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordHashTextBox.Text) ||
                CreatedAtPicker.SelectedDate == null)
            {
                MessageBox.Show("Please complete the staff fields with a valid Role ID.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                string sql;
                if (_editingStaff == null)
                {
                    sql = "INSERT INTO Staff (FullName, Username, PasswordHash, RoleId, IsActive, CreatedAt) VALUES (@FullName, @Username, @PasswordHash, @RoleId, @IsActive, @CreatedAt)";
                }
                else
                {
                    sql = "UPDATE Staff SET FullName=@FullName, Username=@Username, PasswordHash=@PasswordHash, RoleId=@RoleId, IsActive=@IsActive, CreatedAt=@CreatedAt WHERE StaffId=@StaffId";
                }

                await using MySqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@FullName", FullNameTextBox.Text.Trim());
                command.Parameters.AddWithValue("@Username", UsernameTextBox.Text.Trim());
                command.Parameters.AddWithValue("@PasswordHash", PasswordHashTextBox.Text.Trim());
                command.Parameters.AddWithValue("@RoleId", roleId);
                command.Parameters.AddWithValue("@IsActive", IsActiveCheckBox.IsChecked == true);
                command.Parameters.AddWithValue("@CreatedAt", CreatedAtPicker.SelectedDate.Value);
                if (_editingStaff != null)
                    command.Parameters.AddWithValue("@StaffId", _editingStaff.Id);

                await command.ExecuteNonQueryAsync();
                await LoadStaffAsync();
                MessageBox.Show("Staff record saved successfully.", "Staff", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save the staff record.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearFields();

        private void ClearFields()
        {
            _editingStaff = null;
            FullNameTextBox.Clear();
            UsernameTextBox.Clear();
            PasswordHashTextBox.Clear();
            RoleIdTextBox.Clear();
            IsActiveCheckBox.IsChecked = true;
            CreatedAtPicker.SelectedDate = DateTime.Today;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not StaffRow staff)
                return;

            _editingStaff = staff;
            FullNameTextBox.Text = staff.FullName;
            UsernameTextBox.Text = staff.Username;
            PasswordHashTextBox.Text = staff.PasswordHash;
            RoleIdTextBox.Text = staff.RoleId;
            IsActiveCheckBox.IsChecked = staff.IsActive;
            CreatedAtPicker.SelectedDate = staff.CreatedAt;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not StaffRow staff)
                return;

            if (MessageBox.Show("Delete this staff record?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                await using MySqlCommand command = new("DELETE FROM Staff WHERE StaffId=@StaffId", connection);
                command.Parameters.AddWithValue("@StaffId", staff.Id);
                await command.ExecuteNonQueryAsync();
                await LoadStaffAsync();
                if (ReferenceEquals(_editingStaff, staff))
                    ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not delete the staff record.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class StaffRow
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
