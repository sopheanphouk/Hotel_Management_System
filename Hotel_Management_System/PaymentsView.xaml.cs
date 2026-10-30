using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Dashboard_WPF.Database;
using MySqlConnector;

namespace Dashboard_WPF
{
    public partial class PaymentsView : UserControl
    {
        public ObservableCollection<PaymentRow> Payments { get; } = new();
        private PaymentRow? _editingPayment;

        public PaymentsView()
        {
            InitializeComponent();
            DataContext = this;
            PaidAtPicker.SelectedDate = DateTime.Today;
            _ = LoadPaymentsAsync();
        }

        private async Task LoadPaymentsAsync()
        {
            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                await using MySqlCommand command = new("SELECT PaymentId, InvoiceId, Amount, PaymentMethod, PaidAt, ReceivedBy FROM Payments ORDER BY PaymentId", connection);
                await using MySqlDataReader reader = await command.ExecuteReaderAsync();

                Payments.Clear();
                while (await reader.ReadAsync())
                {
                    Payments.Add(new PaymentRow
                    {
                        Id = Convert.ToInt32(reader["PaymentId"]),
                        InvoiceId = reader["InvoiceId"].ToString() ?? string.Empty,
                        Amount = Convert.ToDecimal(reader["Amount"]).ToString("0.00"),
                        PaymentMethod = reader["PaymentMethod"].ToString() ?? string.Empty,
                        PaidAt = Convert.ToDateTime(reader["PaidAt"]),
                        ReceivedBy = reader["ReceivedBy"].ToString() ?? string.Empty
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load payment records.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(InvoiceIdTextBox.Text.Trim(), out int invoiceId) ||
                !decimal.TryParse(AmountTextBox.Text.Trim(), out decimal amount) ||
                !int.TryParse(ReceivedByTextBox.Text.Trim(), out int receivedBy) ||
                PaymentMethodComboBox.SelectedItem == null ||
                PaidAtPicker.SelectedDate == null)
            {
                MessageBox.Show("Please enter valid Invoice ID, Amount, and Received By values.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                string sql = _editingPayment == null
                    ? "INSERT INTO Payments (InvoiceId, Amount, PaymentMethod, PaidAt, ReceivedBy) VALUES (@InvoiceId, @Amount, @PaymentMethod, @PaidAt, @ReceivedBy)"
                    : "UPDATE Payments SET InvoiceId=@InvoiceId, Amount=@Amount, PaymentMethod=@PaymentMethod, PaidAt=@PaidAt, ReceivedBy=@ReceivedBy WHERE PaymentId=@PaymentId";

                await using MySqlCommand command = new(sql, connection);
                command.Parameters.AddWithValue("@InvoiceId", invoiceId);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@PaymentMethod", GetComboBoxValue(PaymentMethodComboBox));
                command.Parameters.AddWithValue("@PaidAt", PaidAtPicker.SelectedDate.Value);
                command.Parameters.AddWithValue("@ReceivedBy", receivedBy);
                if (_editingPayment != null)
                    command.Parameters.AddWithValue("@PaymentId", _editingPayment.Id);

                await command.ExecuteNonQueryAsync();
                await LoadPaymentsAsync();
                MessageBox.Show("Payment saved successfully.", "Payment", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save the payment.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearFields();

        private void ClearFields()
        {
            _editingPayment = null;
            InvoiceIdTextBox.Clear();
            AmountTextBox.Clear();
            PaymentMethodComboBox.SelectedIndex = -1;
            PaidAtPicker.SelectedDate = DateTime.Today;
            ReceivedByTextBox.Clear();
            UpdateReceipt();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not PaymentRow payment)
                return;

            _editingPayment = payment;
            InvoiceIdTextBox.Text = payment.InvoiceId;
            AmountTextBox.Text = payment.Amount;
            SelectComboBoxItem(PaymentMethodComboBox, payment.PaymentMethod);
            PaidAtPicker.SelectedDate = payment.PaidAt;
            ReceivedByTextBox.Text = payment.ReceivedBy;
            UpdateReceipt();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not PaymentRow payment)
                return;

            if (MessageBox.Show("Delete this payment?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                await using MySqlConnection connection = await DatabaseHelper.OpenConnectionAsync();
                await using MySqlCommand command = new("DELETE FROM Payments WHERE PaymentId=@PaymentId", connection);
                command.Parameters.AddWithValue("@PaymentId", payment.Id);
                await command.ExecuteNonQueryAsync();
                await LoadPaymentsAsync();
                if (ReferenceEquals(_editingPayment, payment))
                    ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not delete the payment.\n{ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateReceipt()
        {
            ReceiptInvoiceIdText.Text = string.IsNullOrWhiteSpace(InvoiceIdTextBox.Text) ? "[Invoice ID]" : InvoiceIdTextBox.Text.Trim();
            ReceiptAmountText.Text = string.IsNullOrWhiteSpace(AmountTextBox.Text) ? "0.00" : AmountTextBox.Text.Trim();
            ReceiptMethodText.Text = PaymentMethodComboBox.SelectedItem is ComboBoxItem item ? item.Content?.ToString() : "[Payment method]";
            ReceiptPaidAtText.Text = PaidAtPicker.SelectedDate?.ToString("d") ?? "[Paid date]";
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

    public class PaymentRow
    {
        public int Id { get; set; }
        public string InvoiceId { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaidAt { get; set; }
        public string ReceivedBy { get; set; } = string.Empty;
    }
}
