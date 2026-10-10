using System.Windows;
using System.Windows.Controls;

namespace GreenKeeper.Views
{
    /// <summary>
    /// Limits a text box to a whole number as written: digits only, no leading zero,
    /// no more digits than MaxDigits. Whatever else gets into the box, typed, pasted
    /// or dropped, is taken out at once. Unlike MaxLength this cleans before it
    /// counts, so a zero or a space in front cannot push a digit out of the field.
    /// </summary>
    public static class WholeNumberInput
    {
        public static readonly DependencyProperty MaxDigitsProperty = DependencyProperty.RegisterAttached(
            "MaxDigits",
            typeof(int),
            typeof(WholeNumberInput),
            new PropertyMetadata(0, OnMaxDigitsChanged));

        public static int GetMaxDigits(TextBox box)
        {
            return (int)box.GetValue(MaxDigitsProperty);
        }

        public static void SetMaxDigits(TextBox box, int maxDigits)
        {
            box.SetValue(MaxDigitsProperty, maxDigits);
        }

        // What the field keeps of a text.
        public static string Clean(string text, int maxDigits)
        {
            string number = new string(text.Where(char.IsAsciiDigit).ToArray()).TrimStart('0');

            return number.Length > maxDigits ? number[..maxDigits] : number;
        }

        private static void OnMaxDigitsChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            if (element is TextBox box)
            {
                box.TextChanged -= Box_TextChanged;
                box.TextChanged += Box_TextChanged;
            }
        }

        private static void Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = (TextBox)sender;
            int maxDigits = GetMaxDigits(box);
            string cleaned = Clean(box.Text, maxDigits);

            if (cleaned == box.Text)
            {
                return;
            }

            // The cursor stays behind the digits that were in front of it.
            int cursor = Clean(box.Text[..box.CaretIndex], maxDigits).Length;

            box.Text = cleaned;
            box.CaretIndex = cursor;
        }
    }
}
