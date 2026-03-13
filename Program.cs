using System.Globalization;
using System.Text;
using System.Windows.Forms;

ApplicationConfiguration.Initialize();
Application.Run(new CurrencyConverterForm());

internal sealed class CurrencyConverterForm : Form
{
    private readonly TextBox amountTextBox;
    private readonly ComboBox fromCurrencyComboBox;
    private readonly ComboBox toCurrencyComboBox;
    private readonly TextBox resultTextBox;

    private static readonly Currency[] Currencies =
    [
        new("THB", "Thai Baht", 1.00m),
        new("USD", "US Dollar", 35.80m),
        new("EUR", "Euro", 38.90m),
        new("GBP", "British Pound", 45.60m),
        new("JPY", "Japanese Yen", 0.24m),
        new("CNY", "Chinese Yuan", 4.95m),
        new("SGD", "Singapore Dollar", 26.50m)
    ];

    public CurrencyConverterForm()
    {
        Text = "โปรแกรมแปลงสกุลเงิน";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(640, 430);
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        var amountLabel = new Label
        {
            Text = "จำนวนเงิน",
            AutoSize = true,
            Location = new Point(30, 33)
        };

        amountTextBox = new TextBox
        {
            Location = new Point(240, 30),
            Size = new Size(250, 32)
        };

        var fromLabel = new Label
        {
            Text = "จากสกุลเงิน",
            AutoSize = true,
            Location = new Point(30, 83)
        };

        fromCurrencyComboBox = new ComboBox
        {
            Location = new Point(240, 80),
            Size = new Size(250, 32),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        var toLabel = new Label
        {
            Text = "เป็นสกุลเงิน",
            AutoSize = true,
            Location = new Point(30, 133)
        };

        toCurrencyComboBox = new ComboBox
        {
            Location = new Point(240, 130),
            Size = new Size(250, 32),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        resultTextBox = new TextBox
        {
            Location = new Point(30, 185),
            Size = new Size(560, 140),
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical
        };

        var convertButton = new Button
        {
            Text = "แปลงค่า",
            Location = new Point(180, 340),
            Size = new Size(110, 34)
        };
        convertButton.Click += (_, _) => ConvertCurrency();

        var resetButton = new Button
        {
            Text = "ล้างค่า",
            Location = new Point(320, 340),
            Size = new Size(110, 34)
        };
        resetButton.Click += (_, _) => ResetFields();

        foreach (var currency in Currencies)
        {
            var item = $"{currency.Code} - {currency.Name}";
            fromCurrencyComboBox.Items.Add(item);
            toCurrencyComboBox.Items.Add(item);
        }

        Controls.Add(amountLabel);
        Controls.Add(amountTextBox);
        Controls.Add(fromLabel);
        Controls.Add(fromCurrencyComboBox);
        Controls.Add(toLabel);
        Controls.Add(toCurrencyComboBox);
        Controls.Add(resultTextBox);
        Controls.Add(convertButton);
        Controls.Add(resetButton);

        ResetFields();
    }

    private void ResetFields()
    {
        amountTextBox.Clear();
        fromCurrencyComboBox.SelectedIndex = 0;
        toCurrencyComboBox.SelectedIndex = 1;
        resultTextBox.Text =
            "กรอกจำนวนเงิน เลือกสกุลเงินต้นทางและปลายทาง" + Environment.NewLine +
            "แล้วกดปุ่ม แปลงค่า";
    }

    private void ConvertCurrency()
    {
        if (!TryParseAmount(out var amount))
        {
            return;
        }

        if (fromCurrencyComboBox.SelectedIndex < 0 || toCurrencyComboBox.SelectedIndex < 0)
        {
            MessageBox.Show(this, "กรุณาเลือกสกุลเงินต้นทางและปลายทาง", "ข้อมูลไม่ถูกต้อง", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var fromCurrency = Currencies[fromCurrencyComboBox.SelectedIndex];
        var toCurrency = Currencies[toCurrencyComboBox.SelectedIndex];

        var amountInThb = amount * fromCurrency.RateToThb;
        var convertedAmount = amountInThb / toCurrency.RateToThb;
        var directRate = fromCurrency.RateToThb / toCurrency.RateToThb;

        var builder = new StringBuilder();
        builder.AppendLine($"จำนวนเงิน: {amount:F2} {fromCurrency.Code}");
        builder.AppendLine($"แปลงแล้ว: {convertedAmount:F2} {toCurrency.Code}");
        builder.AppendLine();
        builder.AppendLine($"อัตราแลกเปลี่ยน: 1 {fromCurrency.Code} = {directRate:F4} {toCurrency.Code}");
        builder.AppendLine($"อ้างอิง: 1 {fromCurrency.Code} = {fromCurrency.RateToThb:F2} THB");
        builder.AppendLine($"อ้างอิง: 1 {toCurrency.Code} = {toCurrency.RateToThb:F2} THB");
        builder.AppendLine();
        builder.Append("หมายเหตุ: อัตราแลกเปลี่ยนถูกกำหนดคงที่ในซอร์สโค้ดสำหรับการสาธิต");

        resultTextBox.Text = builder.ToString();
    }

    private bool TryParseAmount(out decimal amount)
    {
        if (string.IsNullOrWhiteSpace(amountTextBox.Text))
        {
            MessageBox.Show(this, "กรุณากรอกจำนวนเงิน", "ข้อมูลไม่ถูกต้อง", MessageBoxButtons.OK, MessageBoxIcon.Error);
            amount = 0m;
            return false;
        }

        if (!decimal.TryParse(amountTextBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount) &&
            !decimal.TryParse(amountTextBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out amount))
        {
            MessageBox.Show(this, "จำนวนเงินต้องเป็นตัวเลข", "ข้อมูลไม่ถูกต้อง", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (amount < 0m)
        {
            MessageBox.Show(this, "จำนวนเงินต้องมากกว่าหรือเท่ากับศูนย์", "ข้อมูลไม่ถูกต้อง", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        return true;
    }
}

internal sealed record Currency(string Code, string Name, decimal RateToThb);
