namespace ShopApi.Models;
public sealed class Cart
{
    public required List<string> products { get; set; }
    public float subtotal {get; set;}
    public required string address { get; set; }

    public required string paymentMethod {get; set;}
}