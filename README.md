# UsersApp - Order Management System

A comprehensive web application built with ASP.NET Core MVC for managing users, products, and orders with role-based access control.

## 🚀 Features

### User Management
- **Registration & Authentication** - Secure user registration and login system
- **Role-Based Access Control** - Three distinct user roles:
  - **Admin** - Full system access, user management
  - **Accountant** - Access to products and orders
  - **Cashier** - Access to orders only
- **Password Recovery** - Email-based password reset functionality

### Product Management
- Create, read, update, and delete products
- Product pricing with decimal precision
- Restricted to Admin and Accountant roles

### Order Management
- **Multi-item Orders** - Create orders with multiple products
- **Customer Assignment** - Link orders to registered users
- **Real-time Calculations** - Automatic subtotal and total calculations
- **Monthly Grouping** - Orders organized by month with summary statistics
- **Order Tracking** - Detailed order history and status

### Admin Panel
- Complete user management (create, edit, delete users)
- Role assignment and management
- User overview with role-based badges
- Protected routes and actions

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, jQuery, Font Awesome
- **Architecture**: MVC Pattern with Repository-like structure

## 📋 Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB, Express, or full version)
- Visual Studio 2022 or VS Code

## ⚙️ Installation & Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd UsersApp
   ```

2. **Update Connection String**
   Edit `appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=YOUR_SERVER;Database=UsersAppNew;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

## 👤 Default Admin Account

The application automatically creates a default admin account:
- **Email**: `admin@admin.com`
- **Password**: `Admin123!`


## 🔐 Authorization Attributes

The application uses custom authorization attributes for role-based access:

- `[AdminOnly]` - Admin role only
- `[AccountantOrAdmin]` - Admin or Accountant roles
- `[CashierOrAdmin]` - Admin or Cashier roles
- `[AllRoles]` - All authenticated users

## 📊 Database Schema

### Key Tables
- **AspNetUsers** - Extended with FullName for user information
- **Products** - Product catalog with name and price
- **Orders** - Order headers with customer and total information
- **OrderItems** - Individual line items within orders

### Relationships
- One-to-Many: Order → OrderItems
- Many-to-One: OrderItem → Product
- Foreign key constraints maintain data integrity

## 🎨 UI Features

- **Responsive Design** - Mobile-friendly Bootstrap layout
- **Role-based Navigation** - Dynamic menu based on user roles
- **Interactive Forms** - Client-side validation and calculations
- **Alert System** - Success/error message notifications
- **Monthly Order Grouping** - Expandable sections for better organization

## 🔧 Configuration

### Password Requirements
Default password policy (configurable in `Program.cs`):
- Minimum 8 characters
- No special character requirement
- No uppercase/lowercase requirement

### Roles
Three predefined roles are automatically created:
- Admin
- Accountant  
- Cashier

**Built with ❤️ using ASP.NET Core**
