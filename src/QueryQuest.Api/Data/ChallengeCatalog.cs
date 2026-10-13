using QueryQuest.Api.Models;

namespace QueryQuest.Api.Data;

public static class ChallengeCatalog
{
    private static readonly List<Challenge> _challenges = new()
    {
        // ------------------ Category 1: SQL Basics ------------------
        new Challenge
        {
            Id = "c1-select-all-employees",
            Title = "Select All Employees",
            Category = "SQL Basics",
            Difficulty = "Beginner",
            Order = 1,
            LearningObjective = "Understand the fundamental SELECT * FROM table syntax in SQL to retrieve all columns and rows.",
            ProblemStatement = "Retrieve all records and columns from the `employees` table to inspect the team roster.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "employee_id", "first_name", "last_name", "email", "salary", "hire_date", "department_id" },
            StarterSql = "-- Write a query to select all columns and rows from the employees table\nSELECT ",
            Hints = new List<string>
            {
                "Use the asterisk symbol (*) after SELECT to retrieve every column in a table.",
                "Specify the source table using FROM employees.",
                "Your complete query should be: SELECT * FROM employees;"
            },
            Explanation = "The `SELECT * FROM employees` statement instructs the database engine to retrieve all columns and all rows available in the `employees` table without any filtering.",
            ReferenceSolution = "SELECT * FROM employees;",
            RequiresExactOrdering = false,
            Points = 100
        },
        new Challenge
        {
            Id = "c2-select-specific-columns",
            Title = "Select Specific Columns",
            Category = "SQL Basics",
            Difficulty = "Beginner",
            Order = 2,
            LearningObjective = "Learn how to project only the required columns instead of retrieving all data.",
            ProblemStatement = "Retrieve the `first_name`, `last_name`, and `salary` for all employees from the `employees` table.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "first_name", "last_name", "salary" },
            StarterSql = "-- Select only first_name, last_name, and salary from employees\nSELECT ",
            Hints = new List<string>
            {
                "List column names separated by commas directly after the SELECT keyword.",
                "Do not use SELECT * when specific columns are requested.",
                "Structure: SELECT first_name, last_name, salary FROM employees;"
            },
            Explanation = "Column projection improves query efficiency by fetching only necessary data over the wire and avoiding unnecessary memory usage.",
            ReferenceSolution = "SELECT first_name, last_name, salary FROM employees;",
            RequiresExactOrdering = false,
            Points = 100
        },
        new Challenge
        {
            Id = "c3-filter-by-department",
            Title = "Filter Employees by Department",
            Category = "SQL Basics",
            Difficulty = "Beginner",
            Order = 3,
            LearningObjective = "Filter rows using a simple WHERE equality condition.",
            ProblemStatement = "Find all employees who belong to Engineering (`department_id = 1`). Return their `first_name`, `last_name`, and `department_id`.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "first_name", "last_name", "department_id" },
            StarterSql = "-- Find all employees where department_id is 1\nSELECT first_name, last_name, department_id\nFROM employees\nWHERE ",
            Hints = new List<string>
            {
                "Use the WHERE clause to specify conditions that rows must satisfy.",
                "The equality operator in SQL is the single equal sign (=).",
                "Complete the clause with: WHERE department_id = 1"
            },
            Explanation = "The WHERE clause filters rows before projection or aggregation. Only rows matching the boolean condition are returned.",
            ReferenceSolution = "SELECT first_name, last_name, department_id FROM employees WHERE department_id = 1;",
            RequiresExactOrdering = false,
            Points = 100
        },
        new Challenge
        {
            Id = "c4-salary-above-threshold",
            Title = "Salaries Above a Threshold",
            Category = "SQL Basics",
            Difficulty = "Beginner",
            Order = 4,
            LearningObjective = "Use numerical comparison operators (> or >=) in a WHERE clause.",
            ProblemStatement = "Retrieve the `first_name`, `last_name`, and `salary` of all employees earning strictly more than 80,000.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "first_name", "last_name", "salary" },
            StarterSql = "-- Find employees with salary > 80000\nSELECT first_name, last_name, salary\nFROM employees\nWHERE ",
            Hints = new List<string>
            {
                "Use the strictly greater-than operator (>) with the salary column.",
                "Do not format numbers with currency symbols or commas in SQL queries.",
                "Condition: WHERE salary > 80000;"
            },
            Explanation = "Numerical operators in SQL include =, != (<>), >, <, >=, and <=. Numbers are entered as plain digits.",
            ReferenceSolution = "SELECT first_name, last_name, salary FROM employees WHERE salary > 80000;",
            RequiresExactOrdering = false,
            Points = 100
        },

        // ------------------ Category 2: Filtering and Sorting ------------------
        new Challenge
        {
            Id = "c5-sort-products-by-price",
            Title = "Sort Products by Price",
            Category = "Filtering and Sorting",
            Difficulty = "Beginner",
            Order = 5,
            LearningObjective = "Order query results using the ORDER BY clause with DESC modifier.",
            ProblemStatement = "List the `product_name` and `unit_price` of all products, ordered from most expensive to least expensive (descending).",
            RequiredTables = new List<string> { "products" },
            ExpectedColumns = new List<string> { "product_name", "unit_price" },
            StarterSql = "-- Retrieve product_name and unit_price sorted descending by unit_price\nSELECT product_name, unit_price\nFROM products\nORDER BY ",
            Hints = new List<string>
            {
                "Use the ORDER BY clause at the end of the query.",
                "Add the keyword DESC after the column name to sort in descending order.",
                "Complete with: ORDER BY unit_price DESC;"
            },
            Explanation = "By default, ORDER BY sorts in ascending order (ASC). Specify DESC to sort from highest to lowest.",
            ReferenceSolution = "SELECT product_name, unit_price FROM products ORDER BY unit_price DESC;",
            RequiresExactOrdering = true,
            Points = 100
        },
        new Challenge
        {
            Id = "c6-customers-from-city",
            Title = "Customers from a Specified City",
            Category = "Filtering and Sorting",
            Difficulty = "Beginner",
            Order = 6,
            LearningObjective = "Filter textual data using exact string matching with single quotes.",
            ProblemStatement = "Select the `first_name`, `last_name`, and `city` of all customers who live in `'Scranton'`.",
            RequiredTables = new List<string> { "customers" },
            ExpectedColumns = new List<string> { "first_name", "last_name", "city" },
            StarterSql = "-- Find customers from Scranton\nSELECT first_name, last_name, city\nFROM customers\nWHERE ",
            Hints = new List<string>
            {
                "SQL string literals are enclosed in single quotes: 'Scranton'.",
                "String matching is case-sensitive or depends on SQLite collation; use 'Scranton'.",
                "Filter: WHERE city = 'Scranton';"
            },
            Explanation = "String literals in SQL standard must always use single quotation marks ('value'). Double quotes are used for identifiers/columns in standard SQL.",
            ReferenceSolution = "SELECT first_name, last_name, city FROM customers WHERE city = 'Scranton';",
            RequiresExactOrdering = false,
            Points = 100
        },
        new Challenge
        {
            Id = "c7-between-salary-range",
            Title = "Salary Range with BETWEEN",
            Category = "Filtering and Sorting",
            Difficulty = "Beginner",
            Order = 7,
            LearningObjective = "Use the BETWEEN operator to perform inclusive range filtering.",
            ProblemStatement = "Find all employees with a `salary` between 60,000 and 90,000 (inclusive). Return `first_name`, `last_name`, and `salary`, ordered by `salary` ascending.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "first_name", "last_name", "salary" },
            StarterSql = "-- Use BETWEEN to find salaries from 60000 to 90000 inclusive\nSELECT first_name, last_name, salary\nFROM employees\nWHERE salary BETWEEN ",
            Hints = new List<string>
            {
                "The syntax for BETWEEN is: column BETWEEN low_val AND high_val.",
                "BETWEEN is inclusive on both boundaries.",
                "Remember to append ORDER BY salary ASC (or ORDER BY salary)."
            },
            Explanation = "The BETWEEN operator provides a clean shorthand for `salary >= 60000 AND salary <= 90000`.",
            ReferenceSolution = "SELECT first_name, last_name, salary FROM employees WHERE salary BETWEEN 60000 AND 90000 ORDER BY salary ASC;",
            RequiresExactOrdering = true,
            Points = 100
        },
        new Challenge
        {
            Id = "c8-filter-with-in-and-like",
            Title = "Filter with IN and LIKE",
            Category = "Filtering and Sorting",
            Difficulty = "Beginner",
            Order = 8,
            LearningObjective = "Combine set membership (IN) with wildcard pattern matching (LIKE).",
            ProblemStatement = "Retrieve the `first_name`, `last_name`, and `email` of all employees whose department_id is in (1, 2) AND whose last_name starts with the letter 'S'.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "first_name", "last_name", "email" },
            StarterSql = "-- Use IN and LIKE with AND to filter employees\nSELECT first_name, last_name, email\nFROM employees\nWHERE ",
            Hints = new List<string>
            {
                "Use department_id IN (1, 2) to test membership in a set of values.",
                "Use last_name LIKE 'S%' where '%' is the SQL wildcard for zero or more characters.",
                "Join both criteria with the AND boolean operator."
            },
            Explanation = "The IN operator matches any value in a list, while LIKE with '%' matches patterns. 'S%' matches any text starting with 'S'.",
            ReferenceSolution = "SELECT first_name, last_name, email FROM employees WHERE department_id IN (1, 2) AND last_name LIKE 'S%';",
            RequiresExactOrdering = false,
            Points = 100
        },

        // ------------------ Category 3: Aggregation ------------------
        new Challenge
        {
            Id = "c9-count-employees",
            Title = "Count Total Employees",
            Category = "Aggregation",
            Difficulty = "Beginner",
            Order = 9,
            LearningObjective = "Use the COUNT aggregate function to tally rows.",
            ProblemStatement = "Count the total number of employees in the `employees` table and alias the result as `total_employees`.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "total_employees" },
            StarterSql = "-- Count all employees with an alias\nSELECT COUNT(*) AS total_employees\nFROM employees;",
            Hints =
            new List<string>
            {
                "Use COUNT(*) to count every row in the table.",
                "Provide an alias for the computed column using AS total_employees.",
                "Query: SELECT COUNT(*) AS total_employees FROM employees;"
            },
            Explanation = "COUNT(*) tallies all rows, while COUNT(column) tallies non-NULL values in that column. Aliases name the output column.",
            ReferenceSolution = "SELECT COUNT(*) AS total_employees FROM employees;",
            RequiresExactOrdering = false,
            Points = 100
        },
        new Challenge
        {
            Id = "c10-average-salary",
            Title = "Calculate Average Salary",
            Category = "Aggregation",
            Difficulty = "Beginner",
            Order = 10,
            LearningObjective = "Use mathematical aggregate functions (AVG, ROUND) to summarize numerical data.",
            ProblemStatement = "Calculate the average salary across all employees. Round the result to 2 decimal places and alias it as `avg_salary`.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "avg_salary" },
            StarterSql = "-- Calculate average salary rounded to 2 decimal places\nSELECT ",
            Hints =
            new List<string>
            {
                "Use AVG(salary) to compute the arithmetic mean.",
                "Wrap AVG inside ROUND(..., 2) to format to 2 decimal points.",
                "Complete with: SELECT ROUND(AVG(salary), 2) AS avg_salary FROM employees;"
            },
            Explanation = "AVG ignores NULL entries and returns a floating-point average. ROUND(value, n) truncates or rounds to n decimals.",
            ReferenceSolution = "SELECT ROUND(AVG(salary), 2) AS avg_salary FROM employees;",
            RequiresExactOrdering = false,
            Points = 100
        },
        new Challenge
        {
            Id = "c11-group-by-department",
            Title = "Group Employees by Department",
            Category = "Aggregation",
            Difficulty = "Intermediate",
            Order = 11,
            LearningObjective = "Partition data into groups using GROUP BY and calculate aggregate counts.",
            ProblemStatement = "Count how many employees are in each department. Return `department_id` and the count aliased as `employee_count` for all employees assigned to a department (ignore NULL department_id). Order by `department_id` ascending.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "department_id", "employee_count" },
            StarterSql = "-- Group by department_id and count employees\nSELECT department_id, COUNT(*) AS employee_count\nFROM employees\nWHERE department_id IS NOT NULL\nGROUP BY ",
            Hints =
            new List<string>
            {
                "Add GROUP BY department_id after the WHERE clause.",
                "Make sure to filter out unassigned employees using WHERE department_id IS NOT NULL.",
                "Order the groups with ORDER BY department_id ASC."
            },
            Explanation = "GROUP BY collapses multiple rows sharing the same key into a single summary row per group. Aggregate functions operate on each group separately.",
            ReferenceSolution = "SELECT department_id, COUNT(*) AS employee_count FROM employees WHERE department_id IS NOT NULL GROUP BY department_id ORDER BY department_id ASC;",
            RequiresExactOrdering = true,
            Points = 120
        },
        new Challenge
        {
            Id = "c12-filter-groups-having",
            Title = "Filter Groups with HAVING",
            Category = "Aggregation",
            Difficulty = "Intermediate",
            Order = 12,
            LearningObjective = "Distinguish WHERE (row filtering) from HAVING (aggregate group filtering).",
            ProblemStatement = "Find all departments that have 2 or more employees. Return `department_id` and `employee_count` (COUNT(*)), ordered by `employee_count` DESC.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "department_id", "employee_count" },
            StarterSql = "-- Group by department and use HAVING to filter counts\nSELECT department_id, COUNT(*) AS employee_count\nFROM employees\nWHERE department_id IS NOT NULL\nGROUP BY department_id\nHAVING ",
            Hints =
            new List<string>
            {
                "The HAVING clause evaluates conditions on aggregated results, whereas WHERE operates on raw rows.",
                "Specify HAVING COUNT(*) >= 2.",
                "Add ORDER BY employee_count DESC."
            },
            Explanation = "WHERE filters individual records before groups are formed. HAVING filters the resulting aggregated groups after GROUP BY.",
            ReferenceSolution = "SELECT department_id, COUNT(*) AS employee_count FROM employees WHERE department_id IS NOT NULL GROUP BY department_id HAVING COUNT(*) >= 2 ORDER BY employee_count DESC;",
            RequiresExactOrdering = true,
            Points = 120
        },

        // ------------------ Category 4: Joins ------------------
        new Challenge
        {
            Id = "c13-join-employees-departments",
            Title = "Join Employees and Departments",
            Category = "Joins",
            Difficulty = "Intermediate",
            Order = 13,
            LearningObjective = "Combine rows from two tables using an INNER JOIN based on a foreign key.",
            ProblemStatement = "Retrieve each employee's `first_name`, `last_name`, and their corresponding `department_name` and `location`. Order results by `last_name` ascending.",
            RequiredTables = new List<string> { "employees", "departments" },
            ExpectedColumns = new List<string> { "first_name", "last_name", "department_name", "location" },
            StarterSql = "-- Join employees with departments on department_id\nSELECT e.first_name, e.last_name, d.department_name, d.location\nFROM employees e\nJOIN departments d ON ",
            Hints =
            new List<string>
            {
                "Use the ON keyword to specify the joining condition between the primary key and foreign key.",
                "Match on e.department_id = d.department_id.",
                "Add ORDER BY e.last_name ASC."
            },
            Explanation = "INNER JOIN matches rows where the join condition is satisfied in both tables. Employees without a department or departments without employees are omitted.",
            ReferenceSolution = "SELECT e.first_name, e.last_name, d.department_name, d.location FROM employees e JOIN departments d ON e.department_id = d.department_id ORDER BY e.last_name ASC;",
            RequiresExactOrdering = true,
            Points = 120
        },
        new Challenge
        {
            Id = "c14-orders-with-customer-names",
            Title = "Orders with Customer Names",
            Category = "Joins",
            Difficulty = "Intermediate",
            Order = 14,
            LearningObjective = "Join transaction data with entity data to produce human-readable reports.",
            ProblemStatement = "List all orders showing `order_id`, customer's `first_name`, `last_name`, `order_date`, and `total_amount`. Order by `order_id` ascending.",
            RequiredTables = new List<string> { "orders", "customers" },
            ExpectedColumns = new List<string> { "order_id", "first_name", "last_name", "order_date", "total_amount" },
            StarterSql = "-- Join orders and customers\nSELECT o.order_id, c.first_name, c.last_name, o.order_date, o.total_amount\nFROM orders o\nJOIN customers c ON ",
            Hints =
            new List<string>
            {
                "Link orders and customers using o.customer_id = c.customer_id.",
                "Ensure column aliases match the expected names.",
                "Sort using ORDER BY o.order_id ASC."
            },
            Explanation = "Relational database normalization breaks data into separate tables (customers, orders) to eliminate redundancy, linked via foreign keys.",
            ReferenceSolution = "SELECT o.order_id, c.first_name, c.last_name, o.order_date, o.total_amount FROM orders o JOIN customers c ON o.customer_id = c.customer_id ORDER BY o.order_id ASC;",
            RequiresExactOrdering = true,
            Points = 120
        },
        new Challenge
        {
            Id = "c15-products-and-order-items",
            Title = "Combine Products and Order Items",
            Category = "Joins",
            Difficulty = "Intermediate",
            Order = 15,
            LearningObjective = "Join line-item details with catalog products and calculate computed line totals.",
            ProblemStatement = "For order_id 1, retrieve `product_name`, `quantity`, line item `unit_price`, and calculate the line item total as `line_total` (quantity * unit_price). Order by `line_total` DESC.",
            RequiredTables = new List<string> { "order_items", "products" },
            ExpectedColumns = new List<string> { "product_name", "quantity", "unit_price", "line_total" },
            StarterSql = "-- Join order_items with products for order_id = 1\nSELECT p.product_name, oi.quantity, oi.unit_price, (oi.quantity * oi.unit_price) AS line_total\nFROM order_items oi\nJOIN products p ON ",
            Hints =
            new List<string>
            {
                "Join condition: oi.product_id = p.product_id.",
                "Filter specifically for order_id = 1 using WHERE oi.order_id = 1.",
                "Sort by the computed column: ORDER BY line_total DESC."
            },
            Explanation = "SQL supports computed columns in the SELECT clause using basic arithmetic operators like `*`, `+`, `-`, and `/`.",
            ReferenceSolution = "SELECT p.product_name, oi.quantity, oi.unit_price, (oi.quantity * oi.unit_price) AS line_total FROM order_items oi JOIN products p ON oi.product_id = p.product_id WHERE oi.order_id = 1 ORDER BY line_total DESC;",
            RequiresExactOrdering = true,
            Points = 120
        },
        new Challenge
        {
            Id = "c16-customers-with-no-orders",
            Title = "Find Customers with No Orders",
            Category = "Joins",
            Difficulty = "Intermediate",
            Order = 16,
            LearningObjective = "Use LEFT JOIN and test for NULL to find unmatched records (anti-join pattern).",
            ProblemStatement = "Find all customers who have never placed an order. Return their `customer_id`, `first_name`, `last_name`, and `email`, ordered by `customer_id` ascending.",
            RequiredTables = new List<string> { "customers", "orders" },
            ExpectedColumns = new List<string> { "customer_id", "first_name", "last_name", "email" },
            StarterSql = "-- Use a LEFT JOIN from customers to orders and filter where order_id IS NULL\nSELECT c.customer_id, c.first_name, c.last_name, c.email\nFROM customers c\nLEFT JOIN orders o ON ",
            Hints =
            new List<string>
            {
                "LEFT JOIN keeps all rows from the left table (customers) even if there is no matching row in orders.",
                "Unmatched rows in the right table produce NULL values in its columns.",
                "Filter with WHERE o.order_id IS NULL and ORDER BY c.customer_id ASC."
            },
            Explanation = "The `LEFT JOIN ... WHERE right_table.id IS NULL` pattern is the standard, highly efficient anti-join technique in SQL.",
            ReferenceSolution = "SELECT c.customer_id, c.first_name, c.last_name, c.email FROM customers c LEFT JOIN orders o ON c.customer_id = o.customer_id WHERE o.order_id IS NULL ORDER BY c.customer_id ASC;",
            RequiresExactOrdering = true,
            Points = 130
        },

        // ------------------ Category 5: Subqueries and Advanced Queries ------------------
        new Challenge
        {
            Id = "c17-employees-above-average-salary",
            Title = "Employees Above Average Salary",
            Category = "Subqueries and Advanced Queries",
            Difficulty = "Intermediate",
            Order = 17,
            LearningObjective = "Use a scalar subquery within a WHERE clause to compare against dynamic metrics.",
            ProblemStatement = "Find all employees whose `salary` is strictly greater than the company-wide average salary. Return `first_name`, `last_name`, and `salary`, ordered by `salary` DESC.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "first_name", "last_name", "salary" },
            StarterSql = "-- Subquery to find employees earning above company average\nSELECT first_name, last_name, salary\nFROM employees\nWHERE salary > (SELECT ...)\nORDER BY salary DESC;",
            Hints =
            new List<string>
            {
                "The subquery inside parentheses should calculate the average salary: (SELECT AVG(salary) FROM employees).",
                "Compare directly: WHERE salary > (SELECT AVG(salary) FROM employees).",
                "Order results by salary in descending order."
            },
            Explanation = "A scalar subquery produces a single value (one column, one row) and can be used anywhere an expression or constant is expected.",
            ReferenceSolution = "SELECT first_name, last_name, salary FROM employees WHERE salary > (SELECT AVG(salary) FROM employees) ORDER BY salary DESC;",
            RequiresExactOrdering = true,
            Points = 140
        },
        new Challenge
        {
            Id = "c18-most-expensive-product-in-category",
            Title = "Most Expensive Product in Each Category",
            Category = "Subqueries and Advanced Queries",
            Difficulty = "Advanced",
            Order = 18,
            LearningObjective = "Identify group extrema by joining against an aggregated subquery or using IN with a composite tuple.",
            ProblemStatement = "Find the most expensive product in each category. Return the `category`, `product_name`, and `unit_price`, ordered by `category` ascending.",
            RequiredTables = new List<string> { "products" },
            ExpectedColumns = new List<string> { "category", "product_name", "unit_price" },
            StarterSql = "-- Find the highest priced product per category\nSELECT p.category, p.product_name, p.unit_price\nFROM products p\nWHERE p.unit_price = (\n    SELECT MAX(p2.unit_price) FROM products p2 WHERE p2.category = p.category\n)\nORDER BY p.category ASC;",
            Hints =
            new List<string>
            {
                "A correlated subquery compares against MAX(unit_price) filtered to the outer query's category.",
                "Alternatively, join products with a subquery of (SELECT category, MAX(unit_price) AS max_price FROM products GROUP BY category).",
                "Sort output by category ASC."
            },
            Explanation = "In a correlated subquery, the inner query references values from the outer query for each row candidate.",
            ReferenceSolution = "SELECT p.category, p.product_name, p.unit_price FROM products p WHERE p.unit_price = (SELECT MAX(p2.unit_price) FROM products p2 WHERE p2.category = p.category) ORDER BY p.category ASC;",
            RequiresExactOrdering = true,
            Points = 150
        },
        new Challenge
        {
            Id = "c19-correlated-subquery-dept-avg",
            Title = "Correlated Subquery: Department Comparison",
            Category = "Subqueries and Advanced Queries",
            Difficulty = "Advanced",
            Order = 19,
            LearningObjective = "Write a correlated subquery that compares an entity directly against its peer group average.",
            ProblemStatement = "Find employees who earn more than the average salary of their own department. Return `first_name`, `last_name`, `department_id`, and `salary`, ordered by `department_id` ASC, then `salary` DESC.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "first_name", "last_name", "department_id", "salary" },
            StarterSql = "-- Compare employee salary with the department average using a correlated subquery\nSELECT e1.first_name, e1.last_name, e1.department_id, e1.salary\nFROM employees e1\nWHERE e1.salary > (\n    SELECT AVG(e2.salary)\n    FROM employees e2\n    WHERE e2.department_id = e1.department_id\n)\nORDER BY e1.department_id ASC, e1.salary DESC;",
            Hints =
            new List<string>
            {
                "Alias the outer table as e1 and the inner table as e2.",
                "In the subquery WHERE clause, link the two: e2.department_id = e1.department_id.",
                "Ensure both department_id ASC and salary DESC sorting criteria are applied."
            },
            Explanation = "Correlated subqueries execute conceptually once for every row evaluated by the outer query, enabling localized group benchmarks.",
            ReferenceSolution = "SELECT e1.first_name, e1.last_name, e1.department_id, e1.salary FROM employees e1 WHERE e1.salary > (SELECT AVG(e2.salary) FROM employees e2 WHERE e2.department_id = e1.department_id) ORDER BY e1.department_id ASC, e1.salary DESC;",
            RequiresExactOrdering = true,
            Points = 150
        },
        new Challenge
        {
            Id = "c20-common-table-expression",
            Title = "Common Table Expression (CTE)",
            Category = "Subqueries and Advanced Queries",
            Difficulty = "Advanced",
            Order = 20,
            LearningObjective = "Structure complex multi-step queries cleanly using WITH (Common Table Expressions).",
            ProblemStatement = "Using a CTE named `CustomerSpending`, calculate total spending (`total_spent` = SUM(total_amount)) and order count (`order_count` = COUNT(*)) per customer. Then select `customer_id`, `total_spent`, and `order_count` for customers who spent more than 500. Order by `total_spent` DESC.",
            RequiredTables = new List<string> { "orders" },
            ExpectedColumns = new List<string> { "customer_id", "total_spent", "order_count" },
            StarterSql = "-- Define a CTE and filter customers with total_spent > 500\nWITH CustomerSpending AS (\n    SELECT customer_id, SUM(total_amount) AS total_spent, COUNT(*) AS order_count\n    FROM orders\n    GROUP BY customer_id\n)\nSELECT customer_id, total_spent, order_count\nFROM CustomerSpending\nWHERE total_spent > 500\nORDER BY total_spent DESC;",
            Hints =
            new List<string>
            {
                "Start with WITH CustomerSpending AS ( ... ).",
                "Inside the CTE, GROUP BY customer_id and calculate SUM(total_amount) and COUNT(*).",
                "In the final query, filter WHERE total_spent > 500 and ORDER BY total_spent DESC."
            },
            Explanation = "Common Table Expressions (CTEs) create temporary named result sets that make queries more readable, modular, and maintainable than nested subqueries.",
            ReferenceSolution = "WITH CustomerSpending AS (SELECT customer_id, SUM(total_amount) AS total_spent, COUNT(*) AS order_count FROM orders GROUP BY customer_id) SELECT customer_id, total_spent, order_count FROM CustomerSpending WHERE total_spent > 500 ORDER BY total_spent DESC;",
            RequiresExactOrdering = true,
            Points = 150
        },

        // ------------------ Category 6: Data Analysis ------------------
        new Challenge
        {
            Id = "c21-revenue-by-product",
            Title = "Calculate Revenue by Product",
            Category = "Data Analysis",
            Difficulty = "Advanced",
            Order = 21,
            LearningObjective = "Aggregate financial transactions across related catalog entities and format numerical sums.",
            ProblemStatement = "Calculate total revenue generated by each product across all order items (`total_revenue` = SUM(oi.quantity * oi.unit_price)) and total units sold (`total_units` = SUM(oi.quantity)). Return `product_name`, `total_units`, and `total_revenue`, ordered by `total_revenue` DESC.",
            RequiredTables = new List<string> { "products", "order_items" },
            ExpectedColumns = new List<string> { "product_name", "total_units", "total_revenue" },
            StarterSql = "-- Join products and order_items, group by product_name, and calculate totals\nSELECT p.product_name, SUM(oi.quantity) AS total_units, ROUND(SUM(oi.quantity * oi.unit_price), 2) AS total_revenue\nFROM products p\nJOIN order_items oi ON p.product_id = oi.product_id\nGROUP BY p.product_name\nORDER BY total_revenue DESC;",
            Hints =
            new List<string>
            {
                "Join products p with order_items oi on p.product_id = oi.product_id.",
                "GROUP BY p.product_name.",
                "Calculate total units sold with SUM(oi.quantity) and revenue with SUM(oi.quantity * oi.unit_price).",
                "Order the final list by total_revenue DESC."
            },
            Explanation = "Grouping transaction line items by product dimension allows reporting key performance indicators (KPIs) like total sales volume and product revenue.",
            ReferenceSolution = "SELECT p.product_name, SUM(oi.quantity) AS total_units, ROUND(SUM(oi.quantity * oi.unit_price), 2) AS total_revenue FROM products p JOIN order_items oi ON p.product_id = oi.product_id GROUP BY p.product_name ORDER BY total_revenue DESC;",
            RequiresExactOrdering = true,
            Points = 150
        },
        new Challenge
        {
            Id = "c22-monthly-sales-totals",
            Title = "Find Monthly Sales Totals",
            Category = "Data Analysis",
            Difficulty = "Advanced",
            Order = 22,
            LearningObjective = "Perform time-series aggregation using date manipulation functions in SQLite.",
            ProblemStatement = "Extract the year and month (`sales_month` in YYYY-MM format using `substr(order_date, 1, 7)`), total orders count (`order_count`), and total sales amount (`total_sales` rounded to 2 decimals) from the `orders` table. Order by `sales_month` ASC.",
            RequiredTables = new List<string> { "orders" },
            ExpectedColumns = new List<string> { "sales_month", "order_count", "total_sales" },
            StarterSql = "-- Group orders by month (substr(order_date, 1, 7))\nSELECT substr(order_date, 1, 7) AS sales_month, COUNT(*) AS order_count, ROUND(SUM(total_amount), 2) AS total_sales\nFROM orders\nGROUP BY sales_month\nORDER BY sales_month ASC;",
            Hints =
            new List<string>
            {
                "In SQLite, dates formatted as 'YYYY-MM-DD' can have the month extracted with substr(order_date, 1, 7) or strftime('%Y-%m', order_date).",
                "Group by the formatted month alias.",
                "Sort chronologically using ORDER BY sales_month ASC."
            },
            Explanation = "Time-series aggregations summarize trends over consistent periods (e.g., month-over-month revenue trends).",
            ReferenceSolution = "SELECT substr(order_date, 1, 7) AS sales_month, COUNT(*) AS order_count, ROUND(SUM(total_amount), 2) AS total_sales FROM orders GROUP BY sales_month ORDER BY sales_month ASC;",
            RequiresExactOrdering = true,
            Points = 150
        },
        new Challenge
        {
            Id = "c23-rank-employees-window-function",
            Title = "Rank Employees by Salary within Department",
            Category = "Data Analysis",
            Difficulty = "Advanced",
            Order = 23,
            LearningObjective = "Apply window functions (RANK() OVER PARTITION BY) to compute departmental rankings.",
            ProblemStatement = "Rank all employees by salary within each department from highest to lowest salary. Return `first_name`, `last_name`, `department_id`, `salary`, and their rank as `salary_rank` using `DENSE_RANK() OVER (PARTITION BY department_id ORDER BY salary DESC)`. Filter out employees with NULL department_id, and order by `department_id` ASC, then `salary_rank` ASC.",
            RequiredTables = new List<string> { "employees" },
            ExpectedColumns = new List<string> { "first_name", "last_name", "department_id", "salary", "salary_rank" },
            StarterSql = "-- Use DENSE_RANK() OVER (PARTITION BY department_id ORDER BY salary DESC)\nSELECT first_name, last_name, department_id, salary,\n       DENSE_RANK() OVER (PARTITION BY department_id ORDER BY salary DESC) AS salary_rank\nFROM employees\nWHERE department_id IS NOT NULL\nORDER BY department_id ASC, salary_rank ASC;",
            Hints =
            new List<string>
            {
                "Window functions compute values across sets of rows that are related to the current row without collapsing them like GROUP BY.",
                "Use DENSE_RANK() OVER (PARTITION BY department_id ORDER BY salary DESC) AS salary_rank.",
                "Filter unassigned employees: WHERE department_id IS NOT NULL."
            },
            Explanation = "Window functions preserve row-level details while calculating aggregations or rankings over specific partitions of data.",
            ReferenceSolution = "SELECT first_name, last_name, department_id, salary, DENSE_RANK() OVER (PARTITION BY department_id ORDER BY salary DESC) AS salary_rank FROM employees WHERE department_id IS NOT NULL ORDER BY department_id ASC, salary_rank ASC;",
            RequiresExactOrdering = true,
            Points = 160
        },
        new Challenge
        {
            Id = "c24-top-customers-by-spending",
            Title = "Top Customers by Total Spending",
            Category = "Data Analysis",
            Difficulty = "Advanced",
            Order = 24,
            LearningObjective = "Synthesize multi-table joins, aggregation, grouping, and LIMIT to identify VIP customers.",
            ProblemStatement = "Find the top 3 highest spending customers across all orders. Return `first_name`, `last_name`, `city`, and their total spending as `total_spent` (rounded to 2 decimal places). Order by `total_spent` DESC and limit to 3 rows.",
            RequiredTables = new List<string> { "customers", "orders" },
            ExpectedColumns = new List<string> { "first_name", "last_name", "city", "total_spent" },
            StarterSql = "-- Join customers and orders, sum total_amount, group, sort, and LIMIT to 3\nSELECT c.first_name, c.last_name, c.city, ROUND(SUM(o.total_amount), 2) AS total_spent\nFROM customers c\nJOIN orders o ON c.customer_id = o.customer_id\nGROUP BY c.customer_id, c.first_name, c.last_name, c.city\nORDER BY total_spent DESC\nLIMIT 3;",
            Hints =
            new List<string>
            {
                "Join customers c and orders o on c.customer_id = o.customer_id.",
                "Group by customer attributes (e.g. c.customer_id).",
                "Order by the sum in descending order: ORDER BY total_spent DESC.",
                "Append LIMIT 3 at the very end to keep only the top three records."
            },
            Explanation = "Combining GROUP BY, ORDER BY DESC, and LIMIT N is the standard SQL pattern for Top-N leaderboards and cohort analysis.",
            ReferenceSolution = "SELECT c.first_name, c.last_name, c.city, ROUND(SUM(o.total_amount), 2) AS total_spent FROM customers c JOIN orders o ON c.customer_id = o.customer_id GROUP BY c.customer_id, c.first_name, c.last_name, c.city ORDER BY total_spent DESC LIMIT 3;",
            RequiresExactOrdering = true,
            Points = 160
        }
    };

    public static IReadOnlyList<Challenge> GetAll() => _challenges;

    public static Challenge? GetById(string id) =>
        _challenges.FirstOrDefault(c => string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase));
}
