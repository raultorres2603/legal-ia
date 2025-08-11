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

### **User Management** (`/api/users`) 👥

| 🛠️ Method | 🔗 Endpoint | 📝 Description |
|--------|----------|-------------|
| `POST` | `/api/users` | Create new user account |
| `GET` | `/api/users/{id}` | Get user by ID |
| `PATCH` | `/api/users/{id}` | Partially update user profile (only provided fields are updated) |
| `DELETE` | `/api/users/{id}` | Deactivate user account |
| `GET` | `/api/users` | List all users (admin) |

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

#### 🛡️ Admin Only
- `GET /invoices` — Get all invoices
  - ✅ **200 OK**: Returns a list of invoices
  - 🚫 **404 Not Found**: No invoices found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `GET /invoices/users/{id}` — Get invoice by user ID
  - ✅ **200 OK**: Returns the invoice
  - 🚫 **404 Not Found**: Invoice not found
  - ❗ **400 Bad Request**: Invalid ID format
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `POST /invoices` — Create invoice
  - ✅ **200 OK**: Returns the created invoice
  - ❗ **400 Bad Request**: Invalid request body
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `PATCH /invoices/users/{id}` — Partially update invoice by user ID
  - ✅ **200 OK**: Returns the updated invoice
  - ❗ **400 Bad Request**: Invalid ID or request body
  - 🚫 **404 Not Found**: Invoice not found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `DELETE /invoices/users/{id}` — Delete invoice by user ID
  - ✅ **200 OK**: Invoice deleted
  - ❗ **400 Bad Request**: Invalid ID format
  - 🚫 **404 Not Found**: Invoice not found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error

#### 👤 User Only
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

### Invoice Item Endpoints

#### 🛡️ Admin Only
- `GET /invoice-items/users` — Get all invoice items
  - ✅ **200 OK**: Returns a list of invoice items
  - 🚫 **404 Not Found**: No invoice items found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `GET /invoice-items/users/{id}` — Get invoice item by ID
  - ✅ **200 OK**: Returns the invoice item
  - 🚫 **404 Not Found**: Invoice item not found
  - ❗ **400 Bad Request**: Invalid ID format
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `POST /invoice-items` — Create invoice item
  - ✅ **200 OK**: Returns the created invoice item
  - ❗ **400 Bad Request**: Invalid request body
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `PATCH /invoice-items/users/{id}` — Update invoice item by ID
  - ✅ **200 OK**: Returns the updated invoice item
  - ❗ **400 Bad Request**: Invalid ID or request body
  - 🚫 **404 Not Found**: Invoice item not found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `DELETE /invoice-items/users/{id}` — Delete invoice item by ID
  - ✅ **200 OK**: Invoice item deleted
  - ❗ **400 Bad Request**: Invalid ID format
  - 🚫 **404 Not Found**: Invoice item not found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error

#### 👤 User Only
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

## How to Test 🧪

1. 🔑 **Obtain a JWT token** by logging in as a user with the `User` or `Admin` role.
2. 🧪 **Use Postman or curl** to call the endpoints above, including the `Authorization: Bearer <token>` header.
3. 👤 **For user endpoints**, the UserId is automatically extracted from the JWT.
4. 🛡️ **For admin endpoints**, use a token with the Admin role.
5. 📊 **Verify business logic**: VAT, IRPF, and totals are calculated and stored as per Spanish legislation.
6. 🛡️ **Check role-based access**: Only users with the correct role can access these endpoints.

For more details, see the orchestrator and activity implementations in `/Functions/Orchestrators/` and `/Functions/Activities/`.

---

## 📊 **Data Models**

### 👤 **User Entity**
```csharp
public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string DNI { get; set; }          // Spanish National ID
    public string CIF { get; set; }          // Business Tax ID
    public string BusinessName { get; set; }
    public string Address { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public string Province { get; set; }
    public string Phone { get; set; }
    public bool IsActive { get; set; }
    // ... timestamps
}
```

