using System;
using System.Collections.Generic;
using System.Data.SqlClient;

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

                            // Load transactions for this account
                            LoadTransactionsForAccount(account, connection);
                        }
                    }
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
                           VALUES (@AccountID, @TransactionType, @Amount, @Reference, @TransactionDate, @Status)";

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

                    command.ExecuteNonQuery();
                    transaction.DatabaseId = 1; // Mark as saved
                }
            }
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
