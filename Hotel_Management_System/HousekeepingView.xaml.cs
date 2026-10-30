using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Dashboard_WPF.Database;
using MySqlConnector;

namespace Dashboard_WPF
{
    public partial class HousekeepingView : UserControl
    {
        public ObservableCollection<HousekeepingRow> Tasks { get; } = new();
        private HousekeepingRow? _editingTask;

        public HousekeepingView()
        {
            InitializeComponent();
            DataContext = this;
            CreatedAtPicker.SelectedDate = DateTime.Today;
            _ = LoadTasksAsync();
        }

        private async Task LoadTasksAsync()
        {
            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                await using MySqlCommand command = new("SELECT TaskId, RoomId, TaskType, Status, AssignedTo, CreatedAt, CompletedAt, Notes FROM HousekeepingTasks ORDER BY TaskId", connection);
                await using MySqlDataReader reader = await command.ExecuteReaderAsync();

                Tasks.Clear();
                while (await reader.ReadAsync())
                {
                    Tasks.Add(new HousekeepingRow
                    {
                        Id = Convert.ToInt32(reader["TaskId"]),
                        RoomId = reader["RoomId"].ToString() ?? string.Empty,
                        TaskType = reader["TaskType"].ToString() ?? string.Empty,
                        Status = reader["Status"].ToString() ?? string.Empty,
                        AssignedTo = reader["AssignedTo"].ToString() ?? string.Empty,
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        CompletedAt = reader.IsDBNull(reader.GetOrdinal("CompletedAt")) ? null : Convert.ToDateTime(reader["CompletedAt"]),
                        Notes = reader["Notes"].ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load housekeeping tasks.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(RoomIdTextBox.Text.Trim(), out int roomId) ||
                !int.TryParse(AssignedToTextBox.Text.Trim(), out int assignedTo) ||
                TaskTypeComboBox.SelectedItem == null ||
                StatusComboBox.SelectedItem == null ||
                CreatedAtPicker.SelectedDate == null)
            {
                MessageBox.Show("Please enter valid Room ID and Assigned To IDs and complete all task fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                string sql = _editingTask == null
                    ? "INSERT INTO HousekeepingTasks (RoomId, TaskType, Status, AssignedTo, CreatedAt, CompletedAt, Notes) VALUES (@RoomId, @TaskType, @Status, @AssignedTo, @CreatedAt, @CompletedAt, @Notes)"
                    : "UPDATE HousekeepingTasks SET RoomId=@RoomId, TaskType=@TaskType, Status=@Status, AssignedTo=@AssignedTo, CreatedAt=@CreatedAt, CompletedAt=@CompletedAt, Notes=@Notes WHERE TaskId=@TaskId";

                await using MySqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@RoomId", roomId);
                command.Parameters.AddWithValue("@TaskType", GetComboBoxValue(TaskTypeComboBox));
                command.Parameters.AddWithValue("@Status", GetComboBoxValue(StatusComboBox));
                command.Parameters.AddWithValue("@AssignedTo", assignedTo);
                command.Parameters.AddWithValue("@CreatedAt", CreatedAtPicker.SelectedDate.Value);
                command.Parameters.AddWithValue("@CompletedAt", CompletedAtPicker.SelectedDate.HasValue ? CompletedAtPicker.SelectedDate.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Notes", NotesTextBox.Text.Trim());
                if (_editingTask != null)
                    command.Parameters.AddWithValue("@TaskId", _editingTask.Id);

                await command.ExecuteNonQueryAsync();
                await LoadTasksAsync();
                MessageBox.Show("Housekeeping task saved successfully.", "Housekeeping", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save the housekeeping task.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearFields();

        private void ClearFields()
        {
            _editingTask = null;
            RoomIdTextBox.Clear();
            TaskTypeComboBox.SelectedIndex = -1;
            StatusComboBox.SelectedIndex = -1;
            AssignedToTextBox.Clear();
            CreatedAtPicker.SelectedDate = DateTime.Today;
            CompletedAtPicker.SelectedDate = null;
            NotesTextBox.Clear();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not HousekeepingRow task)
                return;

            _editingTask = task;
            RoomIdTextBox.Text = task.RoomId;
            SelectComboBoxItem(TaskTypeComboBox, task.TaskType);
            SelectComboBoxItem(StatusComboBox, task.Status);
            AssignedToTextBox.Text = task.AssignedTo;
            CreatedAtPicker.SelectedDate = task.CreatedAt;
            CompletedAtPicker.SelectedDate = task.CompletedAt;
            NotesTextBox.Text = task.Notes;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not HousekeepingRow task)
                return;

            if (MessageBox.Show("Delete this housekeeping task?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                await using MySqlCommand command = new("DELETE FROM HousekeepingTasks WHERE TaskId=@TaskId", connection);
                command.Parameters.AddWithValue("@TaskId", task.Id);
                await command.ExecuteNonQueryAsync();
                await LoadTasksAsync();
                if (ReferenceEquals(_editingTask, task))
                    ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not delete the housekeeping task.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GetComboBoxValue(ComboBox comboBox) => ((ComboBoxItem)comboBox.SelectedItem).Content?.ToString() ?? string.Empty;

        private static void SelectComboBoxItem(ComboBox comboBox, string value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content?.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
        }
    }

    public class HousekeepingRow
    {
        public int Id { get; set; }
        public string RoomId { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