## 🔧 **Development Setup**

### **Prerequisites**
- 🟦 .NET 8 SDK
- 🐳 Docker & Docker Compose
- ☁️ Azure Functions Core Tools
- 🐘 PostgreSQL client (optional)

### **Quick Start**

1. 📥 **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Legal-IA
   ```

2. 🏁 **Start infrastructure services**
   ```bash
   docker-compose up -d
   ```
   This starts:
   - 🐘 PostgreSQL (port 5433)
   - 🧠 Redis (port 6380)
   - 📦 Azurite (blob storage on port 10000)

3. ⚙️ **Configure connection strings**
   Update `local.settings.json`:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;...",
       "ConnectionStrings__DefaultConnection": "Host=localhost;Port=5433;Database=LegalIA;Username=postgres;Password=password",
       "ConnectionStrings__Redis": "localhost:6380"
     }
   }
   ```

4. 🗄️ **Run database migrations**
   ```bash
   dotnet ef database update
   ```

5. ▶️ **Start the application**
   ```bash
   func start
   ```

6. 🌐 **Access the API**
   - Functions: `http://localhost:7071`
   - Admin interface: `http://localhost:7071/admin`

## 📁 **Project Structure**

```
Legal-IA/
├── 📂 Data/                    # Database context and configuration
├── 📂 DTOs/                    # Data Transfer Objects
├── 📂 Functions/               # Azure Functions (HTTP triggers)
│   ├── 📂 Activities/          # Durable Functions activities
│   ���── 📂 Orchestrators/       # Workflow orchestrators
├── 📂 Interfaces/              # Service and repository contracts
│   ├── 📂 Repositories/        # Data access interfaces
│   └── 📂 Services/            # Business logic interfaces
├── 📂 Models/                  # Domain entities
├── 📂 Repositories/            # Data access implementations
├── 📂 Services/                # Business logic implementations
├── 📂 Validators/              # Input validation rules
├── ��� Migrations/              # EF Core database migrations
└── 📄 Program.cs               # Dependency injection configuration
```

## 🧪 **Testing & API Documentation**

### **Postman Collections** 📦
The project includes **6 comprehensive Postman collections** located in the `/Postman/` directory, providing complete API testing coverage for all functionality:

#### **1. Legal-IA Main Collection** 📋
*Complete API collection for core functionality testing*

**Features:**
- 🤖 **AI Document Generation** (4 endpoints)
  - `POST /ai/documents/generate` - Generate documents from natural language prompts
  - `POST /ai/documents/{id}/regenerate` - Update documents with new prompts
  - `GET /ai/documents/{id}/status` - Check generation progress
  - `GET /ai/documents/{id}/download` - Download generated PDFs
- 👥 User Management (5 endpoints) - Complete CRUD with Spanish validation
- 📄 Document Management (9 endpoints) - Traditional document operations
- 🧠 Smart Variables - Automatic ID capture and chaining between requests
- 🛡️ Global Test Scripts - Performance and validation assertions

#### **2. Spanish Bureaucratic Documents** 🇪🇸
*Specialized collection for AI generation of Spanish legal documents*

**Document Categories:**
- 💰 Facturas (Invoices) - Service and product invoices with Spanish tax compliance
  - Professional service invoices with IRPF 15% withholding
  - Product sales invoices with IVA 21% calculations
- 📊 Declaraciones Fiscales (Tax Returns)
  - Quarterly VAT returns (Modelo 303)
  - Annual IRPF declarations with deductions
- 🏢 Contratos (Contracts)
  - Technology service agreements
  - Office rental contracts (Ley de Arrendamientos Urbanos)
- 📋 Seguridad Social - RETA registration forms for autonomous workers
- 📊 Informes de Gastos - Deductible expense reports with tax analysis
- 📋 Recibos y Otros - Professional receipts and business plans

**Spanish Legal Compliance:**
- 🆔 Proper DNI/CIF format validation
- 💸 Spanish tax calculations (IVA, IRPF, Social Security)
- 🏛️ Compliance with Spanish bureaucratic requirements
- 🏢 Real-world scenarios for consultants and businesses

