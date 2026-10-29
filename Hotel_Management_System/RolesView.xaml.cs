using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(RoleNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionTextBox.Text) ||
                string.IsNullOrWhiteSpace(BuildPermissions()))
            {
                MessageBox.Show("Please complete all role fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            RoleRow role = new()
            {
                RoleName = RoleNameTextBox.Text.Trim(),
                Description = DescriptionTextBox.Text.Trim(),
                Permissions = BuildPermissions()
            };

            if (_editingRole == null)
            {
                role.Id = Roles.Count + 1;
                Roles.Add(role);
            }
            else
            {
                role.Id = _editingRole.Id;
                Roles[Roles.IndexOf(_editingRole)] = role;
            }

            MessageBox.Show("Role saved successfully.", "Roles", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearFields();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            _editingRole = null;
            RoleNameTextBox.Clear();
            DescriptionTextBox.Clear();
            foreach (CheckBox checkBox in GetPermissionCheckBoxes())
                checkBox.IsChecked = false;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not RoleRow role)
                return;

            _editingRole = role;
            RoleNameTextBox.Text = role.RoleName;
            DescriptionTextBox.Text = role.Description;
            SetPermissions(role.Permissions);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not RoleRow role)
                return;

            if (MessageBox.Show("Delete this role?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Roles.Remove(role);
            ReassignIds();
            if (ReferenceEquals(_editingRole, role))
                ClearFields();
        }

        private void SetPermissions(string permissions)
        {
            foreach (CheckBox checkBox in GetPermissionCheckBoxes())
                checkBox.IsChecked = false;

            foreach (string modulePermissions in permissions.Split(';', System.StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = modulePermissions.Split(':', 2);
                if (parts.Length != 2)
                    continue;

                string module = parts[0].Trim();
                string[] actions = parts[1].Split(',', System.StringSplitOptions.RemoveEmptyEntries);
                SetModulePermissions(module, actions);
            }
        }

        private void SetModulePermissions(string module, string[] actions)
        {
            CheckBox? view = null;
            CheckBox? add = null;
            CheckBox? edit = null;
            CheckBox? delete = null;

            switch (module)
            {
                case "Rooms":
                    view = RoomsViewCheckBox; add = RoomsAddCheckBox; edit = RoomsEditCheckBox; delete = RoomsDeleteCheckBox;
                    break;
                case "Guests":
                    view = GuestsViewCheckBox; add = GuestsAddCheckBox; edit = GuestsEditCheckBox; delete = GuestsDeleteCheckBox;
                    break;
                case "Reservations":
                    view = ReservationsViewCheckBox; add = ReservationsAddCheckBox; edit = ReservationsEditCheckBox; delete = ReservationsDeleteCheckBox;
                    break;
                case "Invoices":
                    view = InvoicesViewCheckBox; add = InvoicesAddCheckBox; edit = InvoicesEditCheckBox; delete = InvoicesDeleteCheckBox;
                    break;
                case "Payments":
                    view = PaymentsViewCheckBox; add = PaymentsAddCheckBox; edit = PaymentsEditCheckBox; delete = PaymentsDeleteCheckBox;
                    break;
                case "Staff":
                    view = StaffViewCheckBox; add = StaffAddCheckBox; edit = StaffEditCheckBox; delete = StaffDeleteCheckBox;
                    break;
            }

            if (view == null)
                return;

            view.IsChecked = ArrayContains(actions, "View");
            add!.IsChecked = ArrayContains(actions, "Add");
            edit!.IsChecked = ArrayContains(actions, "Edit");
            delete!.IsChecked = ArrayContains(actions, "Delete");
        }

        private static bool ArrayContains(string[] values, string value)
        {
            foreach (string item in values)
            {
                if (item.Trim() == value)
                    return true;
            }

            return false;
        }

        private void ReassignIds()
        {
            for (int i = 0; i < Roles.Count; i++)
                Roles[i].Id = i + 1;
        }

        private string BuildPermissions()
        {
            List<string> permissions = new();
            AddModulePermissions(permissions, "Rooms", RoomsViewCheckBox, RoomsAddCheckBox, RoomsEditCheckBox, RoomsDeleteCheckBox);
            AddModulePermissions(permissions, "Guests", GuestsViewCheckBox, GuestsAddCheckBox, GuestsEditCheckBox, GuestsDeleteCheckBox);
            AddModulePermissions(permissions, "Reservations", ReservationsViewCheckBox, ReservationsAddCheckBox, ReservationsEditCheckBox, ReservationsDeleteCheckBox);
            AddModulePermissions(permissions, "Invoices", InvoicesViewCheckBox, InvoicesAddCheckBox, InvoicesEditCheckBox, InvoicesDeleteCheckBox);
            AddModulePermissions(permissions, "Payments", PaymentsViewCheckBox, PaymentsAddCheckBox, PaymentsEditCheckBox, PaymentsDeleteCheckBox);
            AddModulePermissions(permissions, "Staff", StaffViewCheckBox, StaffAddCheckBox, StaffEditCheckBox, StaffDeleteCheckBox);
            return string.Join("; ", permissions);
        }

        private static void AddModulePermissions(List<string> permissions, string module, params CheckBox[] checkBoxes)
        {
            string[] names = { "View", "Add", "Edit", "Delete" };
            List<string> selected = new();

            for (int i = 0; i < checkBoxes.Length; i++)
            {
                if (checkBoxes[i].IsChecked == true)
                    selected.Add(names[i]);
            }

            if (selected.Count > 0)
                permissions.Add($"{module}: {string.Join(", ", selected)}");
        }

        private IEnumerable<CheckBox> GetPermissionCheckBoxes()
        {
            yield return RoomsViewCheckBox;
            yield return RoomsAddCheckBox;
            yield return RoomsEditCheckBox;
            yield return RoomsDeleteCheckBox;
            yield return GuestsViewCheckBox;
            yield return GuestsAddCheckBox;
            yield return GuestsEditCheckBox;
            yield return GuestsDeleteCheckBox;
            yield return ReservationsViewCheckBox;
            yield return ReservationsAddCheckBox;
            yield return ReservationsEditCheckBox;
            yield return ReservationsDeleteCheckBox;
            yield return InvoicesViewCheckBox;
            yield return InvoicesAddCheckBox;
            yield return InvoicesEditCheckBox;
            yield return InvoicesDeleteCheckBox;
            yield return PaymentsViewCheckBox;
            yield return PaymentsAddCheckBox;
            yield return PaymentsEditCheckBox;
            yield return PaymentsDeleteCheckBox;
            yield return StaffViewCheckBox;
            yield return StaffAddCheckBox;
            yield return StaffEditCheckBox;
            yield return StaffDeleteCheckBox;
        }
    }

    public class RoleRow
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Permissions { get; set; } = string.Empty;
    }
}
