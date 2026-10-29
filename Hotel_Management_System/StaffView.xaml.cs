using System.Collections.ObjectModel;
using System;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text) ||
                RoleComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                DateOfBirthPicker.SelectedDate == null ||
                GetSelectedGender() == null ||
                string.IsNullOrWhiteSpace(PlaceTextBox.Text) ||
                string.IsNullOrWhiteSpace(ProfileTextBox.Text))
            {
                MessageBox.Show("Please complete all staff fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            StaffRow staff = new()
            {
                FullName = FullNameTextBox.Text.Trim(),
                Role = ((ComboBoxItem)RoleComboBox.SelectedItem).Content.ToString() ?? string.Empty,
                Phone = PhoneTextBox.Text.Trim(),
                Email = EmailTextBox.Text.Trim(),
                DateOfBirth = DateOfBirthPicker.SelectedDate.Value,
                Gender = GetSelectedGender()!,
                Place = PlaceTextBox.Text.Trim(),
                PhotoPath = PhotoPathText.Text,
                Profile = ProfileTextBox.Text.Trim()
            };

            if (_editingStaff == null)
            {
                staff.Id = StaffMembers.Count + 1;
                StaffMembers.Add(staff);
            }
            else
            {
                staff.Id = _editingStaff.Id;
                StaffMembers[StaffMembers.IndexOf(_editingStaff)] = staff;
            }

            MessageBox.Show("Staff member saved successfully.", "Staff", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearFields();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            _editingStaff = null;
            FullNameTextBox.Clear();
            RoleComboBox.SelectedIndex = -1;
            PhoneTextBox.Clear();
            EmailTextBox.Clear();
            DateOfBirthPicker.SelectedDate = null;
            MaleRadioButton.IsChecked = false;
            FemaleRadioButton.IsChecked = false;
            OtherRadioButton.IsChecked = false;
            PlaceTextBox.Clear();
            ProfileTextBox.Clear();
            ProfileImage.Source = null;
            PhotoPathText.Text = string.Empty;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not StaffRow staff)
                return;

            _editingStaff = staff;
            FullNameTextBox.Text = staff.FullName;
            SelectComboBoxItem(RoleComboBox, staff.Role);
            PhoneTextBox.Text = staff.Phone;
            EmailTextBox.Text = staff.Email;
            DateOfBirthPicker.SelectedDate = staff.DateOfBirth;
            MaleRadioButton.IsChecked = staff.Gender == "Male";
            FemaleRadioButton.IsChecked = staff.Gender == "Female";
            OtherRadioButton.IsChecked = staff.Gender == "Other";
            PlaceTextBox.Text = staff.Place;
            ProfileTextBox.Text = staff.Profile;
            PhotoPathText.Text = staff.PhotoPath;

            if (!string.IsNullOrWhiteSpace(staff.PhotoPath) && File.Exists(staff.PhotoPath))
            {
                BitmapImage image = new();
                image.BeginInit();
                image.UriSource = new Uri(staff.PhotoPath);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                ProfileImage.Source = image;
            }
            else
            {
                ProfileImage.Source = null;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not StaffRow staff)
                return;

            if (MessageBox.Show("Delete this staff member?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            StaffMembers.Remove(staff);
            ReassignIds();
            if (ReferenceEquals(_editingStaff, staff))
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

        private void ReassignIds()
        {
            for (int i = 0; i < StaffMembers.Count; i++)
                StaffMembers[i].Id = i + 1;
        }

        private void UploadPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new()
            {
                Title = "Select staff profile photo",
                Filter = "Image files|*.jpg;*.jpeg;*.png;*.bmp|All files|*.*"
            };

            if (dialog.ShowDialog() != true)
                return;

            BitmapImage image = new();
            image.BeginInit();
            image.UriSource = new Uri(dialog.FileName);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            ProfileImage.Source = image;
            PhotoPathText.Text = dialog.FileName;
        }

        private string? GetSelectedGender()
        {
            if (MaleRadioButton.IsChecked == true)
                return "Male";

            if (FemaleRadioButton.IsChecked == true)
                return "Female";

            if (OtherRadioButton.IsChecked == true)
                return "Other";

            return null;
        }
    }

    public class StaffRow
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Place { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = string.Empty;
        public string Profile { get; set; } = string.Empty;
    }
}
