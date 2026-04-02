# Product Inventory Tracker API - Documentation

## Overview
This API provides comprehensive CRUD operations for managing product inventory, including categories, products, suppliers, users, and stock transactions with soft delete functionality.

## Base URL
```
/api
```

## Features
- **Soft Delete**: Records are marked with Status = 3 instead of being permanently deleted
- **SID-based Operations**: All operations use Structured IDs (SID) for safe identification
- **Pagination**: All list endpoints support pagination
- **Search & Filter**: Advanced search capabilities with sorting options
- **Validation**: FluentValidation for all request models
- **Error Handling**: Comprehensive error handling with appropriate HTTP status codes

---

## API Endpoints

### Categories

#### 1. Get Categories List
**Endpoint:** `GET /api/categories/list`

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Records per page (default: 10, max: 10000)
- `searchText` (string): Search in category name or description
- `sortColumn` (string): Column to sort by (default: LastModifiedAt)
- `sortOrder` (string): ASC or DESC (default: DESC)
- `filters` (string): JSON filters

**Response:**
```json
{
  "meta": {
    "page": 1,
    "page_size": 10,
    "total_results": 25,
    "total_page_num": 3
  },
  "results": [
    {
      "categorySid": "CAT1A2B3C4D5E6F7",
      "categoryName": "Electronics",
      "description": "Electronic devices",
      "status": 1,
      "createdAt": "2024-01-15T10:30:00Z",
      "lastModifiedAt": "2024-01-15T10:30:00Z"
    }
  ]
}
```

#### 2. Get Category by SID
**Endpoint:** `GET /api/categories/{categorySid}`

**Response:** Single category object (see above)

#### 3. Add New Category
**Endpoint:** `POST /api/categories/add`

**Request Body:**
```json
{
  "categoryName": "Electronics",
  "description": "Electronic devices"
}
```

**Validation:**
- `categoryName`: Required, max 100 characters
- `description`: Optional, max 500 characters

**Response:** Created category with SID (201 Created)

#### 4. Update Category
**Endpoint:** `PUT /api/categories/update/{categorySid}`

**Request Body:** Same as Add

**Response:** Updated category object

#### 5. Delete Category (Soft Delete)
**Endpoint:** `DELETE /api/categories/delete/{categorySid}`

**Response:** 204 No Content

---

### Products

#### 1. Get Products List
**Endpoint:** `GET /api/products/list`

**Query Parameters:** Same as Categories

**Response:**
```json
{
  "meta": {...},
  "results": [
    {
      "productSid": "PRD1A2B3C4D5E6F7",
      "productName": "Laptop",
      "sku": "LAP-001",
      "description": "High-performance laptop",
      "categoryName": "Electronics",
      "supplierName": "Tech Supplier Inc",
      "unitPrice": 1299.99,
      "currentStock": 15,
      "reorderThreshold": 5,
      "status": 1,
      "createdAt": "2024-01-15T10:30:00Z",
      "lastModifiedAt": "2024-01-15T10:30:00Z"
    }
  ]
}
```

#### 2. Get Product by SID
**Endpoint:** `GET /api/products/{productSid}`

**Response:** Single product object

#### 3. Add New Product
**Endpoint:** `POST /api/products/add`

**Request Body:**
```json
{
  "productName": "Laptop",
  "sku": "LAP-001",
  "description": "High-performance laptop",
  "categorySid": "CAT1A2B3C4D5E6F7",
  "supplierSid": "SUP1A2B3C4D5E6F7",
  "unitPrice": 1299.99,
  "currentStock": 20,
  "reorderThreshold": 5
}
```

**Validation:**
- `productName`: Required, max 200 characters
- `sku`: Required, max 50 characters, must be unique
- `unitPrice`: Required, must be > 0
- `currentStock`: Required, must be >= 0
- `reorderThreshold`: Required, must be >= 0

**Response:** Created product with SID (201 Created)

#### 4. Update Product
**Endpoint:** `PUT /api/products/update/{productSid}`

**Request Body:** Same as Add

**Response:** Updated product object

#### 5. Delete Product (Soft Delete)
**Endpoint:** `DELETE /api/products/delete/{productSid}`

**Response:** 204 No Content

#### 6. Get Low Stock Products
**Endpoint:** `GET /api/products/low-stock`

**Query Parameters:** Same as Categories (default sort by CurrentStock ASC)

**Response:** Products where currentStock <= reorderThreshold

---

### Suppliers

#### 1. Get Suppliers List
**Endpoint:** `GET /api/suppliers/list`

**Query Parameters:** Same as Categories

**Response:**
```json
{
  "meta": {...},
  "results": [
    {
      "supplierSid": "SUP1A2B3C4D5E6F7",
      "supplierName": "Tech Supplier Inc",
      "contactEmail": "contact@techsupplier.com",
      "phone": "+1-800-123-4567",
      "address": "123 Business St, Tech City, TC 12345",
      "status": 1,
      "createdAt": "2024-01-15T10:30:00Z",
      "lastModifiedAt": "2024-01-15T10:30:00Z"
    }
  ]
}
```

#### 2. Get Supplier by SID
**Endpoint:** `GET /api/suppliers/{supplierSid}`

**Response:** Single supplier object

#### 3. Add New Supplier
**Endpoint:** `POST /api/suppliers/add`

**Request Body:**
```json
{
  "supplierName": "Tech Supplier Inc",
  "contactEmail": "contact@techsupplier.com",
  "phone": "+1-800-123-4567",
  "address": "123 Business St, Tech City, TC 12345"
}
```

