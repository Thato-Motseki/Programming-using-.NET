namespace SEIMS.Services
{
    public class InputValidator
    {
        public bool TryGetQuantity(string? input, out int quantity)
        {
            if (!int.TryParse(input, out quantity))
            {
                quantity = 0;
                return false;
            }

            if (quantity < 0)
            {
                quantity = 0;
                return false;
            }

            return true;
        }
    }
}
