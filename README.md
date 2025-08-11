# Legal-IA 🏛️⚖️

**AI-Powered Legal Document Generation System for Spanish Legal Professionals** 🚀🤖

Legal-IA is a sophisticated Azure Functions-based microservice that leverages artificial intelligence to generate, manage, and process legal documents specifically tailored for the Spanish legal system. The platform automates the creation of complex legal documents including invoices, tax returns, contracts, and regulatory forms while ensuring compliance with Spanish legal requirements. 🇪🇸📄

## 🎯 **Purpose & Vision**

Legal-IA addresses the critical need for automated legal document generation in Spain's complex bureaucratic landscape. The system:

- ✍️ **Automates Document Creation**: Uses AI to generate legal documents based on user prompts and context
- 🛡️ **Ensures Compliance**: Templates and validation rules aligned with Spanish legal requirements
- ⚡ **Streamlines Workflows**: From document creation to submission and archival
- ❌ **Reduces Manual Errors**: AI-driven content generation with built-in validation
- 📈 **Scales Efficiently**: Cloud-native architecture supporting high-volume document processing

## 🏗️ **Architecture Overview**

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Azure Functions  │    │   PostgreSQL DB   │    │   Azure Blob     │
│   (API Gateway)    │◄──►│   (Metadata)      │    │   (PDF Storage)  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                        │                        │
         ▼                        ▼                        ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Redis Cache     │    │   AI Service      │    │   Validation     │
│   (Performance)   │    │   (Content Gen)   │    │   (FluentVal)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### **Tech Stack** 🧰
- 🟦 **.NET 8** - Modern C# runtime
- ☁️ **Azure Functions V4** - Serverless compute platform
- 🐘 **PostgreSQL** - Primary database for metadata
- 🧠 **Redis** - High-performance caching layer
- 📦 **Azure Blob Storage** - Document file storage
- 🗃️ **Entity Framework Core** - ORM for data access
- ✅ **FluentValidation** - Input validation framework

## 📋 **Core Features**

### 🤖 **AI Document Generation**
- 📝 Generate legal documents using natural language prompts
- 📚 Support for 13+ Spanish legal document types
- 🧠 Context-aware content generation
- 🖨️ PDF output with proper formatting

### 👥 **User Management**
- 🆔 Spanish citizen identification (DNI/CIF validation)
- 🏢 Business entity management
- 🛡️ Role-based access control
- 📊 User activity tracking

### 📄 **Document Lifecycle Management**
- 🔄 Draft → In Progress → Generated → Submitted → Approved/Rejected → Archived
- 🗂️ Version control and document history
- 🧩 Template management for reusable documents
- 📦 Batch processing capabilities

### 🔍 **Search & Discovery**
- 🔎 Full-text search across documents
- 🗃️ Filter by type, status, date, amount
- 👤 User-specific document collections
- 📑 Template library access

## 📚 **Supported Document Types**

| 📄 Document Type | 🇪🇸 Spanish Name | 💼 Use Case |
|------------------|-----------------|------------|
| **Invoice** | Factura | Business invoicing |
| **Expense** | Gasto | Expense reporting |
| **VAT Return** | Declaración de IVA | Tax compliance |
| **IRPF Return** | Declaración de IRPF | Income tax filing |
| **Income Statement** | Declaración de Ingresos | Revenue reporting |
| **Social Security Form** | Formulario Seguridad Social | Employment compliance |
| **Expense Report** | Informe de Gastos | Business expense tracking |
| **Contract** | Contrato | Legal agreements |
| **Receipt** | Recibo | Payment confirmations |
| **Tax Form** | Formulario Fiscal | General tax documents |
| **Business Plan** | Plan de Negocio | Business documentation |
| **Legal Document** | Documento Legal | General legal papers |

## 🛠️ **Services Architecture**

### **Core Services**

#### 🤖 **AIDocumentGenerationService**
- **Purpose**: AI-powered document content generation
- **Key Methods**:
  - `GenerateDocumentAsync()` - Create new AI documents
- **Features**: Prompt processing, content generation, PDF conversion

#### 📄 **DocumentService**
- **Purpose**: Document lifecycle management
- **Key Methods**:
  - `CreateDocumentAsync()` - Create new documents
  - `UpdateDocumentAsync()` - Modify existing documents
  - `SearchDocumentsAsync()` - Search and filter documents
  - `UpdateDocumentFileInfoAsync()` - Update file metadata
- **Features**: CRUD operations, caching, status management

#### 👤 **UserService**
- **Purpose**: User account and profile management
- **Key Methods**:
  - `CreateUserAsync()` - Register new users
  - `UpdateUserAsync()` - Update user profiles
  - `GetUserAsync()` - Retrieve user information
- **Features**: Spanish ID validation, business entity support

#### 📦 **FileStorageService**
- **Purpose**: Document file storage and retrieval
- **Key Methods**:
  - `SaveDocumentBytesAsync()` - Store PDF files
  - `GetDocumentBytesAsync()` - Retrieve stored files
  - `GetDocumentMetadataAsync()` - File information
- **Features**: Azure Blob integration, metadata management

#### 🧠 **CacheService**
- **Purpose**: Performance optimization through caching
- **Key Methods**:
  - `GetAsync<T>()` - Retrieve cached data
  - `SetAsync<T>()` - Store data in cache
  - `RemoveAsync()` - Invalidate cache entries
- **Features**: Redis integration, pattern-based invalidation

#### 📢 **NotificationService**
- **Purpose**: User communication and alerts
- **Key Methods**:
  - `SendDocumentGenerationNotificationAsync()` - Generation alerts
  - `SendDocumentStatusChangeNotificationAsync()` - Status updates
