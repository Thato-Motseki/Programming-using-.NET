using Microsoft.Data.SqlClient;

namespace SEIMS.Services
{
    public class DataOperationHandler
    {
        // Class-level variable: shared by methods in this class.
        private int _successfulOperations;

        public int SuccessfulOperations => _successfulOperations;

        public bool Execute(
            Func<bool> operation,
            Action<string> reportError)
        {
            if (operation is null)
                throw new ArgumentNullException(nameof(operation));

            if (reportError is null)
                throw new ArgumentNullException(nameof(reportError));

            // Local variable: exists only within this method.
            bool succeeded = false;

            try
            {
                succeeded = operation();
                if (succeeded)
                    _successfulOperations++;

                return succeeded;
            }
            catch (FormatException ex)
            {
                reportError($"Invalid data format: {ex.Message}");
                return false;
            }
            catch (SqlException ex)
            {
                reportError($"Database operation failed: {ex.Message}");
                return false;
            }
            finally
            {
                Console.WriteLine(
                    $"Operation finished. Successful operations: " +
                    $"{_successfulOperations}");
            }
        }
    }
}