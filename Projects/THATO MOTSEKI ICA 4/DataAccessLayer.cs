using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace EcoCashSimulation
{
    public class DataAccessLayer
    {
        private readonly DatabaseManager _dbManager;

        public DataAccessLayer(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        public Dictionary<int, Account> LoadAllAccounts()
        {
            Dictionary<int, Account> accounts = new Dictionary<int, Account>();

            string query = @"SELECT AccountID, PhoneNumber, Name, PIN, Balance 
                           FROM Accounts 
                           ORDER BY PhoneNumber";

            using (SqlConnection connection = _dbManager.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int accountId = reader.GetInt32(0);
                            int phoneNumber = reader.GetInt32(1);
                            string name = reader.GetString(2);
                            string pin = reader.GetString(3);
                            decimal balance = reader.GetDecimal(4);

                            Account account = new Account(phoneNumber, name, pin, balance)
                            {
                                DatabaseId = accountId
                            };

                            accounts.Add(phoneNumber, account);
                        }
                    }
                }

                foreach (Account account in accounts.Values)
                {
                    LoadTransactionsForAccount(account, connection);
                }
            }

            return accounts;
        }

        private void LoadTransactionsForAccount(Account account, SqlConnection connection)
        {
            string query = @"SELECT TransactionID, TransactionType, Amount, Reference, TransactionDate, Status 
                           FROM Transactions 
                           WHERE AccountID = @AccountID 
                           ORDER BY TransactionDate DESC";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@AccountID", account.DatabaseId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int transactionId = reader.GetInt32(0);
                        string type = reader.GetString(1);
                        decimal amount = reader.GetDecimal(2);
                        string reference = reader.GetString(3);
                        DateTime date = reader.GetDateTime(4);
                        string status = reader.GetString(5);

                        Transaction transaction = new GenericTransaction(account, type, amount, reference, date, status)
                        {
                            DatabaseId = transactionId
                        };

                        account.AddTransaction(transaction);
                    }
                }
            }
        }

        public int SaveAccount(Account account)
        {
            if (account.DatabaseId == 0)
            {
                return InsertAccount(account);
            }
            else
            {
                return UpdateAccount(account);
            }
        }

        private int InsertAccount(Account account)
        {
            string query = @"INSERT INTO Accounts (PhoneNumber, Name, PIN, Balance)
                           VALUES (@PhoneNumber, @Name, @PIN, @Balance);
                           SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = _dbManager.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", account.PhoneNumber);
                    command.Parameters.AddWithValue("@Name", account.Name);
                    command.Parameters.AddWithValue("@PIN", account.GetPin());
                    command.Parameters.AddWithValue("@Balance", account.Balance);

                    int newId = (int)(decimal)command.ExecuteScalar();
                    account.DatabaseId = newId;
                    return newId;
                }
            }
        }

        private int UpdateAccount(Account account)
        {
            string query = @"UPDATE Accounts 
                           SET Balance = @Balance
                           WHERE AccountID = @AccountID";

            using (SqlConnection connection = _dbManager.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Balance", account.Balance);
                    command.Parameters.AddWithValue("@AccountID", account.DatabaseId);
                    command.ExecuteNonQuery();
                    return account.DatabaseId;
                }
            }
        }

        public void SaveTransaction(Account account, Transaction transaction)
        {
            if (transaction.DatabaseId != 0)
            {
                return; // Already saved
            }

            string query = @"INSERT INTO Transactions (AccountID, TransactionType, Amount, Reference, TransactionDate, Status)
                           VALUES (@AccountID, @TransactionType, @Amount, @Reference, @TransactionDate, @Status);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection connection = _dbManager.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AccountID", account.DatabaseId);
                    command.Parameters.AddWithValue("@TransactionType", transaction.TransactionType);
                    command.Parameters.AddWithValue("@Amount", transaction.Amount);
                    command.Parameters.AddWithValue("@Reference", transaction.Reference);
                    command.Parameters.AddWithValue("@TransactionDate", transaction.TransactionDate);
                    command.Parameters.AddWithValue("@Status", transaction.Status);

                    transaction.DatabaseId = (int)command.ExecuteScalar();
                }
            }
        }

        public void ExportDatabase(string filePath)
        {
            using (SqlConnection connection = _dbManager.GetConnection())
            {
                connection.Open();

                using (StreamWriter writer = new StreamWriter(filePath, false, new UTF8Encoding(false)))
                {
                    writer.WriteLine("IF DB_ID(N'EcoCashDB') IS NULL CREATE DATABASE [EcoCashDB];");
                    writer.WriteLine("GO");
                    writer.WriteLine("USE [EcoCashDB];");
                    writer.WriteLine("GO");
                    writer.WriteLine();
                    writer.WriteLine(@"IF OBJECT_ID(N'dbo.Accounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Accounts (
        AccountID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        PhoneNumber INT NOT NULL UNIQUE,
        Name NVARCHAR(100) NOT NULL,
        PIN NVARCHAR(4) NOT NULL,
        Balance DECIMAL(10, 2) NOT NULL,
        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
    );
END;
GO
IF OBJECT_ID(N'dbo.Transactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Transactions (
        TransactionID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        AccountID INT NOT NULL,
        TransactionType NVARCHAR(50) NOT NULL,
        Amount DECIMAL(10, 2) NOT NULL,
        Reference NVARCHAR(50) NOT NULL,
        TransactionDate DATETIME NOT NULL,
        Status NVARCHAR(20) NOT NULL,
        RecipientAccountID INT NULL,
        FOREIGN KEY (AccountID) REFERENCES dbo.Accounts(AccountID),
        FOREIGN KEY (RecipientAccountID) REFERENCES dbo.Accounts(AccountID)
    );
END;
GO");

                    writer.WriteLine("SET IDENTITY_INSERT dbo.Accounts ON;");
                    using (SqlCommand command = new SqlCommand(
                        @"SELECT AccountID, PhoneNumber, Name, PIN, Balance, CreatedDate
                          FROM dbo.Accounts ORDER BY AccountID;", connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            writer.WriteLine(
                                "IF NOT EXISTS (SELECT 1 FROM dbo.Accounts WHERE AccountID = {0}) INSERT INTO dbo.Accounts (AccountID, PhoneNumber, Name, PIN, Balance, CreatedDate) VALUES ({0}, {1}, {2}, {3}, {4}, {5});",
                                reader.GetInt32(0),
                                reader.GetInt32(1),
                                SqlString(reader.GetString(2)),
                                SqlString(reader.GetString(3)),
                                SqlDecimal(reader.GetDecimal(4)),
                                SqlDateTime(reader.GetDateTime(5)));
                        }
                    }
                    writer.WriteLine("SET IDENTITY_INSERT dbo.Accounts OFF;");
                    writer.WriteLine("GO");
                    writer.WriteLine();

                    writer.WriteLine("SET IDENTITY_INSERT dbo.Transactions ON;");
                    using (SqlCommand command = new SqlCommand(
                        @"SELECT TransactionID, AccountID, TransactionType, Amount, Reference,
                                 TransactionDate, Status, RecipientAccountID
                          FROM dbo.Transactions ORDER BY TransactionID;", connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string recipientAccountId = reader.IsDBNull(7)
                                ? "NULL"
                                : reader.GetInt32(7).ToString(CultureInfo.InvariantCulture);

                            writer.WriteLine(
                                "IF NOT EXISTS (SELECT 1 FROM dbo.Transactions WHERE TransactionID = {0}) INSERT INTO dbo.Transactions (TransactionID, AccountID, TransactionType, Amount, Reference, TransactionDate, Status, RecipientAccountID) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7});",
                                reader.GetInt32(0),
                                reader.GetInt32(1),
                                SqlString(reader.GetString(2)),
                                SqlDecimal(reader.GetDecimal(3)),
                                SqlString(reader.GetString(4)),
                                SqlDateTime(reader.GetDateTime(5)),
                                SqlString(reader.GetString(6)),
                                recipientAccountId);
                        }
                    }
                    writer.WriteLine("SET IDENTITY_INSERT dbo.Transactions OFF;");
                    writer.WriteLine("GO");
                    writer.WriteLine();
                    writer.WriteLine(@"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Accounts_PhoneNumber' AND object_id = OBJECT_ID(N'dbo.Accounts'))
    CREATE NONCLUSTERED INDEX IX_Accounts_PhoneNumber ON dbo.Accounts(PhoneNumber);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Transactions_AccountID' AND object_id = OBJECT_ID(N'dbo.Transactions'))
    CREATE NONCLUSTERED INDEX IX_Transactions_AccountID ON dbo.Transactions(AccountID);
GO");
                }
            }
        }

        private static string SqlString(string value)
        {
            return "N'" + value.Replace("'", "''") + "'";
        }

        private static string SqlDecimal(decimal value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string SqlDateTime(DateTime value)
        {
            return "CONVERT(DATETIME, '" + value.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) + "', 126)";
        }

        public Account GetAccountByPhoneNumber(int phoneNumber)
        {
            string query = @"SELECT AccountID, PhoneNumber, Name, PIN, Balance 
                           FROM Accounts 
                           WHERE PhoneNumber = @PhoneNumber";

            using (SqlConnection connection = _dbManager.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int accountId = reader.GetInt32(0);
                            string name = reader.GetString(2);
                            string pin = reader.GetString(3);
                            decimal balance = reader.GetDecimal(4);

                            Account account = new Account(phoneNumber, name, pin, balance)
                            {
                                DatabaseId = accountId
                            };

                            connection.Close();

                            // Load transactions for this account
                            using (SqlConnection connForTrans = _dbManager.GetConnection())
                            {
                                connForTrans.Open();
                                LoadTransactionsForAccount(account, connForTrans);
                            }

                            return account;
                        }
                    }
                }
            }

            return null;
        }

        public bool AccountExists(int phoneNumber)
        {
            string query = "SELECT COUNT(*) FROM Accounts WHERE PhoneNumber = @PhoneNumber";

            using (SqlConnection connection = _dbManager.GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }
}
