using System;
using System.Collections.Generic;

namespace storeApi.Models
{
    public sealed class SalesData
    {
        public DateTime PurchaseDate { get; set; }
        public string PurchaseNumber { get; set; }
        public decimal Total { get; set; }
        public int AmountProducts { get; set; }
        public List<ProductQuantity> ProductsAnnotation { get; set; }

        // Constructor que inicializa todas las propiedades de la clase
        public SalesData(DateTime purchaseDate, string purchaseNPurchaseNumber, decimal total, int amountProducts, List<ProductQuantity> productsAnnotation)
        {
            ValidateSalesData(purchaseNPurchaseNumber, purchaseDate, total, amountProducts, productsAnnotation); 
            this.PurchaseDate = purchaseDate;
            this.PurchaseNumber = purchaseNPurchaseNumber;
            this.Total = total;
            this.AmountProducts = amountProducts;
            this.ProductsAnnotation = productsAnnotation;
        }
        private static void ValidateSalesData(string purchaseNPurchaseNumber, DateTime purchaseDate, decimal total, int amountProducts, List<ProductQuantity> productsAnnotation)
        {
            if (string.IsNullOrEmpty(purchaseNPurchaseNumber)){throw new ArgumentException("El ID de compra no puede estar vacío o nulo.", nameof(purchaseNPurchaseNumber));}
            if (purchaseDate == default(DateTime)){throw new ArgumentException("La fecha de compra no puede ser la predeterminada.", nameof(purchaseDate));}
            if (total < 0){throw new ArgumentException("El total debe ser mayor que cero.", nameof(total));}
            if (amountProducts < 0){throw new ArgumentException("La cantidad de productos debe ser mayor que cero.", nameof(amountProducts));}
        }
    }
}
