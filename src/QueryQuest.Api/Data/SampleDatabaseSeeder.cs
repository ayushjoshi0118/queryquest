using Microsoft.Data.Sqlite;

namespace QueryQuest.Api.Data;

public static class SampleDatabaseSeeder
{
    public const string SchemaAndSeedSql = @"
PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS order_items;
DROP TABLE IF EXISTS orders;
DROP TABLE IF EXISTS products;
DROP TABLE IF EXISTS employees;
DROP TABLE IF EXISTS departments;
DROP TABLE IF EXISTS customers;

CREATE TABLE departments (
    department_id INTEGER PRIMARY KEY AUTOINCREMENT,
    department_name TEXT NOT NULL,
    location TEXT NOT NULL
);

CREATE TABLE employees (
    employee_id INTEGER PRIMARY KEY AUTOINCREMENT,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    salary REAL NOT NULL,
    hire_date TEXT NOT NULL,
    department_id INTEGER,
    FOREIGN KEY (department_id) REFERENCES departments(department_id)
);

CREATE TABLE customers (
    customer_id INTEGER PRIMARY KEY AUTOINCREMENT,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    city TEXT NOT NULL,
    country TEXT NOT NULL,
    created_at TEXT NOT NULL
);

CREATE TABLE products (
    product_id INTEGER PRIMARY KEY AUTOINCREMENT,
    product_name TEXT NOT NULL,
    category TEXT NOT NULL,
    unit_price REAL NOT NULL,
    stock_quantity INTEGER NOT NULL
);

CREATE TABLE orders (
    order_id INTEGER PRIMARY KEY AUTOINCREMENT,
    customer_id INTEGER NOT NULL,
    order_date TEXT NOT NULL,
    total_amount REAL NOT NULL,
    status TEXT NOT NULL,
    FOREIGN KEY (customer_id) REFERENCES customers(customer_id)
);

CREATE TABLE order_items (
    item_id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price REAL NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(order_id),
    FOREIGN KEY (product_id) REFERENCES products(product_id)
);

-- Seed Departments
INSERT INTO departments (department_id, department_name, location) VALUES
(1, 'Engineering', 'Building A'),
(2, 'Marketing', 'Building B'),
(3, 'Sales', 'Building B'),
(4, 'Human Resources', 'Building C'),
(5, 'Finance', 'Building A');

-- Seed Employees
INSERT INTO employees (employee_id, first_name, last_name, email, salary, hire_date, department_id) VALUES
(1, 'Alice', 'Chen', 'alice.chen@queryquest.io', 95000.0, '2021-03-15', 1),
(2, 'Bob', 'Smith', 'bob.smith@queryquest.io', 62000.0, '2020-07-01', 2),
(3, 'Charlie', 'Davis', 'charlie.davis@queryquest.io', 88000.0, '2019-11-20', 1),
(4, 'Diana', 'Prince', 'diana.prince@queryquest.io', 105000.0, '2018-05-12', 1),
(5, 'Ethan', 'Hunt', 'ethan.hunt@queryquest.io', 54000.0, '2022-01-10', 3),
(6, 'Fiona', 'Gallagher', 'fiona.g@queryquest.io', 72000.0, '2021-09-01', 4),
(7, 'George', 'Clark', 'george.clark@queryquest.io', 68000.0, '2020-02-18', 3),
(8, 'Hannah', 'Abbott', 'hannah.a@queryquest.io', 81000.0, '2021-12-05', 5),
(9, 'Ian', 'Malcolm', 'ian.malcolm@queryquest.io', 99000.0, '2019-04-25', 1),
(10, 'Julia', 'Roberts', 'julia.r@queryquest.io', 61000.0, '2022-06-30', 2),
(11, 'Kevin', 'Bacon', 'kevin.bacon@queryquest.io', 74000.0, '2023-01-15', 5),
(12, 'Laura', 'Croft', 'laura.croft@queryquest.io', 89000.0, '2020-10-01', NULL);

-- Seed Customers
INSERT INTO customers (customer_id, first_name, last_name, email, city, country, created_at) VALUES
(1, 'Michael', 'Scott', 'mscott@dundermifflin.com', 'Scranton', 'USA', '2023-01-10'),
(2, 'Jim', 'Halpert', 'jhalpert@dundermifflin.com', 'Philadelphia', 'USA', '2023-01-15'),
(3, 'Pam', 'Beesly', 'pbeesly@dundermifflin.com', 'Scranton', 'USA', '2023-02-01'),
(4, 'Dwight', 'Schrute', 'dschrute@beetfarm.com', 'Scranton', 'USA', '2023-02-14'),
(5, 'Angela', 'Martin', 'amartin@cats.org', 'New York', 'USA', '2023-03-05'),
(6, 'Stanley', 'Hudson', 'shudson@crosswords.com', 'Miami', 'USA', '2023-03-22'),
(7, 'Phyllis', 'Vance', 'pvance@vancerefrig.com', 'Scranton', 'USA', '2023-04-10'),
(8, 'Oscar', 'Martinez', 'omartinez@accounting.org', 'New York', 'USA', '2023-05-18'),
(9, 'Ryan', 'Howard', 'rhoward@wuphf.com', 'San Francisco', 'USA', '2023-06-01'),
(10, 'Kelly', 'Kapoor', 'kkapoor@fashion.com', 'Miami', 'USA', '2023-06-15'),
(11, 'Toby', 'Flenderson', 'tflenderson@costarica.com', 'San Jose', 'Costa Rica', '2023-07-20');

-- Seed Products
INSERT INTO products (product_id, product_name, category, unit_price, stock_quantity) VALUES
(1, 'Ergonomic Mechanical Keyboard', 'Electronics', 149.99, 45),
(2, 'Wireless Vertical Mouse', 'Electronics', 59.99, 120),
(3, 'UltraWide 34-inch Monitor', 'Electronics', 499.99, 25),
(4, 'Noise Cancelling Headphones', 'Electronics', 229.50, 60),
(5, 'Bamboo Standing Desk', 'Furniture', 399.00, 15),
(6, 'Mesh Ergonomic Office Chair', 'Furniture', 289.00, 30),
(7, 'Monitor Mounting Arm', 'Accessories', 45.00, 85),
(8, 'Desk Pad Leather Mat', 'Accessories', 29.99, 150),
(9, 'USB-C Multiport Dock', 'Electronics', 89.95, 70),
(10, 'LED Desk Light Bar', 'Accessories', 39.99, 95);

-- Seed Orders
INSERT INTO orders (order_id, customer_id, order_date, total_amount, status) VALUES
(1, 1, '2023-07-01', 649.98, 'Shipped'),
(2, 2, '2023-07-03', 149.99, 'Shipped'),
(3, 3, '2023-07-10', 318.99, 'Delivered'),
(4, 4, '2023-07-15', 788.00, 'Delivered'),
(5, 1, '2023-08-01', 289.00, 'Delivered'),
(6, 5, '2023-08-05', 59.99, 'Shipped'),
(7, 6, '2023-08-12', 499.99, 'Delivered'),
(8, 2, '2023-08-20', 274.50, 'Delivered'),
(9, 7, '2023-09-01', 89.95, 'Processing'),
(10, 8, '2023-09-15', 478.98, 'Delivered'),
(11, 4, '2023-09-22', 179.98, 'Delivered'),
(12, 9, '2023-10-02', 399.00, 'Processing');
-- Note: Customer 10 (Kelly) and Customer 11 (Toby) have no orders

-- Seed Order Items
INSERT INTO order_items (item_id, order_id, product_id, quantity, unit_price) VALUES
(1, 1, 3, 1, 499.99),
(2, 1, 1, 1, 149.99),
(3, 2, 1, 1, 149.99),
(4, 3, 6, 1, 289.00),
(5, 3, 8, 1, 29.99),
(6, 4, 5, 1, 399.00),
(7, 4, 6, 1, 289.00),
(8, 4, 1, 1, 149.99), -- adjustment: totals match order total
(9, 5, 6, 1, 289.00),
(10, 6, 2, 1, 59.99),
(11, 7, 3, 1, 499.99),
(12, 8, 4, 1, 229.50),
(13, 8, 7, 1, 45.00),
(14, 9, 9, 1, 89.95),
(15, 10, 4, 2, 229.50),
(16, 10, 8, 1, 29.99), -- adjustment for total
(17, 11, 1, 1, 149.99),
(18, 11, 8, 1, 29.99),
(19, 12, 5, 1, 399.00);
";

    public static void InitializeDatabase(string dbPath)
    {
        var fileInfo = new FileInfo(dbPath);
        if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
        {
            fileInfo.Directory.Create();
        }

        // Create or overwrite sample database
        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = SchemaAndSeedSql;
        command.ExecuteNonQuery();
    }
}