#### **3. User Management** 👥
*Dedicated collection for user account operations*

**Features:**
- 🆔 User registration with Spanish legal validation
- 👤 Profile management and updates
- 🆔 Spanish identification validation (DNI, CIF)
- 🏠 Address and contact information handling
- 🏢 Business entity support

#### **4. Document Management** 📄
*Traditional document CRUD operations*

**Features:**
- 📝 Document creation, reading, updating, deletion
- 🗃️ Document filtering by type, status, user
- 🧩 Template management
- 🔎 Search functionality
- 🗂️ File metadata handling

#### **5. Validation & Edge Cases** ⚠️
*Comprehensive error handling and security testing*

**Test Categories:**
- 🤖 AI Generation Validation
  - Invalid user IDs, empty prompts, prompt limits
  - Invalid document types, non-existent documents
  - Context length validation, regeneration errors
- 👥 Spanish Legal Validation
  - DNI format: `12345678Z` pattern validation
  - CIF format: `B12345678` pattern validation
  - Spanish postal codes: `28001` format validation
  - Spanish phone numbers: `+34912345678` format validation
- 🔍 Security Testing
  - SQL injection prevention
  - XSS attack prevention
  - Data sanitization validation
- 🚀 Performance Testing
  - Concurrent AI generation load testing
  - Bulk document operations
  - Response time benchmarks

#### **6. Workflow Testing** 🔄
*End-to-end process validation*

**Features:**
- 🔄 Complete user-to-document workflows
- 🤖 AI generation process validation
- 📝 Multi-step document creation scenarios
- 🔗 Integration testing between services

### **Collection Usage Guide** 📚

#### **Getting Started**
1. 📥 Import Collections - Import all `.postman_collection.json` files from `/Postman/` directory
2. ⚙️ Set Environment - Configure base URL: `http://localhost:7071`
3. 🏁 Start Infrastructure - Run `docker-compose up -d` for databases
4. ▶️ Run Application - Execute `func start` to launch Azure Functions
5. 🧪 Execute Tests - Run collections individually or as a suite

#### **Recommended Testing Flow**
```
1. Start with "Legal-IA Main Collection" → Test core functionality
2. Use "User Management" → Create test users with Spanish data
3. Run "Spanish Bureaucratic Documents" → Test realistic legal scenarios
4. Execute "Validation & Edge Cases" → Ensure robust error handling
5. Perform "Workflow Testing" → Validate end-to-end processes
```

#### **Smart Features**
- 🧠 Automatic Variable Management - Document IDs captured from responses
- 🔗 Chained Requests - Seamless flow between related operations
- 🇪🇸 Realistic Test Data - Spanish legal scenarios with proper formatting
- ⚡ Performance Monitoring - Built-in response time validation
- 🛡️ Security Validation - Automated security testing assertions

### **Test Scenarios Coverage**

#### 🇪🇸 Spanish Legal Compliance Testing
1. 🆔 DNI/CIF Validation - Spanish identification number formats
2. 💸 Tax Calculations - IVA 21%, IRPF withholdings, Social Security
3. 📄 Legal Document Formats - Compliance with Spanish regulations
4. 🏢 Business Rules - Autonomous worker requirements, contract law

#### 🤖 AI Document Generation Testing
1. 📝 Natural Language Processing - Prompt-based document creation
2. 📚 Document Types - All 13 supported Spanish legal document types
3. 🧠 Content Quality - AI-generated content validation
4. 🖨️ PDF Generation - File creation and blob storage integration

#### 🛡️ Security & Performance Testing
1. ✅ Input Validation - Comprehensive field validation testing
2. 🛡️ SQL Injection Prevention - Database security validation
3. 🚀 Load Testing - Concurrent request handling
4. 💥 Error Handling - Graceful failure scenarios

