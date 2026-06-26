using System.Collections.Generic;

namespace RestroManagement.ViewModels
{
    public class PortionOption
    {
        public string Name { get; set; } = string.Empty;
        public double Price { get; set; }
    }

    public class CartItem
    {
        public int FoodItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string PortionName { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Total => Price * Quantity;

        // List of available portions for editing in the cart
        public List<PortionOption> Portions { get; set; } = new List<PortionOption>();
    }
}