**Validation:**
- `supplierName`: Required, max 200 characters
- `contactEmail`: Optional, valid email format, max 200 characters
- `phone`: Optional, max 20 characters
- `address`: Optional, max 500 characters

**Response:** Created supplier with SID (201 Created)

#### 4. Update Supplier
**Endpoint:** `PUT /api/suppliers/update/{supplierSid}`

**Request Body:** Same as Add

**Response:** Updated supplier object

#### 5. Delete Supplier (Soft Delete)
**Endpoint:** `DELETE /api/suppliers/delete/{supplierSid}`

**Response:** 204 No Content

---

### Users

#### 1. Get Users List
**Endpoint:** `GET /api/users/list`

**Query Parameters:** Same as Categories

**Response:**
```json
{
  "meta": {...},
  "results": [
    {
      "userSid": "USR1A2B3C4D5E6F7",
      "fullName": "John Doe",
      "email": "john.doe@example.com",
      "role": "Admin",
      "status": 1,
      "createdAt": "2024-01-15T10:30:00Z",
      "lastModifiedAt": "2024-01-15T10:30:00Z"
    }
  ]
}
```

#### 2. Get User by SID
**Endpoint:** `GET /api/users/{userSid}`

**Response:** Single user object

#### 3. Get User by Email
**Endpoint:** `GET /api/users/email/{email}`

**Response:** Single user object or 404 if not found

#### 4. Add New User
**Endpoint:** `POST /api/users/add`

**Request Body:**
```json
{
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "role": "Admin"
}
```

**Validation:**
- `fullName`: Required, max 200 characters
- `email`: Required, valid email format, max 200 characters, must be unique
- `role`: Required, max 50 characters

**Response:** Created user with SID (201 Created)

#### 5. Update User
**Endpoint:** `PUT /api/users/update/{userSid}`

**Request Body:** Same as Add

**Response:** Updated user object

#### 6. Delete User (Soft Delete)
**Endpoint:** `DELETE /api/users/delete/{userSid}`

**Response:** 204 No Content

---

### Stock Transactions

#### 1. Get Stock Transactions List
**Endpoint:** `GET /api/stocktransactions/list`

**Query Parameters:** Same as Categories (default sort by TransactionDate DESC)

**Response:**
```json
{
  "meta": {...},
  "results": [
    {
      "stockTransactionSid": "STX1A2B3C4D5E6F7",
      "productSid": "PRD1A2B3C4D5E6F7",
      "productName": "Laptop",
      "transactionType": "IN",
      "quantity": 10,
      "notes": "Received from supplier",
      "transactionDate": "2024-01-15T14:30:00Z",
      "status": 1,
      "createdAt": "2024-01-15T14:30:00Z",
      "lastModifiedAt": "2024-01-15T14:30:00Z"
    }
  ]
}
```

#### 2. Get Transaction by SID
**Endpoint:** `GET /api/stocktransactions/{transactionSid}`

**Response:** Single transaction object

#### 3. Add Stock Transaction (IN or OUT)
**Endpoint:** `POST /api/stocktransactions/add`

**Request Body:**
```json
{
  "productSid": "PRD1A2B3C4D5E6F7",
  "transactionType": "IN",
  "quantity": 10,
  "notes": "Received from supplier"
}
```

**Validation:**
- `productSid`: Required
- `transactionType`: Required, must be "IN" or "OUT"
- `quantity`: Required, must be > 0
- `notes`: Optional, max 500 characters

**Response:** Created transaction with updated product stock (201 Created)

**Note:** 
- Automatically updates product's `currentStock`
- For "OUT" transactions, validates sufficient stock available

#### 4. Get Product Transaction History
**Endpoint:** `GET /api/stocktransactions/product/{productSid}/history`

**Query Parameters:** Same as Categories

**Response:** List of all transactions for a specific product

---

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 204 | No Content - Successful deletion |
| 400 | Bad Request - Invalid input or business logic error |
| 404 | Not Found - Resource not found |
| 500 | Internal Server Error - Server error |

## Error Response Format

```json
{
  "error": "Error message describing what went wrong"
}
```

## Status Values

- `1` = Active
- `3` = Deleted (Soft Delete)

## SID Format

SIDs are generated with prefixes:
- Categories: `CAT` + 13 random alphanumeric characters
- Products: `PRD` + 13 random alphanumeric characters
- Suppliers: `SUP` + 13 random alphanumeric characters
- Users: `USR` + 13 random alphanumeric characters
- Stock Transactions: `STX` + 13 random alphanumeric characters

Example: `CAT1A2B3C4D5E6F7`

## Search & Filter Example

```
GET /api/products/list?page=1&pageSize=10&searchText=laptop&sortColumn=ProductName&sortOrder=ASC
```

### Filter JSON Example
```json
[
  {
    "key": "CategoryName",
    "value": "Electronics",
    "condition": "="
  },
  {
    "key": "CurrentStock",
    "value": "10",
    "condition": "<"
  }
]
```

Pass as query parameter: `filters=[{"key":"CategoryName","value":"Electronics","condition":"="}]`

---

## Database Connection

Connection string should be configured in `appsettings.json`:
```json
{
  "DbConnectionString": "Server=YOUR_SERVER;Database=ProductInventoryTracker;Integrated Security=True;TrustServerCertificate=True;"
}
```