#### 🔗 Integration Testing
1. 🗄️ Database Operations - PostgreSQL integration validation
2. 📦 Blob Storage - Azure Blob Storage file operations
3. 🧠 Caching - Redis cache performance and consistency
4. 🌐 External Services - AI service integration readiness

### **API Documentation** 📚
Each Postman collection includes:
- 📝 Detailed Request Documentation - Clear descriptions and usage examples
- 📦 Response Examples - Expected outputs for all scenarios
- 💥 Error Handling Examples - Common error responses and solutions
- 🇪🇸 Spanish Legal Context - Explanations of regulatory requirements
- ⚡ Performance Benchmarks - Expected response times and limits

The collections serve as both **testing tools** and **comprehensive API documentation**, making them invaluable for developers, testers, and integration partners working with the Legal-IA system.

---

# 🧾 Invoices & Invoice Items Module (Updated)

## Why We Added This

Spanish autonomous professionals ("autónomos") are required by law to issue invoices for their services, including specific details such as VAT (IVA), IRPF (retención), and itemized breakdowns. To support this, Legal-IA now includes:
- 📄 **Invoice model**: Captures all required fields for Spanish invoices.
- 🧾 **InvoiceItem model**: Allows itemized details per invoice, supporting legal compliance.
- 🏗️ **Repository, orchestrators, and activities**: Ensure robust, scalable, and maintainable CRUD operations.
- 🛡️ **Role-based access**: Only authorized users can manage invoices/items.

## Endpoints

All endpoints are protected by JWT. 🔒

### Invoice Endpoints

#### 🛡️ Admin Only
- `GET /invoices` — Get all invoices
  - ✅ **200 OK**: Returns a list of invoices
  - 🚫 **404 Not Found**: No invoices found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `GET /invoices/users/{id}` — Get invoice by user ID
  - ✅ **200 OK**: Returns the invoice
  - 🚫 **404 Not Found**: Invoice not found
  - ❗ **400 Bad Request**: Invalid ID format
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `POST /invoices` — Create invoice
  - ✅ **200 OK**: Returns the created invoice
  - ❗ **400 Bad Request**: Invalid request body
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `PATCH /invoices/users/{id}` — Partially update invoice by user ID
  - ✅ **200 OK**: Returns the updated invoice
  - ❗ **400 Bad Request**: Invalid ID or request body
  - 🚫 **404 Not Found**: Invoice not found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `DELETE /invoices/users/{id}` — Delete invoice by user ID
  - ✅ **200 OK**: Invoice deleted
  - ❗ **400 Bad Request**: Invalid ID format
  - 🚫 **404 Not Found**: Invoice not found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error

#### 👤 User Only
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

### Invoice Item Endpoints

#### 🛡️ Admin Only
- `GET /invoice-items/users` — Get all invoice items
  - ✅ **200 OK**: Returns a list of invoice items
  - 🚫 **404 Not Found**: No invoice items found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `GET /invoice-items/users/{id}` — Get invoice item by ID
  - ✅ **200 OK**: Returns the invoice item
  - 🚫 **404 Not Found**: Invoice item not found
  - ❗ **400 Bad Request**: Invalid ID format
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `POST /invoice-items` — Create invoice item
  - ✅ **200 OK**: Returns the created invoice item
  - ❗ **400 Bad Request**: Invalid request body
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `PATCH /invoice-items/users/{id}` — Update invoice item by ID
  - ✅ **200 OK**: Returns the updated invoice item
  - ❗ **400 Bad Request**: Invalid ID or request body
  - 🚫 **404 Not Found**: Invoice item not found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error
- `DELETE /invoice-items/users/{id}` — Delete invoice item by ID
  - ✅ **200 OK**: Invoice item deleted
  - ❗ **400 Bad Request**: Invalid ID format
  - 🚫 **404 Not Found**: Invoice item not found
  - 🔒 **401 Unauthorized**: Invalid or missing token
  - 💥 **500 Internal Server Error**: Unexpected error

#### 👤 User Only
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
