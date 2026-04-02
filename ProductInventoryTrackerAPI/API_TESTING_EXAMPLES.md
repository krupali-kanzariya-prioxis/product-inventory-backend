# API Testing Examples

## Base URL
```
http://localhost:PORT/api
```

---

## Categories API

### 1. Add New Category
```bash
curl -X POST "http://localhost:5000/api/categories/add" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryName": "Electronics",
    "description": "Electronic devices and gadgets"
  }'
```

### 2. Get All Categories (Paginated)
```bash
curl "http://localhost:5000/api/categories/list?page=1&pageSize=10&searchText=&sortColumn=CreatedAt&sortOrder=DESC"
```

### 3. Get Category by SID
```bash
curl "http://localhost:5000/api/categories/CAT1A2B3C4D5E6F7"
```

### 4. Update Category
```bash
curl -X PUT "http://localhost:5000/api/categories/update/CAT1A2B3C4D5E6F7" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryName": "Electronics & Gadgets",
    "description": "Updated description"
  }'
```

### 5. Delete Category (Soft Delete)
```bash
curl -X DELETE "http://localhost:5000/api/categories/delete/CAT1A2B3C4D5E6F7"
```

---

## Products API

### 1. Add New Product
```bash
curl -X POST "http://localhost:5000/api/products/add" \
  -H "Content-Type: application/json" \
  -d '{
    "productName": "Dell Laptop",
    "sku": "DELL-LAP-001",
    "description": "High-performance laptop with 16GB RAM",
    "categorySid": "CAT1A2B3C4D5E6F7",
    "supplierSid": "SUP1A2B3C4D5E6F7",
    "unitPrice": 1299.99,
    "currentStock": 20,
    "reorderThreshold": 5
  }'
```

### 2. Get All Products
```bash
curl "http://localhost:5000/api/products/list?page=1&pageSize=10&searchText=laptop"
```

### 3. Get Product by SID
```bash
curl "http://localhost:5000/api/products/PRD1A2B3C4D5E6F7"
```

### 4. Update Product
```bash
curl -X PUT "http://localhost:5000/api/products/update/PRD1A2B3C4D5E6F7" \
  -H "Content-Type: application/json" \
  -d '{
    "productName": "Dell Laptop XPS",
    "sku": "DELL-LAP-002",
    "description": "Updated description",
    "categorySid": "CAT1A2B3C4D5E6F7",
    "supplierSid": "SUP1A2B3C4D5E6F7",
    "unitPrice": 1399.99,
    "currentStock": 15,
    "reorderThreshold": 5
  }'
```

### 5. Delete Product
```bash
curl -X DELETE "http://localhost:5000/api/products/delete/PRD1A2B3C4D5E6F7"
```

### 6. Get Low Stock Products
```bash
curl "http://localhost:5000/api/products/low-stock?page=1&pageSize=10&sortColumn=CurrentStock&sortOrder=ASC"
```

---

## Suppliers API

### 1. Add New Supplier
```bash
curl -X POST "http://localhost:5000/api/suppliers/add" \
  -H "Content-Type: application/json" \
  -d '{
    "supplierName": "Tech Solutions Inc",
    "contactEmail": "contact@techsolutions.com",
    "phone": "+1-800-TECH-911",
    "address": "123 Tech Boulevard, Silicon Valley, CA 94000"
  }'
```

### 2. Get All Suppliers
```bash
curl "http://localhost:5000/api/suppliers/list?page=1&pageSize=10"
```

### 3. Get Supplier by SID
```bash
curl "http://localhost:5000/api/suppliers/SUP1A2B3C4D5E6F7"
```

### 4. Update Supplier
```bash
curl -X PUT "http://localhost:5000/api/suppliers/update/SUP1A2B3C4D5E6F7" \
  -H "Content-Type: application/json" \
  -d '{
    "supplierName": "Tech Solutions Global",
    "contactEmail": "support@techsolutions.com",
    "phone": "+1-800-TECH-911",
    "address": "456 Innovation Drive, San Francisco, CA 94000"
  }'
```

### 5. Delete Supplier
```bash
curl -X DELETE "http://localhost:5000/api/suppliers/delete/SUP1A2B3C4D5E6F7"
```

---

## Users API

### 1. Add New User
```bash
curl -X POST "http://localhost:5000/api/users/add" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe",
    "email": "john.doe@company.com",
    "role": "Admin"
  }'
```

### 2. Get All Users
```bash
curl "http://localhost:5000/api/users/list?page=1&pageSize=10"
```

### 3. Get User by SID
```bash
curl "http://localhost:5000/api/users/USR1A2B3C4D5E6F7"
```

### 4. Get User by Email
```bash
curl "http://localhost:5000/api/users/email/john.doe@company.com"
```

### 5. Update User
```bash
curl -X PUT "http://localhost:5000/api/users/update/USR1A2B3C4D5E6F7" \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "John Doe Smith",
    "email": "john.smith@company.com",
    "role": "Manager"
  }'
```

### 6. Delete User
```bash
curl -X DELETE "http://localhost:5000/api/users/delete/USR1A2B3C4D5E6F7"
```

---

## Stock Transactions API

### 1. Add Stock Transaction (Stock IN)
```bash
curl -X POST "http://localhost:5000/api/stocktransactions/add" \
  -H "Content-Type: application/json" \
  -d '{
    "productSid": "PRD1A2B3C4D5E6F7",
    "transactionType": "IN",
    "quantity": 25,
    "notes": "Received shipment from supplier"
  }'
```