- **Features**: Multi-channel notifications, event-driven messaging

## 🌐 **API Endpoints**

### **User Management**

| 🛠️ Method | 🔗 Endpoint      | 📝 Description                                      |
|-----------|-----------------|----------------------------------------------------|
| `POST`    | `/api/users`    | Create new user account                            |
| `GET`     | `/api/users/{id}` | Get user by ID                                   |
| `PATCH`   | `/user/me`      | Partially update current user's profile (JWT only)  |
| `DELETE`  | `/api/users/{id}` | Deactivate user account                          |
| `GET`     | `/api/users`    | List all users (admin)                             |

> **Note:**
> - The `PATCH /user/me` endpoint requires a valid JWT in the `Authorization` header. The userId is extracted from the token, so users can only update their own profile.
> - All PATCH endpoints accept only the fields to be updated (partial updates), not the full object.

#### Example PATCH Request (User)
```http
PATCH /user/me
Authorization: Bearer <your-jwt-token>
Content-Type: application/json

{
  "FirstName": "Ana",
  "Phone": "+34 600 123 456"
}
```
This will only update the user's first name and phone number, leaving all other fields unchanged.

# 🧾 Invoices & Invoice Items Module

## Why We Added This

Spanish autonomous professionals ("autónomos") are required by law to issue invoices for their services, including specific details such as VAT (IVA), IRPF (retención), and itemized breakdowns. To support this, Legal-IA now includes:
- 📄 **Invoice model**: Captures all required fields for Spanish invoices.
- 🧾 **InvoiceItem model**: Allows itemized details per invoice, supporting legal compliance.
- 🏗️ **Repository, orchestrators, and activities**: Ensure robust, scalable, and maintainable CRUD operations.
- 🛡️ **Role-based access**: Only authorized users can manage invoices/items.

## Endpoints

All endpoints are protected by JWT. 🔒

### Invoice Endpoints

- `GET /invoices/user` — Get invoices for current user
  - ✅ **200 OK**: Returns a list of invoices
  - 🚫 **404 Not Found**: No invoices found
  - ❗ **400 Bad Request**: Invalid or missing UserId in JWT
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `POST /invoices/user` — Create invoice for current user
  - ✅ **200 OK**: Returns the created invoice
  - ❗ **400 Bad Request**: Invalid request body or missing UserId in JWT
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `PATCH /invoices/user/{id}` — Update invoice for current user
  - ✅ **200 OK**: Returns the updated invoice
  - ❗ **400 Bad Request**: Invalid ID, request body, or missing UserId in JWT
  - 🚫 **404 Not Found**: Invoice not found or does not belong to user
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `DELETE /invoices/user/{id}` — Delete invoice for current user
  - ✅ **200 OK**: Invoice deleted
  - ❗ **400 Bad Request**: Invalid ID format or missing UserId in JWT
  - 🚫 **404 Not Found**: Invoice not found or does not belong to user
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error

### Invoice Item Endpoints

- `GET /invoice-items/user` — Get invoice items for current user
  - ✅ **200 OK**: Returns a list of invoice items
  - 🚫 **404 Not Found**: No invoice items found
  - ❗ **400 Bad Request**: Invalid or missing UserId in JWT
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `POST /invoice-items/user` — Create invoice item for current user
  - ✅ **200 OK**: Returns the created invoice item
  - ❗ **400 Bad Request**: Invalid request body or missing UserId in JWT
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `PATCH /invoice-items/user/{id}` — Update invoice item for current user
  - ✅ **200 OK**: Returns the updated invoice item
  - ❗ **400 Bad Request**: Invalid ID, request body, or missing UserId in JWT
  - 🚫 **404 Not Found**: Invoice item not found or does not belong to user
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `DELETE /invoice-items/user/{id}` — Delete invoice item for current user
  - ✅ **200 OK**: Invoice item deleted
  - ❗ **400 Bad Request**: Invalid ID format or missing UserId in JWT
  - 🚫 **404 Not Found**: Invoice item not found or does not belong to user
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error

## Cache Invalidation Logic
- Whenever you create, update, or delete an invoice item, the cache for both invoice items and invoices for the specific user is now invalidated:
  - `invoiceitems:user:{userId}` and `invoices:user:{userId}` are both cleared.
  - This ensures users always see the latest data after any CRUD operation on invoice items or invoices.
- The cache is invalidated by fetching the parent invoice to get the correct userId, not by relying on navigation properties.

## Invoice & Invoice Item Caching

- **Per-user cache keys:**  
  - Invoices: `invoices:user:{userId}`
  - Invoice items: `invoiceitems:user:{userId}`
- **Cache invalidation:**  
  - On any create, update, or delete of an invoice item, both the invoice items and invoices cache for the affected user are cleared.
  - This guarantees that users always see the latest invoice and item data after any change.

## ⚡ PATCH-Only Update Semantics

- All update operations for User, Invoice, and InvoiceItem now use PATCH (partial update) semantics.
- PATCH endpoints accept only the fields to be updated (partial updates), not the full object.
- All orchestrators, activities, and HTTP triggers for update operations are named with the `Patch*` prefix (e.g., PatchUser, PatchInvoice, PatchInvoiceItem).
- Deprecated update code and PUT endpoints have been removed for clarity and maintainability.

### Example PATCH Request (User)
```http
PATCH /api/users/123e4567-e89b-12d3-a456-426614174000
Content-Type: application/json

{
  "FirstName": "Ana",
  "Phone": "+34 600 123 456"
}
```
This will only update the user's first name and phone number, leaving all other fields unchanged.
