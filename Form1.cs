using System.ComponentModel;

namespace TicketSales;

public partial class Form1 : Form
{
    private static readonly Dictionary<string, decimal> EventPrices = new()
    {
        ["کنسرت موسیقی"] = 850_000m,
        ["نمایش تئاتر"] = 420_000m,
        ["مسابقه فوتبال"] = 300_000m
    };

    private readonly BindingList<TicketOrder> _orders = new();
    private readonly TicketSalesDatabase _database;
    private readonly ComboBox _eventSelector = new();
    private readonly ComboBox _ticketTypeSelector = new();
    private readonly DateTimePicker _showDate = new();
    private readonly NumericUpDown _quantity = new();
    private readonly Label _unitPriceValue = new();
    private readonly Label _estimatedTotalValue = new();
    private readonly Label _salesSummary = new();
    private readonly DataGridView _ordersGrid = new();

    public Form1(TicketSalesDatabase database)
    {
        _database = database;
        InitializeComponent();

        Text = "سامانه فروش بلیط";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1180, 760);
        MinimumSize = new Size(920, 640);
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        Font = new Font("Tahoma", 9F);
        BackColor = Color.FromArgb(244, 246, 250);

        BuildInterface();
        UpdatePricePreview();
        UpdateSalesSummary();
    }

    private void BuildInterface()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = BackColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(BuildHeader(), 0, 0);
        layout.Controls.Add(BuildMainArea(), 0, 1);
        Controls.Add(layout);
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(24, 8, 24, 8),
            BackColor = Color.FromArgb(30, 45, 72)
        };
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 38));

        header.Controls.Add(new Label
        {
            Text = "سامانهٔ فروش بلیط",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.White,
            Font = new Font("Tahoma", 19F, FontStyle.Bold)
        }, 0, 0);
        header.Controls.Add(new Label
        {
            Text = "ثبت سفارش، مشاهدهٔ سبد و محاسبهٔ مبلغ",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(205, 216, 233),
            Font = new Font("Tahoma", 9F)
        }, 0, 1);

        return header;
    }

    private Control BuildMainArea()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            RightToLeft = RightToLeft.Yes,
            Size = new Size(1180, 660),
            SplitterWidth = 6,
            BackColor = Color.FromArgb(225, 230, 238),
            Panel1MinSize = 300,
            Panel2MinSize = 500,
            SplitterDistance = 370
        };

        split.Panel1.Padding = new Padding(18);
        split.Panel1.BackColor = Color.FromArgb(244, 246, 250);
        split.Panel1.Controls.Add(BuildOrderEntry());

        split.Panel2.Padding = new Padding(18);
        split.Panel2.BackColor = Color.FromArgb(244, 246, 250);
        split.Panel2.Controls.Add(BuildOrdersArea());

        return split;
    }

    private Control BuildOrderEntry()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            BackColor = Color.White,
            Padding = new Padding(18),
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        panel.Controls.Add(new Label
        {
            Text = "اطلاعات سفارش",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Tahoma", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(37, 51, 75)
        }, 0, 0);

        ConfigureCombo(_eventSelector);
        _eventSelector.Items.AddRange(EventPrices.Keys.Cast<object>().ToArray());
        _eventSelector.SelectedIndex = 0;
        _eventSelector.SelectedIndexChanged += (_, _) => UpdatePricePreview();
        panel.Controls.Add(CreateField("رویداد", _eventSelector), 0, 1);

        _showDate.Format = DateTimePickerFormat.Custom;
        _showDate.CustomFormat = "yyyy/MM/dd";
        _showDate.Dock = DockStyle.Fill;
        _showDate.RightToLeft = RightToLeft.Yes;
        _showDate.RightToLeftLayout = true;
        panel.Controls.Add(CreateField("تاریخ اجرا", _showDate), 0, 2);

        ConfigureCombo(_ticketTypeSelector);
        _ticketTypeSelector.Items.AddRange(new object[] { "معمولی", "VIP" });
        _ticketTypeSelector.SelectedIndex = 0;
        _ticketTypeSelector.SelectedIndexChanged += (_, _) => UpdatePricePreview();
        panel.Controls.Add(CreateField("نوع بلیط", _ticketTypeSelector), 0, 3);

        _quantity.Minimum = 1;
        _quantity.Maximum = 20;
        _quantity.Value = 1;
        _quantity.Dock = DockStyle.Fill;
        _quantity.TextAlign = HorizontalAlignment.Right;
        _quantity.ValueChanged += (_, _) => UpdatePricePreview();
        panel.Controls.Add(CreateField("تعداد بلیط", _quantity), 0, 4);

        panel.Controls.Add(BuildPriceSummary(), 0, 5);

        var addButton = CreateButton("افزودن به سبد", Color.FromArgb(40, 125, 92), Color.White);
        addButton.Dock = DockStyle.Bottom;
        addButton.Click += (_, _) => AddOrder();
        panel.Controls.Add(addButton, 0, 6);

        return panel;
    }

    private Control BuildPriceSummary()
    {
        var summary = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.FromArgb(242, 246, 250),
            Padding = new Padding(10, 5, 10, 5),
            Margin = new Padding(0, 4, 0, 4)
        };
        summary.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        summary.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        _unitPriceValue.Dock = DockStyle.Fill;
        _unitPriceValue.TextAlign = ContentAlignment.MiddleRight;
        _unitPriceValue.ForeColor = Color.FromArgb(76, 89, 109);
        _estimatedTotalValue.Dock = DockStyle.Fill;
        _estimatedTotalValue.TextAlign = ContentAlignment.MiddleRight;
        _estimatedTotalValue.ForeColor = Color.FromArgb(30, 45, 72);
        _estimatedTotalValue.Font = new Font("Tahoma", 10F, FontStyle.Bold);

        summary.Controls.Add(_unitPriceValue, 0, 0);
        summary.Controls.Add(_estimatedTotalValue, 0, 1);
        return summary;
    }

    private Control BuildOrdersArea()
    {
        var area = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.White,
            Padding = new Padding(18)
        };
        area.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        area.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        area.RowStyles.Add(new RowStyle(SizeType.Absolute, 96));

        var title = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        title.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        title.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        title.Controls.Add(new Label
        {
            Text = "سبد فروش",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Tahoma", 12F, FontStyle.Bold),
            ForeColor = Color.FromArgb(37, 51, 75)
        }, 0, 0);
        title.Controls.Add(new Label
        {
            Text = "سفارش‌های ثبت‌شده در این نشست",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(115, 126, 144)
        }, 0, 1);
        area.Controls.Add(title, 0, 0);

        ConfigureOrdersGrid();
        area.Controls.Add(_ordersGrid, 0, 1);

        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 7, 0, 0)
        };
        footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _salesSummary.Dock = DockStyle.Fill;
        _salesSummary.TextAlign = ContentAlignment.MiddleRight;
        _salesSummary.Font = new Font("Tahoma", 10F, FontStyle.Bold);
        _salesSummary.ForeColor = Color.FromArgb(37, 51, 75);
        footer.Controls.Add(_salesSummary, 0, 0);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = Padding.Empty
        };

        var checkoutButton = CreateButton("ثبت فروش", Color.FromArgb(40, 125, 92), Color.White);
        checkoutButton.Width = 120;
        checkoutButton.Click += (_, _) => Checkout();
        var removeButton = CreateButton("حذف انتخاب‌شده", Color.FromArgb(232, 237, 244), Color.FromArgb(49, 63, 85));
        removeButton.Width = 145;
        removeButton.Click += (_, _) => RemoveSelectedOrder();
        var clearButton = CreateButton("خالی‌کردن سبد", Color.FromArgb(250, 235, 235), Color.FromArgb(155, 58, 58));
        clearButton.Width = 130;
        clearButton.Click += (_, _) => ClearOrders();
        var historyButton = CreateButton("تاریخچه فروش", Color.FromArgb(232, 237, 244), Color.FromArgb(49, 63, 85));
        historyButton.Width = 125;
        historyButton.Click += (_, _) => ShowSalesHistory();

        buttons.Controls.Add(checkoutButton);
        buttons.Controls.Add(removeButton);
        buttons.Controls.Add(clearButton);
        buttons.Controls.Add(historyButton);
        footer.Controls.Add(buttons, 0, 1);
        area.Controls.Add(footer, 0, 2);

        return area;
    }

    private void ConfigureOrdersGrid()
    {
        _ordersGrid.Dock = DockStyle.Fill;
        _ordersGrid.DataSource = _orders;
        _ordersGrid.AutoGenerateColumns = false;
        _ordersGrid.AllowUserToAddRows = false;
        _ordersGrid.AllowUserToDeleteRows = false;
        _ordersGrid.AllowUserToResizeRows = false;
        _ordersGrid.ReadOnly = true;
        _ordersGrid.MultiSelect = false;
        _ordersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _ordersGrid.RowHeadersVisible = false;
        _ordersGrid.RightToLeft = RightToLeft.Yes;
        _ordersGrid.BackgroundColor = Color.White;
        _ordersGrid.BorderStyle = BorderStyle.None;
        _ordersGrid.GridColor = Color.FromArgb(232, 236, 242);
        _ordersGrid.EnableHeadersVisualStyles = false;
        _ordersGrid.ColumnHeadersHeight = 42;
        _ordersGrid.RowTemplate.Height = 38;
        _ordersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _ordersGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 243, 248);
        _ordersGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(54, 68, 90);
        _ordersGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9F, FontStyle.Bold);
        _ordersGrid.DefaultCellStyle.Font = new Font("Tahoma", 9F);
        _ordersGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(222, 235, 248);
        _ordersGrid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(30, 45, 72);
        _ordersGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _ordersGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        _ordersGrid.Columns.Add(CreateGridColumn("رویداد", nameof(TicketOrder.EventName), 140));
        var dateColumn = CreateGridColumn("تاریخ اجرا", nameof(TicketOrder.ShowDate), 95);
        dateColumn.DefaultCellStyle.Format = "yyyy/MM/dd";
        _ordersGrid.Columns.Add(dateColumn);
        _ordersGrid.Columns.Add(CreateGridColumn("نوع", nameof(TicketOrder.TicketType), 70));
        _ordersGrid.Columns.Add(CreateGridColumn("تعداد", nameof(TicketOrder.Quantity), 55));
        var unitPriceColumn = CreateGridColumn("قیمت واحد (تومان)", nameof(TicketOrder.UnitPrice), 115);
        unitPriceColumn.DefaultCellStyle.Format = "N0";
        _ordersGrid.Columns.Add(unitPriceColumn);
        var amountColumn = CreateGridColumn("مبلغ (تومان)", nameof(TicketOrder.Amount), 115);
        amountColumn.DefaultCellStyle.Format = "N0";
        _ordersGrid.Columns.Add(amountColumn);
    }

    private static DataGridViewTextBoxColumn CreateGridColumn(string title, string propertyName, float fillWeight)
    {
        return new DataGridViewTextBoxColumn
        {
            HeaderText = title,
            DataPropertyName = propertyName,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
    }

    private static void ConfigureCombo(ComboBox combo)
    {
        combo.Dock = DockStyle.Fill;
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        combo.FlatStyle = FlatStyle.System;
        combo.RightToLeft = RightToLeft.Yes;
    }

    private static Control CreateField(string caption, Control editor)
    {
        var field = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };
        field.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        field.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        field.Controls.Add(new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(76, 89, 109)
        }, 0, 0);
        editor.Margin = Padding.Empty;
        field.Controls.Add(editor, 0, 1);
        return field;
    }

    private static Button CreateButton(string text, Color backColor, Color foreColor)
    {
        return new Button
        {
            Text = text,
            Height = 42,
            Margin = new Padding(5, 2, 5, 2),
            BackColor = backColor,
            ForeColor = foreColor,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand,
            Font = new Font("Tahoma", 9F, FontStyle.Bold),
            UseVisualStyleBackColor = false
        };
    }

    private decimal GetUnitPrice()
    {
        if (_eventSelector.SelectedItem is not string eventName || !EventPrices.TryGetValue(eventName, out var basePrice))
        {
            return 0;
        }

        return _ticketTypeSelector.SelectedItem?.ToString() == "VIP"
            ? basePrice * 1.8m
            : basePrice;
    }

    private void UpdatePricePreview()
    {
        var unitPrice = GetUnitPrice();
        var total = unitPrice * _quantity.Value;
        _unitPriceValue.Text = $"قیمت هر بلیط: {unitPrice:N0} تومان";
        _estimatedTotalValue.Text = $"مبلغ سفارش: {total:N0} تومان";
    }

    private void AddOrder()
    {
        if (_eventSelector.SelectedItem is not string eventName || _ticketTypeSelector.SelectedItem is null)
        {
            return;
        }

        var unitPrice = GetUnitPrice();
        _orders.Add(new TicketOrder
        {
            EventName = eventName,
            ShowDate = _showDate.Value.Date,
            TicketType = _ticketTypeSelector.SelectedItem.ToString() ?? "معمولی",
            Quantity = (int)_quantity.Value,
            UnitPrice = unitPrice,
            Amount = unitPrice * _quantity.Value
        });

        UpdateSalesSummary();
    }

    private void RemoveSelectedOrder()
    {
        if (_ordersGrid.CurrentRow?.DataBoundItem is TicketOrder selectedOrder)
        {
            _orders.Remove(selectedOrder);
            UpdateSalesSummary();
        }
    }

    private void ClearOrders()
    {
        if (_orders.Count == 0)
        {
            return;
        }

        var answer = MessageBox.Show(
            this,
            "همهٔ سفارش‌های سبد حذف شوند؟",
            "خالی‌کردن سبد",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer == DialogResult.Yes)
        {
            _orders.Clear();
            UpdateSalesSummary();
        }
    }

    private void Checkout()
    {
        if (_orders.Count == 0)
        {
            MessageBox.Show(this, "سبد فروش خالی است.", "ثبت فروش", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var ticketCount = _orders.Sum(order => order.Quantity);
        var total = _orders.Sum(order => order.Amount);
        try
        {
            var saleId = _database.SaveSale(_orders.ToList());
            MessageBox.Show(
                this,
                $"فروش با موفقیت در پایگاه داده ثبت شد.\n\nشماره فروش: {saleId}\nتعداد بلیط: {ticketCount:N0}\nمبلغ قابل پرداخت: {total:N0} تومان",
                "ثبت فروش",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            _orders.Clear();
            UpdateSalesSummary();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                $"فروش در پایگاه داده ذخیره نشد.\nسرور: {_database.ServerName}\nپایگاه داده: {_database.DatabaseName}\n{exception.Message}",
                "خطا در ذخیره‌سازی",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ShowSalesHistory()
    {
        try
        {
            var sales = _database.GetRecentSales();
            if (sales.Count == 0)
            {
                MessageBox.Show(this, "هنوز فروشی در پایگاه داده ثبت نشده است.", "تاریخچه فروش", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var historyForm = new Form
            {
                Text = "تاریخچه فروش",
                StartPosition = FormStartPosition.CenterParent,
                Size = new Size(760, 440),
                MinimumSize = new Size(620, 340),
                RightToLeft = RightToLeft.Yes,
                RightToLeftLayout = true,
                Font = Font,
                BackColor = Color.White,
                Padding = new Padding(14)
            };

            var historyGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                RightToLeft = RightToLeft.Yes,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(232, 236, 242),
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DataSource = sales
            };
            historyGrid.ColumnHeadersHeight = 40;
            historyGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(239, 243, 248);
            historyGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Tahoma", 9F, FontStyle.Bold);
            historyGrid.DefaultCellStyle.Font = new Font("Tahoma", 9F);
            historyGrid.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            historyGrid.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            historyGrid.Columns.Add(CreateGridColumn("شماره فروش", nameof(SaleSummary.SaleId), 65));
            var soldAtColumn = CreateGridColumn("تاریخ و ساعت", nameof(SaleSummary.SoldAt), 130);
            soldAtColumn.DefaultCellStyle.Format = "yyyy/MM/dd HH:mm";
            historyGrid.Columns.Add(soldAtColumn);
            historyGrid.Columns.Add(CreateGridColumn("تعداد بلیط", nameof(SaleSummary.TicketCount), 80));
            var totalColumn = CreateGridColumn("مبلغ کل (تومان)", nameof(SaleSummary.TotalAmount), 120);
            totalColumn.DefaultCellStyle.Format = "N0";
            historyGrid.Columns.Add(totalColumn);

            historyForm.Controls.Add(historyGrid);
            historyForm.ShowDialog(this);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                $"خواندن تاریخچه فروش ناموفق بود.\n{exception.Message}",
                "خطا در خواندن پایگاه داده",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void UpdateSalesSummary()
    {
        var ticketCount = _orders.Sum(order => order.Quantity);
        var total = _orders.Sum(order => order.Amount);
        _salesSummary.Text = $"تعداد بلیط‌ها: {ticketCount:N0}     جمع کل: {total:N0} تومان";
    }

}
