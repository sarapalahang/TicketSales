namespace TicketSales;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        var database = new TicketSalesDatabase();
        try
        {
            database.Initialize();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                $"اتصال به پایگاه داده انجام نشد.\nسرور: {database.ServerName}\nپایگاه داده: {database.DatabaseName}\n{exception.Message}\n\nاگر دیتابیس TicketSalesDb هنوز در SSMS ساخته نشده، ابتدا آن را بسازید.",
                "خطا در راه‌اندازی",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Application.Run(new Form1(database));
    }
}
