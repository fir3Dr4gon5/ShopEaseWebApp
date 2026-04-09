# ShopEaseWebApp
 
A retail shop web application built with ASP.NET Core Razor Pages, Entity Framework Core, and ASP.NET Identity.
 
---
 
## Prerequisites
 
Before you begin make sure you have the following installed:
 
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (with the **ASP.NET and web development** workload)
- [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio)
- [Git](https://git-scm.com/)
 
---
 
## Getting Started
 
### 1. Clone the Repository
 
Open a terminal and run:
 
```bash
git clone https://github.com/fire3Dr4gon5/ShopEaseWebApp.git
cd ShopEaseWebApp
```
 
### 2. Open in Visual Studio
 
Open the `.sln` file in Visual Studio 2022.
 
### 3. Restore NuGet Packages
 
Visual Studio will restore packages automatically. If it does not, run:
 
```bash
dotnet restore
```
 
### 4. Install EF Core CLI Tools
 
If you do not already have the EF Core tools installed, run:
 
```bash
dotnet tool install --global dotnet-ef
```
 
If already installed, make sure it is up to date:
 
```bash
dotnet tool update --global dotnet-ef
```
 
---

## Setting Up the Database (LocalDB)
 
LocalDB is a lightweight version of SQL Server that comes with Visual Studio. No separate SQL Server installation is needed.
 
### 1. Check your connection string
 
Open `appsettings.json` and confirm the connection string looks like this:
 
```json
"ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ShopEaseWebApp;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```
 
You do not need to create the database manually — EF Core will create it for you in the next step.
 
### 2. Apply Migrations
 
In the terminal (from the project root folder) run:
 
```bash
dotnet ef database update
```
 
This will create the database and all tables automatically including the Identity tables for user login and all shop tables (Products, CartItems, Orders, OrderItems).
 
### 3. Verify the Database
 
To confirm the database was created correctly:
 
1. Open Visual Studio
2. Go to **View → SQL Server Object Explorer**
3. Expand **(localdb)\MSSQLLocalDB → Databases → ShopEaseWebApp → Tables**
 
You should see the following tables:
 
- AspNetUsers
- AspNetRoles
- Products
- CartItems
- Orders
- OrderItems
 
---
 
## Adding Products to the Database
 
Products are seeded automatically the first time the app runs on a fresh database. No manual steps are needed — just run the app after applying migrations and the products will be there.
 
---
 
## Running the App
 
### 1. Trust the HTTPS Developer Certificate
 
The first time you run the app you may get a browser warning about an untrusted certificate. To fix this run the following command once:
 
```bash
dotnet dev-certs https --trust
```
 
A popup will appear asking you to confirm — click **Yes**. You only need to do this once.
 
### 2. Run the App
 
Press **F5** in Visual Studio or run:
 
```bash
dotnet run
```
 
Then open your browser and go to `https://localhost:XXXX` (the port number will be shown in the terminal).
 
### 3. Create an Account
 
When the app loads you will need to create an account before you can use the shop:
 
1. Click **Register** in the top navigation bar
2. Enter your email address and a password
3. Click **Register** to create your account
4. You will be logged in automatically and can now browse products, add items to your cart and place orders
 
---
 
## Project Structure
 
```
ShopEaseWebApp/
├── Data/
│   ├── ApplicationDbContext.cs   # Database context
│   └── Migrations/               # EF Core migrations
├── Models/
│   ├── Product.cs
│   ├── CartItem.cs
│   ├── Order.cs
│   └── OrderItem.cs
├── Pages/
│   ├── Products.cshtml           # Browse all products
│   ├── ProductDetails.cshtml     # View product and add to cart
│   ├── Cart.cshtml               # View and manage cart
│   ├── Checkout.cshtml           # Enter shipping and place order
│   └── OrderConfirmation.cshtml  # Order summary
├── appsettings.json              # App configuration and connection string
└── Program.cs                    # App startup and configuration
```
 
---
 