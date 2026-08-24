# 📊 Supplier Price Tracking System

An enterprise web application developed with **ASP.NET Core MVC**, **EF Core**, and **SQL Server** to manage and track supplier price quotes.

---

### ✨ Features
* **Role-Based Authentication:** Cookie authentication with `Admin` (Full CRUD) and `Viewer` (Read-only) access control.
* **Date Range Validation:** Automatic conflict detection for overlapping price quote intervals.
* **Search & Server-Side Pagination:** Filter by material/supplier name and date ranges with optimized SQL queries.
* **Export:** One-click CSV/Excel export for filtered records.

---

### 👥 Demo Accounts

| Role | Username | Password | Access |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin` | `admin123` | Full Access (Create, Edit, Delete, View) |
| **Viewer** | `viewer` | `viewer123` | Read-only & CSV Export |

---

### 🚀 Quick Start
1. Configure connection string in `appsettings.json`.
2. Apply database migrations:
   ```bash
   dotnet ef database update
