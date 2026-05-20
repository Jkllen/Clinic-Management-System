using CruzNeryClinic.ViewModels;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CruzNeryClinic.Views
{
    public partial class InventoryView : UserControl
    {
        public InventoryView()
        {
            InitializeComponent();
            DataContext = new InventoryViewModel();

            DataObject.AddPastingHandler(StockTextBox,     NumericOnly_Pasting);
            DataObject.AddPastingHandler(UnitPriceTextBox, DecimalOnly_Pasting);
            DataObject.AddPastingHandler(ThresholdTextBox, NumericOnly_Pasting);
        }

        private void IntegerOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        private void DecimalOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = (TextBox)sender;
            bool isDigit = Regex.IsMatch(e.Text, @"^[0-9]+$");
            bool isDot   = e.Text == "." && !textBox.Text.Contains(".");
            e.Handled = !(isDigit || isDot);
        }

        private void NumericOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!Regex.IsMatch(text, @"^[0-9]+$"))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void DecimalOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!Regex.IsMatch(text, @"^[0-9]*\.?[0-9]*$"))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void RestockItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RestockItemDropdownBtn.IsChecked == true)
                RestockItemDropdownBtn.IsChecked = false;
        }

        private void UsageItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UsageItemDropdownBtn.IsChecked == true)
                UsageItemDropdownBtn.IsChecked = false;
        }

    }
}
