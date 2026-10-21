using System;
using System.Configuration;
using System.Data.SqlClient;

namespace EcoCashSimulation
{
    public class DatabaseManager
    {
        private readonly string _connectionString;
        private readonly string _masterConnectionString;
        private const string DatabaseName = "EcoCashDB";

        public DatabaseManager()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["EcoCashDB"]?.ConnectionString;
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ConfigurationErrorsException("Connection string 'EcoCashDB' not found in app.config");
            }

            SqlConnectionStringBuilder masterBuilder = new SqlConnectionStringBuilder(_connectionString)
            {
                InitialCatalog = "master"
            };
            _masterConnectionString = masterBuilder.ConnectionString;
        }

        public void InitializeDatabase()
        {
            try
            {
                // First, ensure the database exists
                EnsureDatabaseExists();

                // Then create tables
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    CreateTables(connection);
                    SeedDefaultAccounts(connection);
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Database initialization error: {ex.Message}");
                throw;
            }
        }

        private void EnsureDatabaseExists()
        {
            try
            {
                using (SqlConnection masterConnection = new SqlConnection(_masterConnectionString))
                {
                    masterConnection.Open();

                    string checkQuery = "SELECT database_id FROM sys.databases WHERE name = @DatabaseName";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, masterConnection))
                    {
                        checkCommand.Parameters.AddWithValue("@DatabaseName", DatabaseName);
                        var result = checkCommand.ExecuteScalar();
                        if (result == null)
                        {
                            string createQuery = "CREATE DATABASE [EcoCashDB]";
                            using (SqlCommand createCommand = new SqlCommand(createQuery, masterConnection))
                            {
                                createCommand.ExecuteNonQuery();
                                Logger.Log($"Database '{DatabaseName}' created successfully.");
                            }
                        }
                        else
                        {
                            Logger.Log($"Database '{DatabaseName}' already exists.");
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Logger.Log($"Error ensuring database exists: {ex.Message}");
                throw;
            }
        }

        private void CreateTables(SqlConnection connection)
        {
            string[] scripts = new string[]
            {
                // Accounts table
                @"IF OBJECT_ID('Accounts', 'U') IS NULL
                  BEGIN
                    CREATE TABLE Accounts (
                        AccountID INT IDENTITY(1,1) PRIMARY KEY,
                        PhoneNumber INT NOT NULL UNIQUE,
                        Name NVARCHAR(100) NOT NULL,
                        PIN NVARCHAR(4) NOT NULL,
                        Balance DECIMAL(10, 2) NOT NULL,
                        CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
                    )
                    PRINT 'Accounts table created successfully.'
                  END",

                // Transactions table
                @"IF OBJECT_ID('Transactions', 'U') IS NULL
                  BEGIN
                    CREATE TABLE Transactions (
                        TransactionID INT IDENTITY(1,1) PRIMARY KEY,
                        AccountID INT NOT NULL,
                        TransactionType NVARCHAR(50) NOT NULL,
                        Amount DECIMAL(10, 2) NOT NULL,
                        Reference NVARCHAR(50) NOT NULL,
                        TransactionDate DATETIME NOT NULL,
                        Status NVARCHAR(20) NOT NULL,
                        RecipientAccountID INT NULL,
                        FOREIGN KEY (AccountID) REFERENCES Accounts(AccountID),
                        FOREIGN KEY (RecipientAccountID) REFERENCES Accounts(AccountID)
                    )
                    PRINT 'Transactions table created successfully.'
                  END",

                // Create index on PhoneNumber for fast lookup
                @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Accounts_PhoneNumber')
                  BEGIN
                    CREATE NONCLUSTERED INDEX IX_Accounts_PhoneNumber ON Accounts(PhoneNumber)
                    PRINT 'Index IX_Accounts_PhoneNumber created successfully.'
                  END",

                // Create index on AccountID for transactions
                @"IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Transactions_AccountID')
                  BEGIN
                    CREATE NONCLUSTERED INDEX IX_Transactions_AccountID ON Transactions(AccountID)
                    PRINT 'Index IX_Transactions_AccountID created successfully.'
                  END"
            };

            foreach (var script in scripts)
            {
                using (SqlCommand command = new SqlCommand(script, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void SeedDefaultAccounts(SqlConnection connection)
        {
            string checkQuery = "SELECT COUNT(*) FROM Accounts";
            using (SqlCommand command = new SqlCommand(checkQuery, connection))
            {
                int count = (int)command.ExecuteScalar();
                if (count > 0)
                {
                    Logger.Log("Database already contains accounts. Skipping seed data.");
                    return; // Database already has data
                }
            }

            // Insert default accounts
            string insertQuery = @"INSERT INTO Accounts (PhoneNumber, Name, PIN, Balance)
                                   VALUES (@PhoneNumber, @Name, @PIN, @Balance)";

            var defaultAccounts = new[]
            {
                new { PhoneNumber = 61234567, Name = "Thato Motseki", PIN = "1234", Balance = 2500m },
                new { PhoneNumber = 62345678, Name = "Mpho Mokoena", PIN = "4321", Balance = 1800m },
                new { PhoneNumber = 63456789, Name = "Lerato Phiri", PIN = "1111", Balance = 3200m }
            };

            foreach (var account in defaultAccounts)
            {
                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", account.PhoneNumber);
                    command.Parameters.AddWithValue("@Name", account.Name);
                    command.Parameters.AddWithValue("@PIN", account.PIN);
                    command.Parameters.AddWithValue("@Balance", account.Balance);
                    command.ExecuteNonQuery();
                }
            }

            Logger.Log("Default accounts seeded successfully.");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public string ConnectionString => _connectionString;
    }
}
