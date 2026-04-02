

-- ⚠️ Connect to ProductInventoryTracker before running the rest

---------------------------------------------
-- 1. Categories
---------------------------------------------
CREATE TABLE dbo.Categories (
    CategoryId      INT IDENTITY(1,1)   NOT NULL,
    CategorySid     NVARCHAR(50)        NOT NULL,
    CategoryName    NVARCHAR(100)       NOT NULL,
    [Description]   NVARCHAR(500)       NULL,
    [Status]        INT                 NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2(7)        NOT NULL,
    LastModifiedAt  DATETIME2(7)        NULL,
    CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (CategoryId),
    CONSTRAINT UQ_Categories_Sid UNIQUE (CategorySid)
);

---------------------------------------------
-- 2. Suppliers
---------------------------------------------
CREATE TABLE dbo.Suppliers (
    SupplierId      INT IDENTITY(1,1)   NOT NULL,
    SupplierSid     NVARCHAR(50)        NOT NULL,
    SupplierName    NVARCHAR(200)       NOT NULL,
    ContactEmail    NVARCHAR(200)       NULL,
    Phone           NVARCHAR(20)        NULL,
    [Address]       NVARCHAR(500)       NULL,
    [Status]        INT                 NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2(7)        NOT NULL,
    LastModifiedAt  DATETIME2(7)        NULL,
    CONSTRAINT PK_Suppliers PRIMARY KEY CLUSTERED (SupplierId),
    CONSTRAINT UQ_Suppliers_Sid UNIQUE (SupplierSid)
);

---------------------------------------------
-- 3. Products
---------------------------------------------
CREATE TABLE dbo.Products (
    ProductId         INT IDENTITY(1,1)   NOT NULL,
    ProductSid        NVARCHAR(50)        NOT NULL,
    ProductName       NVARCHAR(200)       NOT NULL,
    SKU               NVARCHAR(50)        NOT NULL,
    [Description]     NVARCHAR(1000)      NULL,
    CategoryId        INT                 NULL,
    SupplierId        INT                 NULL,
    UnitPrice         DECIMAL(18,2)       NOT NULL,
    CurrentStock      INT                 NOT NULL DEFAULT 0,
    ReorderThreshold  INT                 NOT NULL DEFAULT 10,
    [Status]          INT                 NOT NULL DEFAULT 1,
    CreatedAt         DATETIME2(7)        NOT NULL ,
    LastModifiedAt    DATETIME2(7)        NULL,
    CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (ProductId),
    CONSTRAINT UQ_Products_Sid UNIQUE (ProductSid),
    CONSTRAINT UQ_Products_SKU UNIQUE (SKU),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId)
        REFERENCES dbo.Categories (CategoryId),
    CONSTRAINT FK_Products_Suppliers FOREIGN KEY (SupplierId)
        REFERENCES dbo.Suppliers (SupplierId),
    CONSTRAINT CK_Products_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT CK_Products_CurrentStock CHECK (CurrentStock >= 0),
    CONSTRAINT CK_Products_ReorderThreshold CHECK (ReorderThreshold >= 0)
);

---------------------------------------------
-- 4. StockTransactions
---------------------------------------------
CREATE TABLE dbo.StockTransactions (
    StockTransactionId   INT IDENTITY(1,1)   NOT NULL,
    StockTransactionSid  NVARCHAR(50)        NOT NULL,
    ProductId            INT                 NOT NULL,
    TransactionType      NVARCHAR(10)        NOT NULL,
    Quantity             INT                 NOT NULL,
    Notes                NVARCHAR(500)       NULL,
    TransactionDate      DATETIME2(7)        NOT NULL DEFAULT GETDATE(),
    [Status]             INT                 NOT NULL DEFAULT 1,
    CreatedAt            DATETIME2(7)        NOT NULL,
    LastModifiedAt       DATETIME2(7)         NULL,
    CONSTRAINT PK_StockTransactions PRIMARY KEY CLUSTERED (StockTransactionId),
    CONSTRAINT UQ_StockTransactions_Sid UNIQUE (StockTransactionSid),
    CONSTRAINT FK_StockTransactions_Products FOREIGN KEY (ProductId)
        REFERENCES dbo.Products (ProductId),
    CONSTRAINT CK_StockTransactions_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_StockTransactions_Type CHECK (TransactionType IN (N'IN', N'OUT'))
);

---------------------------------------------
-- 5. Users
---------------------------------------------
CREATE TABLE dbo.Users (
    UserId          INT IDENTITY(1,1)   NOT NULL,
    UserSid         NVARCHAR(50)        NOT NULL,
    FullName        NVARCHAR(200)       NOT NULL,
    Email           NVARCHAR(200)       NOT NULL,
    [Role]          NVARCHAR(50)        NOT NULL DEFAULT N'User',
    [Status]        INT                 NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2(7)        NOT NULL,
    LastModifiedAt  DATETIME2(7)        NULL,
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId),
    CONSTRAINT UQ_Users_Sid UNIQUE (UserSid),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);