using System.Data;
using Microsoft.Data.SqlClient;

namespace TicketSales;

public sealed class TicketSalesDatabase
{
    private const string DefaultDatabaseName = "TicketSalesDb";
    private readonly string _connectionString;

    public string ServerName { get; }
    public string DatabaseName { get; }

    public TicketSalesDatabase()
    {
        ServerName = Environment.GetEnvironmentVariable("TICKETSALES_SQL_SERVER")?.Trim() is { Length: > 0 } configuredServer
            ? configuredServer
            : ".";
        DatabaseName = Environment.GetEnvironmentVariable("TICKETSALES_SQL_DATABASE")?.Trim() is { Length: > 0 } configuredDatabase
            ? configuredDatabase
            : DefaultDatabaseName;

        var builder = new SqlConnectionStringBuilder
        {
            DataSource = ServerName,
            InitialCatalog = DatabaseName,
            IntegratedSecurity = true,
            Encrypt = SqlConnectionEncryptOption.Mandatory,
            TrustServerCertificate = true,
            ConnectTimeout = 10,
            ApplicationName = "TicketSales"
        };

        _connectionString = builder.ConnectionString;
    }

    public void Initialize()
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            IF OBJECT_ID(N'dbo.Sales', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Sales
                (
                    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Sales PRIMARY KEY,
                    SoldAtUtc DATETIME2(0) NOT NULL,
                    TicketCount INT NOT NULL CHECK (TicketCount > 0),
                    TotalAmountToman BIGINT NOT NULL CHECK (TotalAmountToman >= 0)
                );
            END;

            IF OBJECT_ID(N'dbo.SaleItems', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.SaleItems
                (
                    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SaleItems PRIMARY KEY,
                    SaleId BIGINT NOT NULL,
                    EventName NVARCHAR(100) NOT NULL,
                    ShowDate DATE NOT NULL,
                    TicketType NVARCHAR(30) NOT NULL,
                    Quantity INT NOT NULL CHECK (Quantity > 0),
                    UnitPriceToman BIGINT NOT NULL CHECK (UnitPriceToman >= 0),
                    AmountToman BIGINT NOT NULL CHECK (AmountToman >= 0),
                    CONSTRAINT FK_SaleItems_Sales FOREIGN KEY (SaleId) REFERENCES dbo.Sales(Id)
                );

                CREATE INDEX IX_SaleItems_SaleId ON dbo.SaleItems(SaleId);
            END;
            """;
        command.ExecuteNonQuery();
    }

    public long SaveSale(IReadOnlyCollection<TicketOrder> orders)
    {
        if (orders.Count == 0)
        {
            throw new ArgumentException("A sale must contain at least one ticket order.", nameof(orders));
        }

        var ticketCount = orders.Sum(order => order.Quantity);
        var totalAmount = orders.Sum(order => order.Amount);

        using var connection = OpenConnection();
        using var transaction = connection.BeginTransaction();

        long saleId;
        using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO dbo.Sales (SoldAtUtc, TicketCount, TotalAmountToman)
                OUTPUT INSERTED.Id
                VALUES (@SoldAtUtc, @TicketCount, @TotalAmountToman);
                """;
            command.Parameters.Add("@SoldAtUtc", SqlDbType.DateTime2).Value = DateTime.UtcNow;
            command.Parameters.Add("@TicketCount", SqlDbType.Int).Value = ticketCount;
            command.Parameters.Add("@TotalAmountToman", SqlDbType.BigInt).Value = ToWholeToman(totalAmount);
            saleId = Convert.ToInt64(command.ExecuteScalar(), System.Globalization.CultureInfo.InvariantCulture);
        }

        foreach (var order in orders)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = """
                INSERT INTO dbo.SaleItems
                    (SaleId, EventName, ShowDate, TicketType, Quantity, UnitPriceToman, AmountToman)
                VALUES
                    (@SaleId, @EventName, @ShowDate, @TicketType, @Quantity, @UnitPriceToman, @AmountToman);
                """;
            command.Parameters.Add("@SaleId", SqlDbType.BigInt).Value = saleId;
            command.Parameters.Add("@EventName", SqlDbType.NVarChar, 100).Value = order.EventName;
            command.Parameters.Add("@ShowDate", SqlDbType.Date).Value = order.ShowDate.Date;
            command.Parameters.Add("@TicketType", SqlDbType.NVarChar, 30).Value = order.TicketType;
            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = order.Quantity;
            command.Parameters.Add("@UnitPriceToman", SqlDbType.BigInt).Value = ToWholeToman(order.UnitPrice);
            command.Parameters.Add("@AmountToman", SqlDbType.BigInt).Value = ToWholeToman(order.Amount);
            command.ExecuteNonQuery();
        }

        transaction.Commit();
        return saleId;
    }

    public List<SaleSummary> GetRecentSales(int limit = 100)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (@Limit) Id, SoldAtUtc, TicketCount, TotalAmountToman
            FROM dbo.Sales
            ORDER BY Id DESC;
            """;
        command.Parameters.Add("@Limit", SqlDbType.Int).Value = Math.Clamp(limit, 1, 1000);

        using var reader = command.ExecuteReader();
        var sales = new List<SaleSummary>();
        while (reader.Read())
        {
            var soldAtUtc = DateTime.SpecifyKind(reader.GetDateTime(1), DateTimeKind.Utc);
            sales.Add(new SaleSummary
            {
                SaleId = reader.GetInt64(0),
                SoldAt = soldAtUtc.ToLocalTime(),
                TicketCount = reader.GetInt32(2),
                TotalAmount = reader.GetInt64(3)
            });
        }

        return sales;
    }

    private SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static long ToWholeToman(decimal amount)
    {
        if (amount != decimal.Truncate(amount))
        {
            throw new InvalidOperationException("Ticket prices must be whole Toman amounts to save to the database.");
        }

        return decimal.ToInt64(amount);
    }
}
