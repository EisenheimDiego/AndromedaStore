using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;//para usar list
namespace MyStoreAPI
{

    //Para no sobrecargar Store con tanto codigo y adelantar un poco del Lab9
    //cargo la funcionaliad de la BD aqui

    //Todo estatico para no estar creando instancias por todo lado    
    public static class DB_Connection
    {        
        private static readonly string connectionString = "server=localhost;user=root;password=123456;database=MyStoreAPI";
        
        //iniciamos la conexion con la BD
        public static void ConnectDB(){

            try{
                //creamos una instancia de mysql
                using (MySqlConnection connection = new MySqlConnection(connectionString)){                
                    //generamos la conexion
                    connection.Open();

                    // creamos la tabla ventas (donde cada carrito seria una venta)
                    string createTableSales = @"
                        CREATE TABLE IF NOT EXISTS Sales (
                            IdSale INT AUTO_INCREMENT PRIMARY KEY,                            
                            PurchaseNum VARCHAR(50) NOT NULL,                           
                            Total DECIMAL(10, 2) NOT NULL,
                            Subtotal DECIMAL(10, 2) NOT NULL,                                                
                            Direction VARCHAR(255) NOT NULL,
                            PaymentMethod INT NOT NULL,
                            DateSale DATETIME NOT NULL
                        );";

                    using (MySqlCommand command = new MySqlCommand(createTableSales, connection)){
                        command.ExecuteNonQuery();
                        Console.WriteLine("Tabla 'Sales' creada correctamente.");
                    }

                    string createTableProducts = @"
                        CREATE TABLE IF NOT EXISTS Products (
                            IdProduct INT AUTO_INCREMENT PRIMARY KEY,
                            Name VARCHAR(255) NOT NULL,
                            ImageUrl VARCHAR(255),
                            Price DECIMAL(10, 2) NOT NULL,
                            Quantity INT NOT NULL,
                            Description TEXT
                        );";

                    using (MySqlCommand command = new MySqlCommand(createTableProducts, connection)){
                        command.ExecuteNonQuery();
                        Console.WriteLine("Tabla 'Compras' creada correctamente.");
                    }
                    
                    string createTableSalesLines = @"
                        CREATE TABLE IF NOT EXISTS SalesLines (
                            IdSale INT NOT NULL,
                            IdProduct INT NOT NULL,
                            FOREIGN KEY (IdSale) REFERENCES Sales(IdSale),
                            FOREIGN KEY (IdProduct) REFERENCES Products(IdProduct)
                        );";                                                    

                    using (MySqlCommand command = new MySqlCommand(createTableSalesLines, connection)){
                        command.ExecuteNonQuery();
                        Console.WriteLine("Tabla 'SalesLines' creada correctamente.");
                    }
                }
            }catch (Exception ex){
                Console.WriteLine("Error al crear la tabla: " + ex.Message);
            }            
        }    

        //Funciones CRUD
        public static void InsertProductsStore(List<Product> allProducts){
            try{
                using (MySqlConnection connection = new MySqlConnection(connectionString)){                
                    connection.Open();
                    foreach (var actualProduct in allProducts){
                        string insertQuery = @"
                            INSERT INTO Products (Name, ImageUrl, Price, Quantity, Description)
                            VALUES (@name, @imageUrl, @price, @quantity, @description);
                        ";

                        using (MySqlCommand command = new MySqlCommand(insertQuery, connection)){                            
                            command.Parameters.AddWithValue("@name", actualProduct.name);
                            command.Parameters.AddWithValue("@imageUrl", actualProduct.imageUrl);
                            command.Parameters.AddWithValue("@price", actualProduct.price);
                            command.Parameters.AddWithValue("@quantity", actualProduct.quantity);
                            command.Parameters.AddWithValue("@description", actualProduct.description);

                            command.ExecuteNonQuery();
                        }
                    }
                    Console.WriteLine("Productos insertados correctamente en la tabla 'Products'.");
                }
            }catch (Exception ex){
                Console.WriteLine("Error al insertar productos en la tabla 'Products': " + ex.Message);
            }            
        }

