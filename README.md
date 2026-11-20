# Ticket Sales Desktop App

A Persian-language Windows Forms demo for managing ticket orders and recording completed sales in Microsoft SQL Server.

## Features

- Select an event, show date, ticket type, and quantity.
- Preview the unit price and order total; VIP tickets use a sample price multiplier.
- Add orders to a cart, remove a selected order, or clear the cart.
- Save each completed sale and its ticket lines together in a SQL Server transaction.
- View the 100 most recent sales.

## Built with

- C# and Windows Forms on .NET 10
- Microsoft.Data.SqlClient
- Microsoft SQL Server

## Requirements

- Windows
- .NET 10 SDK
- A reachable SQL Server instance
- A database named `TicketSalesDb` (or the database name configured below)
- Windows account permissions to create the `Sales` and `SaleItems` tables and their index in that database

The application creates its tables when it starts. It does not create the database itself. In SSMS, connect to your SQL Server, create `TicketSalesDb`, then run the application.

## Configure and run

The default connection uses the local SQL Server instance (`.`), Windows Authentication, and the `TicketSalesDb` database. To use another server or database, set these environment variables in PowerShell before starting the app:

```powershell
$env:TICKETSALES_SQL_SERVER = "."
$env:TICKETSALES_SQL_DATABASE = "TicketSalesDb"
dotnet run --project .\TicketSales.csproj
```

Open `TicketSales.slnx` in Visual Studio, or run the command above from this folder. The database connection currently trusts the SQL Server certificate for local development; configure certificate validation before using this sample in a production environment.

## Project structure

- `Form1.cs` builds the Persian user interface and handles cart and checkout actions.
- `TicketSalesDatabase.cs` creates the tables, saves sales transactionally, and reads recent sales.
- `TicketOrder.cs` contains the order and sale summary models.
- `Program.cs` initializes the database connection and starts the form.

## Scope

This is a learning and portfolio project with sample events and prices. It demonstrates a desktop UI and basic relational persistence. It does not implement seat inventory, user accounts, payment processing, or production operations.

---

# برنامهٔ دسکتاپ فروش بلیط

یک نمونهٔ آموزشی با رابط فارسی برای ثبت سفارش بلیط و ذخیرهٔ فروش‌های نهایی در Microsoft SQL Server.

## امکانات

- انتخاب رویداد، تاریخ اجرا، نوع بلیط و تعداد
- پیش‌نمایش قیمت واحد و مبلغ سفارش؛ قیمت VIP با ضریب نمونه محاسبه می‌شود
- افزودن سفارش به سبد، حذف سفارش انتخاب‌شده یا خالی‌کردن سبد
- ذخیرهٔ فروش و ردیف‌های بلیط در یک تراکنش SQL Server
- مشاهدهٔ ۱۰۰ فروش اخیر

## فناوری‌ها

- سی‌شارپ و Windows Forms روی .NET 10
- بستهٔ Microsoft.Data.SqlClient
- Microsoft SQL Server

## پیش‌نیاز و راه‌اندازی

به ویندوز، .NET 10 SDK، یک نمونهٔ SQL Server در دسترس و دسترسی Windows به دیتابیس نیاز دارید. در SSMS دیتابیس `TicketSalesDb` را بسازید؛ برنامه هنگام اجرا جدول‌های `Sales` و `SaleItems` و ایندکس را ایجاد می‌کند. حساب ویندوز باید اجازهٔ ساخت جدول و ایندکس در دیتابیس را داشته باشد.

اتصال پیش‌فرض از سرور محلی (`.`)، Windows Authentication و دیتابیس `TicketSalesDb` استفاده می‌کند. برای تنظیم سرور یا دیتابیس دیگر، متغیرهای محیطی `TICKETSALES_SQL_SERVER` و `TICKETSALES_SQL_DATABASE` را مطابق بخش انگلیسی تنظیم کنید. سپس فایل `TicketSales.slnx` را در Visual Studio باز کنید یا دستور اجرا را از پوشهٔ پروژه بزنید.

این پروژه نمونهٔ آموزشی و رزومه‌ای است؛ رویداد و قیمت‌ها نمونه‌اند و امکاناتی مانند مدیریت ظرفیت صندلی، حساب کاربری و پرداخت آنلاین را پیاده‌سازی نمی‌کند.