### 2. Add Stock Transaction (Stock OUT)
```bash
curl -X POST "http://localhost:5000/api/stocktransactions/add" \
  -H "Content-Type: application/json" \
  -d '{
    "productSid": "PRD1A2B3C4D5E6F7",
    "transactionType": "OUT",
    "quantity": 5,
    "notes": "Sold to customer"
  }'
```

### 3. Get All Stock Transactions
```bash
curl "http://localhost:5000/api/stocktransactions/list?page=1&pageSize=10&sortColumn=TransactionDate&sortOrder=DESC"
```

### 4. Get Transaction by SID
```bash
curl "http://localhost:5000/api/stocktransactions/STX1A2B3C4D5E6F7"
```

### 5. Get Product Transaction History
```bash
curl "http://localhost:5000/api/stocktransactions/product/PRD1A2B3C4D5E6F7/history?page=1&pageSize=20"
```

---

## Search & Filter Examples

### Search with Text
```bash
curl "http://localhost:5000/api/products/list?page=1&pageSize=10&searchText=laptop&sortColumn=ProductName&sortOrder=ASC"
```

### Pagination with Sort
```bash
curl "http://localhost:5000/api/categories/list?page=2&pageSize=5&sortColumn=CreatedAt&sortOrder=DESC"
```

### Advanced Filter (JSON)
```bash
# Note: Replace URL-encoded version
curl "http://localhost:5000/api/products/list?page=1&pageSize=10&filters=%5B%7B%22key%22%3A%22Status%22%2C%22value%22%3A%221%22%2C%22condition%22%3A%22%3D%22%7D%5D"

# Original filter JSON:
# [{"key":"Status","value":"1","condition":"="}]
```

---

## PowerShell Examples

### Add Category (PowerShell)
```powershell
$body = @{
    categoryName = "Home & Garden"
    description = "Home and garden products"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5000/api/categories/add" `
  -Method POST `
  -ContentType "application/json" `
  -Body $body
```

### Get Products List (PowerShell)
```powershell
$uri = "http://localhost:5000/api/products/list?page=1&pageSize=10&searchText=laptop"

Invoke-WebRequest -Uri $uri -Method GET | ConvertFrom-Json
```

---

## Response Examples

### Success Response (200 OK)
```json
{
  "meta": {
    "page": 1,
    "page_size": 10,
    "key": null,
    "url": null,
    "first_page_url": null,
    "previous_page_url": null,
    "next_page_url": null,
    "total_results": 5,
    "total_page_num": 1,
    "extra_data": []
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

### Created Response (201 Created)
```json
{
  "categorySid": "CAT9X8Y7Z6W5V4U3",
  "categoryName": "Electronics",
  "description": "Electronic devices",
  "status": 1,
  "createdAt": "2024-01-15T10:30:00Z",
  "lastModifiedAt": null
}
```

### Error Response (400 Bad Request)
```json
{
  "error": "Category name must not exceed 100 characters."
}
```

### Not Found Response (404)
```json
{
  "error": "Category not found."
}
```

---

## Validation Error Examples

### Product Validation
```bash
# Missing required field
curl -X POST "http://localhost:5000/api/products/add" \
  -H "Content-Type: application/json" \
  -d '{
    "productName": "Laptop",
    "description": "A laptop"
  }'
# Response: 400 - SKU is required.

# Invalid price
curl -X POST "http://localhost:5000/api/products/add" \
  -H "Content-Type: application/json" \
  -d '{
    "productName": "Laptop",
    "sku": "LAP-001",
    "unitPrice": -100,
    "currentStock": 0,
    "reorderThreshold": 0
  }'
# Response: 400 - Unit price must be greater than 0.
```

### Stock Transaction Validation
```bash
# Insufficient stock for OUT transaction
curl -X POST "http://localhost:5000/api/stocktransactions/add" \
  -H "Content-Type: application/json" \
  -d '{
    "productSid": "PRD1A2B3C4D5E6F7",
    "transactionType": "OUT",
    "quantity": 1000,
    "notes": "Trying to sell more than available"
  }'
# Response: 400 - Insufficient stock available.

# Invalid transaction type
curl -X POST "http://localhost:5000/api/stocktransactions/add" \
  -H "Content-Type: application/json" \
  -d '{
    "productSid": "PRD1A2B3C4D5E6F7",
    "transactionType": "INVALID",
    "quantity": 10,
    "notes": "Invalid type"
  }'
# Response: 400 - Transaction type must be either 'IN' or 'OUT'.
```

---

## Important Notes

1. Replace `http://localhost:5000` with your actual API endpoint
2. SIDs in examples are placeholders - use real SIDs from your responses
3. All timestamps are in UTC format (ISO 8601)
4. Status codes: 1 = Active, 3 = Deleted
5. Pagination starts from page 1
6. Maximum page size is 10,000

---

## Debugging Tips

### Enable Verbose Output (curl)
```bash
curl -v "http://localhost:5000/api/categories/list"
```

### Check Response Headers
```bash
curl -i "http://localhost:5000/api/categories/list"
```

### Pretty Print JSON Response (PowerShell)
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/api/categories/list" | ConvertFrom-Json | ConvertTo-Json
```

### Monitor Network Traffic (curl)
```bash
curl --trace - "http://localhost:5000/api/categories/list" 2>&1 | head -50
```