        public static List<Product> SelectProducts(){

            List<Product> productListToStoreInstance = new List<Product>();

            try{
                //primer usign para establecer la conexion con la BD
                using (MySqlConnection connection = new MySqlConnection(connectionString)){
                    
                    connection.Open();
                    string selectProducts = @"
                        SELECT IdProduct, Name, ImageUrl, Price, Quantity, Description
                        FROM Products;
                        ";                

                    //segundo using para crear un objeto de comandos SQL y que al finalizar se borre y liberen datos
                    using (MySqlCommand command = new MySqlCommand(selectProducts, connection)){
                        //tercer using para crear un objeto de lectura SQL
                        using (MySqlDataReader readerTable = command.ExecuteReader()){

                            //Mientras haya al menos una tupla que leer, guarde los datos de ese Select en la lista
                            while(readerTable.Read()){
                                productListToStoreInstance.Add(new Product{
                                    id = Convert.ToInt32(readerTable["IdProduct"]),
                                    name = readerTable["Name"].ToString(),
                                    imageUrl = readerTable["ImageUrl"].ToString(),
                                    price = Convert.ToDecimal(readerTable["Price"]),
                                    quantity = Convert.ToInt32(readerTable["Quantity"]),
                                    description = readerTable["Description"].ToString()
                                });
                            }
                        }
                    }
                }
            }catch (Exception ex){
                Console.WriteLine("Error al insertar productos en la tabla 'Products': " + ex.Message);
            }            
            return productListToStoreInstance;
        }

        public static string InsertSale(Cart purchasedCart){
            Console.WriteLine("Dentro de SaleLines a insertarlo");
            try{                                        
                //El bloque using(MySql....) es una buena practica ya que conecta y desconecta de la bd, liberando recursos
                //y evitar dejando conexiones abiertas
                using (MySqlConnection connection = new MySqlConnection(connectionString)){

                    connection.Open();                    

                    //Hacemos el insert
                    string insertSale = @"
                    INSERT INTO Sales (Total,PurchaseNum, Subtotal, Direction, PaymentMethod,DateSale)
                    VALUES (@total, @purchaseNum, @subtotal, @direction, @paymentMethod,@dateSale);
                    ";

                    //id global unico para el comprobante                    
                    string purchaseNum = Guid.NewGuid().ToString();                    
                    MySqlCommand command = new MySqlCommand(insertSale, connection);
                    command.Parameters.AddWithValue("@total", purchasedCart.Total);
                    command.Parameters.AddWithValue("@purchaseNum", purchaseNum);
                    command.Parameters.AddWithValue("@subtotal", purchasedCart.Subtotal);
                    command.Parameters.AddWithValue("@direction", purchasedCart.Direction);                  
                    command.Parameters.AddWithValue("@paymentMethod", (int)purchasedCart.PaymentMethod.payment);
                    command.Parameters.AddWithValue("@dateSale", DateTime.Now);
                    command.ExecuteNonQuery();
                   
                    InsertSalesLine(purchaseNum,purchasedCart);
                    //connection.Close();
                    Console.WriteLine("Exito al realizar la compra, guadado en Sales: ");
                    return purchaseNum;
                }            
            }catch (Exception ex){
                Console.WriteLine("Error al insertar la venta: " + ex.Message);
                return null;
            }
        }
        
        private static void InsertSalesLine(string saleId, Cart purchasedCart){
            try{
                using (MySqlConnection connection = new MySqlConnection(connectionString)){
                    connection.Open();

                    string insertSalesLine = @"
                        INSERT INTO SalesLines (IdSale, IdProduct)
                        VALUES (@saleId, @productId);";

                    using (MySqlCommand command = new MySqlCommand(insertSalesLine, connection)){                        
                       
                        foreach (var actualProductId in purchasedCart.allProducts){
                            command.Parameters.AddWithValue("@saleId", saleId);
                            command.Parameters.AddWithValue("@productId", actualProductId);
                            command.ExecuteNonQuery();
                            Console.WriteLine("SaleLine registrada");                            
                        }                                                    
                    }   
                }             
            }catch (Exception ex){
                Console.WriteLine("Error al insertar la línea de venta: " + ex.Message);
            }
                    
        }
        
    }
}