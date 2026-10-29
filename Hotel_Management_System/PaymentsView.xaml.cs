using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Dashboard_WPF
{
    public partial class PaymentsView : UserControl
    {
        public ObservableCollection<PaymentRow> Payments { get; } = new();
        private PaymentRow? _editingPayment;
        private string _selectedPaymentMethod = "Cash";

        public PaymentsView()
        {
            InitializeComponent();
            DataContext = this;
            PaymentDatePicker.SelectedDate = DateTime.Today;
            StatusComboBox.SelectedIndex = 0;
            SelectPaymentMethod("Cash");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (InvoiceComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(GuestNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(BookingReferenceTextBox.Text) ||
                string.IsNullOrWhiteSpace(AmountTextBox.Text) ||
                PaymentDatePicker.SelectedDate == null ||
                StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please complete all down payment fields.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PaymentRow payment = new()
            {
                InvoiceNumber = ((ComboBoxItem)InvoiceComboBox.SelectedItem).Content.ToString() ?? string.Empty,
                GuestName = GuestNameTextBox.Text.Trim(),
                BookingReference = BookingReferenceTextBox.Text.Trim(),
                Amount = AmountTextBox.Text.Trim(),
                PaymentMethod = _selectedPaymentMethod,
                PaymentDate = PaymentDatePicker.SelectedDate.Value,
                Status = ((ComboBoxItem)StatusComboBox.SelectedItem).Content.ToString() ?? string.Empty,
                NameOnCard = NameOnCardTextBox.Text.Trim(),
                CardNumber = CardNumberTextBox.Text.Trim(),
                Expiry = ExpiryTextBox.Text.Trim(),
                Cvc = CvcTextBox.Text.Trim(),
                Notes = NotesTextBox.Text.Trim()
            };

            if (_editingPayment == null)
            {
                payment.Id = Payments.Count + 1;
                Payments.Add(payment);
            }
            else
            {
                payment.Id = _editingPayment.Id;
                Payments[Payments.IndexOf(_editingPayment)] = payment;
            }

            MessageBox.Show("Down payment saved successfully.", "Down Payment", MessageBoxButton.OK, MessageBoxImage.Information);
            ClearFields();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            _editingPayment = null;
            InvoiceComboBox.SelectedIndex = -1;
            GuestNameTextBox.Clear();
            BookingReferenceTextBox.Clear();
            AmountTextBox.Clear();
            PaymentDatePicker.SelectedDate = null;
            StatusComboBox.SelectedIndex = 0;
            NameOnCardTextBox.Clear();
            CardNumberTextBox.Clear();
            ExpiryTextBox.Clear();
            CvcTextBox.Clear();
            NotesTextBox.Clear();
            SelectPaymentMethod("Cash");
            UpdateSummary();
        }

        private void PaymentMethodButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string method)
                SelectPaymentMethod(method);
        }

        private void SelectPaymentMethod(string method)
        {
            _selectedPaymentMethod = method;
            CashButton.Background = method == "Cash" ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue) : null;
            CreditCardButton.Background = method == "Credit Card" ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue) : null;
            BankTransferButton.Background = method == "Bank Transfer" ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue) : null;
            QrPayButton.Background = method == "QR Pay" ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.LightBlue) : null;
            CardDetailsPanel.Visibility = method == "Credit Card" ? Visibility.Visible : Visibility.Collapsed;
            QrDetailsPanel.Visibility = method == "QR Pay" ? Visibility.Visible : Visibility.Collapsed;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not PaymentRow payment)
                return;

            _editingPayment = payment;
            SelectComboBoxItem(InvoiceComboBox, payment.InvoiceNumber);
            GuestNameTextBox.Text = payment.GuestName;
            BookingReferenceTextBox.Text = payment.BookingReference;
            AmountTextBox.Text = payment.Amount;
            SelectPaymentMethod(payment.PaymentMethod);
            PaymentDatePicker.SelectedDate = payment.PaymentDate;
            SelectComboBoxItem(StatusComboBox, payment.Status);
            NameOnCardTextBox.Text = payment.NameOnCard;
            CardNumberTextBox.Text = payment.CardNumber;
            ExpiryTextBox.Text = payment.Expiry;
            CvcTextBox.Text = payment.Cvc;
            NotesTextBox.Text = payment.Notes;
            UpdateSummary();
        }

        private void UpdateSummary()
        {
            SummaryInvoiceText.Text = InvoiceComboBox.SelectedItem is ComboBoxItem invoice ? invoice.Content?.ToString() : "[Invoice no.]";
            SummaryGuestText.Text = string.IsNullOrWhiteSpace(GuestNameTextBox.Text) ? "[Guest name]" : GuestNameTextBox.Text.Trim();
            SummaryPaymentText.Text = string.IsNullOrWhiteSpace(AmountTextBox.Text) ? "0.00" : AmountTextBox.Text.Trim();
            SummaryBalanceText.Text = SummaryPaymentText.Text;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not PaymentRow payment)
                return;

            if (MessageBox.Show("Delete this down payment?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            Payments.Remove(payment);
            ReassignIds();
            if (ReferenceEquals(_editingPayment, payment))
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
            for (int i = 0; i < Payments.Count; i++)
                Payments[i].Id = i + 1;
        }
    }

    public class PaymentRow
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string NameOnCard { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string Expiry { get; set; } = string.Empty;
        public string Cvc { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
