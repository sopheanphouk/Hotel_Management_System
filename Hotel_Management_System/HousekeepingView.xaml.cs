using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RoomNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(AssignedStaffTextBox.Text) ||
                RoomTypeComboBox.SelectedItem == null ||
                StatusComboBox.SelectedItem == null ||
                TaskTypeComboBox.SelectedItem == null ||
                PriorityComboBox.SelectedItem == null ||
                DueDatePicker.SelectedDate == null ||
                string.IsNullOrWhiteSpace(EstimatedMinutesTextBox.Text))
            {
                MessageBox.Show("Please complete all housekeeping fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            HousekeepingRow task = new()
            {
                RoomNumber = RoomNumberTextBox.Text.Trim(),
                AssignedStaff = AssignedStaffTextBox.Text.Trim(),
                RoomType = GetComboBoxValue(RoomTypeComboBox),
                Status = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString() ?? string.Empty,
                TaskType = GetComboBoxValue(TaskTypeComboBox),
                Priority = GetComboBoxValue(PriorityComboBox),
                DueDate = DueDatePicker.SelectedDate.Value,
                DueTime = DueTimeTextBox.Text.Trim(),
                EstimatedMinutes = EstimatedMinutesTextBox.Text.Trim(),
                Checklist = GetChecklist(),
                Notes = NotesTextBox.Text.Trim()
            };

            if (_editingTask == null)
            {
                task.Id = Tasks.Count + 1;
                Tasks.Add(task);
            }
            else
            {
                task.Id = _editingTask.Id;
                Tasks[Tasks.IndexOf(_editingTask)] = task;
            }

            MessageBox.Show("Housekeeping task saved successfully.", "Housekeeping", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearFields();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            _editingTask = null;
            RoomNumberTextBox.Clear();
            AssignedStaffTextBox.Clear();
            RoomTypeComboBox.SelectedIndex = -1;
            StatusComboBox.SelectedIndex = -1;
            TaskTypeComboBox.SelectedIndex = -1;
            PriorityComboBox.SelectedIndex = -1;
            DueDatePicker.SelectedDate = null;
            DueTimeTextBox.Clear();
            EstimatedMinutesTextBox.Clear();
            ChangedBedLinenCheckBox.IsChecked = false;
            CleanBathroomCheckBox.IsChecked = false;
            VacuumFloorCheckBox.IsChecked = false;
            RestockMinibarCheckBox.IsChecked = false;
            ReplaceTowelsCheckBox.IsChecked = false;
            EmptyTrashCheckBox.IsChecked = false;
            NotesTextBox.Clear();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not HousekeepingRow task)
                return;

            _editingTask = task;
            RoomNumberTextBox.Text = task.RoomNumber;
            AssignedStaffTextBox.Text = task.AssignedStaff;
            SelectComboBoxItem(RoomTypeComboBox, task.RoomType);
            SelectComboBoxItem(StatusComboBox, task.Status);
            SelectComboBoxItem(TaskTypeComboBox, task.TaskType);
            SelectComboBoxItem(PriorityComboBox, task.Priority);
            DueDatePicker.SelectedDate = task.DueDate;
            DueTimeTextBox.Text = task.DueTime;
            EstimatedMinutesTextBox.Text = task.EstimatedMinutes;
            SetChecklist(task.Checklist);
            NotesTextBox.Text = task.Notes;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not HousekeepingRow task)
                return;

            if (MessageBox.Show("Delete this housekeeping task?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Tasks.Remove(task);
            ReassignIds();
            if (ReferenceEquals(_editingTask, task))
                ClearFields();
        }

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

        private void SetChecklist(string checklist)
        {
            ChangedBedLinenCheckBox.IsChecked = checklist.Contains("Changed bed linen", StringComparison.Ordinal);
            CleanBathroomCheckBox.IsChecked = checklist.Contains("Clean bathroom", StringComparison.Ordinal);
            VacuumFloorCheckBox.IsChecked = checklist.Contains("Vacuum floor", StringComparison.Ordinal);
            RestockMinibarCheckBox.IsChecked = checklist.Contains("Restock minibar", StringComparison.Ordinal);
            ReplaceTowelsCheckBox.IsChecked = checklist.Contains("Replace towels", StringComparison.Ordinal);
            EmptyTrashCheckBox.IsChecked = checklist.Contains("Empty trash", StringComparison.Ordinal);
        }

        private void ReassignIds()
        {
            for (int i = 0; i < Tasks.Count; i++)
                Tasks[i].Id = i + 1;
        }

        private static string GetComboBoxValue(ComboBox comboBox)
        {
            return ((ComboBoxItem)comboBox.SelectedItem).Content.ToString() ?? string.Empty;
        }

        private string GetChecklist()
        {
            List<string> items = new();
            AddChecklistItem(items, "Changed bed linen", ChangedBedLinenCheckBox);
            AddChecklistItem(items, "Clean bathroom", CleanBathroomCheckBox);
            AddChecklistItem(items, "Vacuum floor", VacuumFloorCheckBox);
            AddChecklistItem(items, "Restock minibar", RestockMinibarCheckBox);
            AddChecklistItem(items, "Replace towels", ReplaceTowelsCheckBox);
            AddChecklistItem(items, "Empty trash", EmptyTrashCheckBox);
            return string.Join(", ", items);
        }

        private static void AddChecklistItem(List<string> items, string name, CheckBox checkBox)
        {
            if (checkBox.IsChecked == true)
                items.Add(name);
        }
    }

    public class HousekeepingRow
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string AssignedStaff { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string DueTime { get; set; } = string.Empty;
        public string EstimatedMinutes { get; set; } = string.Empty;
        public string Checklist { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
