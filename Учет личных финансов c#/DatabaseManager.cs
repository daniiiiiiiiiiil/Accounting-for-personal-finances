using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Учет_личных_финансов_c_;

public class DatabaseManager
{
    private string connectionString;

    public DatabaseManager(string dbPath = "finance.db")
    {
        connectionString = $"Data Source={dbPath};Version=3;";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();

            // Таблица транзакций
            string sql = @"CREATE TABLE IF NOT EXISTS Transactions (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Amount DECIMAL NOT NULL,
                            Description TEXT,
                            Type TEXT NOT NULL,
                            Category TEXT NOT NULL,
                            PaymentMethod TEXT NOT NULL,
                            Date DATETIME NOT NULL,
                            Currency TEXT DEFAULT 'RUB'
                           )";
            new SQLiteCommand(sql, conn).ExecuteNonQuery();

            sql = @"CREATE TABLE IF NOT EXISTS Budgets (
                 Category TEXT PRIMARY KEY,
                 ""Limit"" NUMERIC(10, 2) NOT NULL
               )";
            new SQLiteCommand(sql, conn).ExecuteNonQuery();


            // Таблица регулярных платежей
            sql = @"CREATE TABLE IF NOT EXISTS RecurringPayments (
                     Id INTEGER PRIMARY KEY AUTOINCREMENT,
                     Description TEXT,
                     Amount DECIMAL NOT NULL,
                     Category TEXT NOT NULL,
                     Frequency TEXT NOT NULL,
                     NextPaymentDate DATETIME NOT NULL
                   )";
            new SQLiteCommand(sql, conn).ExecuteNonQuery();

            // Таблица долгов
            sql = @"CREATE TABLE IF NOT EXISTS Debts (
                     Id INTEGER PRIMARY KEY AUTOINCREMENT,
                     Person TEXT NOT NULL,
                     Amount DECIMAL NOT NULL,
                     IsOwedToMe BOOLEAN NOT NULL,
                     DueDate DATETIME NOT NULL,
                     Description TEXT,
                     IsSettled BOOLEAN DEFAULT 0
                   )";
            new SQLiteCommand(sql, conn).ExecuteNonQuery();
        }
    }

    #region Transaction Methods
    public void AddTransaction(Transaction transaction)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = @"INSERT INTO Transactions 
                         (Amount, Description, Type, Category, PaymentMethod, Date, Currency)
                         VALUES (@amount, @desc, @type, @category, @method, @date, @currency)";

            var cmd = new SQLiteCommand(sql, conn);
            AddTransactionParameters(cmd, transaction);
            cmd.ExecuteNonQuery();
        }
    }

    public void UpdateTransaction(Transaction transaction)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = @"UPDATE Transactions SET 
                          Amount = @amount,
                          Description = @desc,
                          Type = @type,
                          Category = @category,
                          PaymentMethod = @method,
                          Date = @date,
                          Currency = @currency
                          WHERE Id = @id";

            var cmd = new SQLiteCommand(sql, conn);
            AddTransactionParameters(cmd, transaction);
            cmd.Parameters.AddWithValue("@id", transaction.Id);
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteTransaction(int id)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "DELETE FROM Transactions WHERE Id = @id";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    public List<Transaction> GetAllTransactions()
    {
        var transactions = new List<Transaction>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM Transactions ORDER BY Date DESC";
            var cmd = new SQLiteCommand(sql, conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    transactions.Add(ReadTransaction(reader));
                }
            }
        }
        return transactions;
    }

    public List<Transaction> GetTransactionsByDateRange(DateTime start, DateTime end)
    {
        var transactions = new List<Transaction>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM Transactions WHERE Date BETWEEN @start AND @end ORDER BY Date DESC";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@start", start);
            cmd.Parameters.AddWithValue("@end", end);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    transactions.Add(ReadTransaction(reader));
                }
            }
        }
        return transactions;
    }

    public List<Transaction> GetTransactionsByCategory(string category)
    {
        var transactions = new List<Transaction>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM Transactions WHERE Category = @category ORDER BY Date DESC";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@category", category);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    transactions.Add(ReadTransaction(reader));
                }
            }
        }
        return transactions;
    }

    public decimal GetCurrentBalance()
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT SUM(CASE WHEN Type = 'Доход' THEN Amount ELSE -Amount END) FROM Transactions";
            var cmd = new SQLiteCommand(sql, conn);
            var result = cmd.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }
    }

    public decimal GetTotalIncome()
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT SUM(Amount) FROM Transactions WHERE Type = 'Доход'";
            var cmd = new SQLiteCommand(sql, conn);
            var result = cmd.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }
    }

    public decimal GetTotalExpenses()
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT SUM(Amount) FROM Transactions WHERE Type = 'Расход'";
            var cmd = new SQLiteCommand(sql, conn);
            var result = cmd.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }
    }

    private void AddTransactionParameters(SQLiteCommand cmd, Transaction transaction)
    {
        cmd.Parameters.AddWithValue("@amount", transaction.Amount);
        cmd.Parameters.AddWithValue("@desc", transaction.Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@type", transaction.Type);
        cmd.Parameters.AddWithValue("@category", transaction.Category);
        cmd.Parameters.AddWithValue("@method", transaction.PaymentMethod);
        cmd.Parameters.AddWithValue("@date", transaction.Date);
        cmd.Parameters.AddWithValue("@currency", transaction.Currency ?? "RUB");
    }

    private Transaction ReadTransaction(SQLiteDataReader reader)
    {
        return new Transaction
        {
            Id = Convert.ToInt32(reader["Id"]),
            Amount = Convert.ToDecimal(reader["Amount"]),
            Description = reader["Description"] != DBNull.Value ? Convert.ToString(reader["Description"]) : null,
            Type = Convert.ToString(reader["Type"]),
            Category = Convert.ToString(reader["Category"]),
            PaymentMethod = Convert.ToString(reader["PaymentMethod"]),
            Date = Convert.ToDateTime(reader["Date"]),
            Currency = Convert.ToString(reader["Currency"])
        };
    }

    public void SetBudget(string category, decimal limit)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = @"INSERT OR REPLACE INTO Budgets (Category, ""Limit"")
                             VALUES (@category, @limit)";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@category", category);
            cmd.Parameters.AddWithValue("@limit", limit);
            cmd.ExecuteNonQuery();
        }
    }

    public decimal? GetBudget(string category)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT Limit FROM Budgets WHERE Category = @category";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@category", category);
            var result = cmd.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToDecimal(result) : (decimal?)null;
        }
    }

    public Dictionary<string, decimal> GetAllBudgets()
    {
        var budgets = new Dictionary<string, decimal>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT Category, \"Limit\" FROM Budgets";
            var cmd = new SQLiteCommand(sql, conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string category = Convert.ToString(reader["Category"]);
                    decimal limit = Convert.ToDecimal(reader["Limit"]);
                    budgets[category] = limit;
                }
            }
        }
        return budgets;
    }


    public void DeleteBudget(string category)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "DELETE FROM Budgets WHERE Category = @category";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@category", category);
            cmd.ExecuteNonQuery();
        }
    }

    public decimal GetCategorySpending(string category, DateTime startDate, DateTime endDate)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = @"SELECT SUM(Amount) FROM Transactions 
                         WHERE Category = @category AND Type = 'Расход' 
                         AND Date BETWEEN @start AND @end";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@category", category);
            cmd.Parameters.AddWithValue("@start", startDate);
            cmd.Parameters.AddWithValue("@end", endDate);
            var result = cmd.ExecuteScalar();
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
        }
    }

    
    public void AddRecurringPayment(RecurringPayment payment)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = @"INSERT INTO RecurringPayments 
                         (Description, Amount, Category, Frequency, NextPaymentDate)
                         VALUES (@desc, @amount, @category, @freq, @nextDate)";
            var cmd = new SQLiteCommand(sql, conn);
            AddRecurringPaymentParameters(cmd, payment);
            cmd.ExecuteNonQuery();
        }
    }

    public void UpdateRecurringPayment(RecurringPayment payment)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = @"UPDATE RecurringPayments SET 
                          Description = @desc,
                          Amount = @amount,
                          Category = @category,
                          Frequency = @freq,
                          NextPaymentDate = @nextDate
                          WHERE Id = @id";
            var cmd = new SQLiteCommand(sql, conn);
            AddRecurringPaymentParameters(cmd, payment);
            cmd.Parameters.AddWithValue("@id", payment.Id);
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteRecurringPayment(int id)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "DELETE FROM RecurringPayments WHERE Id = @id";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    public RecurringPayment GetRecurringPayment(int id)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM RecurringPayments WHERE Id = @id";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return ReadRecurringPayment(reader);
                }
            }
        }
        return null;
    }

    public List<RecurringPayment> GetAllRecurringPayments()
    {
        var payments = new List<RecurringPayment>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM RecurringPayments ORDER BY NextPaymentDate";
            var cmd = new SQLiteCommand(sql, conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    payments.Add(ReadRecurringPayment(reader));
                }
            }
        }
        return payments;
    }

    public List<RecurringPayment> GetDueRecurringPayments(DateTime toDate)
    {
        var payments = new List<RecurringPayment>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM RecurringPayments WHERE NextPaymentDate <= @toDate ORDER BY NextPaymentDate";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@toDate", toDate);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    payments.Add(ReadRecurringPayment(reader));
                }
            }
        }
        return payments;
    }

    public void UpdateNextPaymentDate(int id, DateTime newDate)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "UPDATE RecurringPayments SET NextPaymentDate = @newDate WHERE Id = @id";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@newDate", newDate);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    private void AddRecurringPaymentParameters(SQLiteCommand cmd, RecurringPayment payment)
    {
        cmd.Parameters.AddWithValue("@desc", payment.Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@amount", payment.Amount);
        cmd.Parameters.AddWithValue("@category", payment.Category);
        cmd.Parameters.AddWithValue("@freq", payment.Frequency);
        cmd.Parameters.AddWithValue("@nextDate", payment.NextPaymentDate);
    }

    private RecurringPayment ReadRecurringPayment(SQLiteDataReader reader)
    {
        return new RecurringPayment
        {
            Id = Convert.ToInt32(reader["Id"]),
            Description = reader["Description"] != DBNull.Value ? Convert.ToString(reader["Description"]) : null,
            Amount = Convert.ToDecimal(reader["Amount"]),
            Category = Convert.ToString(reader["Category"]),
            Frequency = Convert.ToString(reader["Frequency"]),
            NextPaymentDate = Convert.ToDateTime(reader["NextPaymentDate"])
        };
    }


    public void AddDebt(DebtRecord debt)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = @"INSERT INTO Debts 
                         (Person, Amount, IsOwedToMe, DueDate, Description)
                         VALUES (@person, @amount, @owed, @dueDate, @desc)";
            var cmd = new SQLiteCommand(sql, conn);
            AddDebtParameters(cmd, debt);
            cmd.ExecuteNonQuery();
        }
    }

    public void UpdateDebt(DebtRecord debt)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = @"UPDATE Debts SET 
                          Person = @person,
                          Amount = @amount,
                          IsOwedToMe = @owed,
                          DueDate = @dueDate,
                          Description = @desc,
                          IsSettled = @settled
                          WHERE Id = @id";
            var cmd = new SQLiteCommand(sql, conn);
            AddDebtParameters(cmd, debt);
            cmd.Parameters.AddWithValue("@id", debt.Id);
            cmd.Parameters.AddWithValue("@settled", debt.IsSettled);
            cmd.ExecuteNonQuery();
        }
    }

    public void DeleteDebt(int id)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "DELETE FROM Debts WHERE Id = @id";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    public DebtRecord GetDebt(int id)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM Debts WHERE Id = @id";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);

            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return ReadDebtRecord(reader);
                }
            }
        }
        return null;
    }

    public List<DebtRecord> GetAllDebts()
    {
        var debts = new List<DebtRecord>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM Debts ORDER BY DueDate";
            var cmd = new SQLiteCommand(sql, conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    debts.Add(ReadDebtRecord(reader));
                }
            }
        }
        return debts;
    }

    public List<DebtRecord> GetActiveDebts()
    {
        var debts = new List<DebtRecord>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM Debts WHERE IsSettled = 0 ORDER BY DueDate";
            var cmd = new SQLiteCommand(sql, conn);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    debts.Add(ReadDebtRecord(reader));
                }
            }
        }
        return debts;
    }

    public List<DebtRecord> GetDebtsByPerson(string person)
    {
        var debts = new List<DebtRecord>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM Debts WHERE Person = @person ORDER BY DueDate";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@person", person);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    debts.Add(ReadDebtRecord(reader));
                }
            }
        }
        return debts;
    }

    public void MarkDebtAsSettled(int id)
    {
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "UPDATE Debts SET IsSettled = 1 WHERE Id = @id";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    public Dictionary<string, decimal> GetDebtsSummary()
    {
        var summary = new Dictionary<string, decimal>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();

            // Долги, которые должны мне
            string sql = "SELECT SUM(Amount) FROM Debts WHERE IsOwedToMe = 1 AND IsSettled = 0";
            var cmd = new SQLiteCommand(sql, conn);
            var owedToMe = cmd.ExecuteScalar();
            summary.Add("Должны мне", owedToMe != DBNull.Value ? Convert.ToDecimal(owedToMe) : 0);

            // Долги, которые должен я
            sql = "SELECT SUM(Amount) FROM Debts WHERE IsOwedToMe = 0 AND IsSettled = 0";
            cmd = new SQLiteCommand(sql, conn);
            var iOwe = cmd.ExecuteScalar();
            summary.Add("Я должен", iOwe != DBNull.Value ? Convert.ToDecimal(iOwe) : 0);

            // Чистый баланс
            summary.Add("Чистый баланс", summary["Должны мне"] - summary["Я должен"]);
        }
        return summary;
    }

    private void AddDebtParameters(SQLiteCommand cmd, DebtRecord debt)
    {
        cmd.Parameters.AddWithValue("@person", debt.Person);
        cmd.Parameters.AddWithValue("@amount", debt.Amount);
        cmd.Parameters.AddWithValue("@owed", debt.IsOwedToMe);
        cmd.Parameters.AddWithValue("@dueDate", debt.DueDate);
        cmd.Parameters.AddWithValue("@desc", debt.Description ?? (object)DBNull.Value);
    }

    private DebtRecord ReadDebtRecord(SQLiteDataReader reader)
    {
        return new DebtRecord
        {
            Id = Convert.ToInt32(reader["Id"]),
            Person = Convert.ToString(reader["Person"]),
            Amount = Convert.ToDecimal(reader["Amount"]),
            IsOwedToMe = Convert.ToBoolean(reader["IsOwedToMe"]),
            DueDate = Convert.ToDateTime(reader["DueDate"]),
            Description = reader["Description"] != DBNull.Value ? Convert.ToString(reader["Description"]) : null,
            IsSettled = Convert.ToBoolean(reader["IsSettled"])
        };
    }

    public List<Transaction> GetRecentTransactions(int count)
    {
        var transactions = new List<Transaction>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = "SELECT * FROM Transactions ORDER BY Date DESC LIMIT @count";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@count", count);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    transactions.Add(ReadTransaction(reader));
                }
            }
        }
        return transactions;
    }

    public Dictionary<string, decimal> GetCategoryTotals(DateTime startDate, DateTime endDate)
    {
        var totals = new Dictionary<string, decimal>();
        using (var conn = new SQLiteConnection(connectionString))
        {
            conn.Open();
            string sql = @"SELECT Category, SUM(Amount) as Total 
                         FROM Transactions 
                         WHERE Date BETWEEN @start AND @end
                         GROUP BY Category";
            var cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@start", startDate);
            cmd.Parameters.AddWithValue("@end", endDate);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    totals.Add(Convert.ToString(reader["Category"]),
                              Convert.ToDecimal(reader["Total"]));
                }
            }
        }
        return totals;
    }
}
#endregion