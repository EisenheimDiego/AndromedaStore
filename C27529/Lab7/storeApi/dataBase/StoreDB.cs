using System;
using System.Data.Common;
using System.IO.Compression;
using MySqlConnector;

namespace storeApi.dataBase;
public sealed class StoreDB
{
    public StoreDB()
    {


    }

    internal static void CreateMysql()
    {
          var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Producto 1",
                    Description = "Audífonos con alta fidelidad",
                    Price = 20000,
                    ImageURL = "https://images-na.ssl-images-amazon.com/images/G/01/AmazonExports/Fuji/2021/June/Fuji_Quad_Headset_1x._SY116_CB667159060_.jpg"
                },
                new Product
                {
                    Id = 2,
                    Name = "Producto 2",
                    Description = "Control PS4",
                    Price = 20000,
                    ImageURL = "https://images-na.ssl-images-amazon.com/images/G/01/AmazonExports/Karu/2021/June/Karu_LP_Controller2.png"
                },
                new Product
                {
                    Id = 3,
                    Name = "Producto 3",
                    Description = "PS4 1TB",
                    Price = 20000,
                    ImageURL = "https://images-na.ssl-images-amazon.com/images/G/01/AmazonExports/Karu/2021/June/Karu_LP_Playstation3.jpg"
                },
                new Product
                {
                    Id = 4,
                    Name = "Producto 4",
                    Description = "Crash Bandicoot 4 Switch",
                    Price = 20000,
                    ImageURL = "https://images-na.ssl-images-amazon.com/images/G/01/AmazonExports/Karu/2021/June/Karu_LP_Game.png"
                },
                new Product
                {
                    Id = 5,
                    Name = "Producto 5",
                    Description = "Mouse Logitech",
                    Price = 20000,
                    ImageURL = "https://images-na.ssl-images-amazon.com/images/G/01/AmazonExports/Karu/2021/June/Karu_Quad_Mouse.jpg"
                },
                new Product
                {
                    Id = 6,
                    Name = "Producto 6",
                    Description = "Silla Oficina",
                    Price = 20000,
                    ImageURL = "https://images-na.ssl-images-amazon.com/images/G/01/AmazonExports/Karu/2021/June/Karu_Quad_Chair.jpg"
                },
                new Product
                {
                    Id = 7,
                    Name = "Producto 7",
                    Description = "Laptop Acer",
                    Price = 20000,
                    ImageURL = "https://images-na.ssl-images-amazon.com/images/G/01/AmazonExports/Karu/2021/June/Karu_LP_Laptop.png"
                },
                new Product
                {
                    Id = 8,
                    Name = "Producto 8",
                    Description = "Oculus Quest 3",
                    Price = 20000,
                    ImageURL = "https://images-na.ssl-images-amazon.com/images/G/01/AmazonExports/Karu/2021/June/Karu_LP_Oculus2.jpg"
                }
            };




        string connectionString = "Server=localhost;Port=3306;Database=mysql;Uid=root;Pwd=123456;";
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            // Create the products table if it does not exist
            string createTableQuery = @"
                DROP DATABASE IF EXISTS store;
                CREATE DATABASE store;
                use store;
                CREATE TABLE IF NOT EXISTS products (
                    id INT AUTO_INCREMENT PRIMARY KEY,
                    name VARCHAR(100),
                    price DECIMAL(10, 2)
                );

                CREATE TABLE IF NOT EXISTS sales (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    productIDs varchar(100),
                    purchase_date DATETIME NOT NULL,
                    total DECIMAL(10, 2) NOT NULL,
                    payment_method INT NOT NULL,
                    purchase_number VARCHAR(50) NOT NULL
                );
                
                INSERT INTO sales (productIDs, purchase_date, total, payment_method, purchase_number)
                VALUES 
                    ('1' ,'2024-04-11 10:00:00',  50.00, 1, '12345'),
                    ('2','2024-04-11 11:30:00',  75.20, 2, '54321'),
                    ('3' ,'2024-04-11 13:45:00',  100.50, 1, '98765');
                ";


            using (var command = new MySqlCommand(createTableQuery, connection))
            {
                int result = command.ExecuteNonQuery();
                bool dbNoCreated = result < 0;
                if(dbNoCreated)
                    throw new Exception("Error creating the bd");
            }

            // Begin a transaction
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    // Insert 30 products into the table
                    int i =0;
                    foreach(Product prodduct in products)
                    {
                        i++;
                        string productName = $"Product {i}";
                        decimal productPrice = i * 10.0m;

                        string insertProductQuery = @"
                            INSERT INTO products (name, price)
                            VALUES (@name, @price);";

                        using (var insertCommand = new MySqlCommand(insertProductQuery, connection, transaction))
                        {
                            insertCommand.Parameters.AddWithValue("@name", productName);
                            insertCommand.Parameters.AddWithValue("@price", productPrice);
                            insertCommand.ExecuteNonQuery();
                        }
                    }

                    // Commit the transaction if all inserts are successful
                    transaction.Commit();
                }
                catch (Exception)
                {
                    // Rollback the transaction if an error occurs
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}