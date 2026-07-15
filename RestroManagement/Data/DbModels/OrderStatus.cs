namespace RestroManagement.DbModels
{
    public enum OrderStatus
    {
        Pending,     // Order placed, awaiting confirmation
        Accepted,    // Accepted by the restaurant
        Preparing,   // Being cooked / prepared
        Ready,       // Ready for pickup or delivery
        Delivered,   // Order completed / delivered
        Cancelled    // Cancelled by customer or restaurant
    }
}
