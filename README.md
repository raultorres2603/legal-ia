# Legal-IA 🏛️⚖️

**AI-Powered Legal Document Generation System for Spanish Legal Professionals**

Legal-IA is a sophisticated Azure Functions-based microservice that leverages artificial intelligence to generate, manage, and process legal documents specifically tailored for the Spanish legal system. The platform automates the creation of complex legal documents including invoices, tax returns, contracts, and regulatory forms while ensuring compliance with Spanish legal requirements.

## 🎯 **Purpose & Vision**

Legal-IA addresses the critical need for automated legal document generation in Spain's complex bureaucratic landscape. The system:

- **Automates Document Creation**: Uses AI to generate legal documents based on user prompts and context
- **Ensures Compliance**: Templates and validation rules aligned with Spanish legal requirements
- **Streamlines Workflows**: From document creation to submission and archival
- **Reduces Manual Errors**: AI-driven content generation with built-in validation
- **Scales Efficiently**: Cloud-native architecture supporting high-volume document processing

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

### **Tech Stack**
- **.NET 8** - Modern C# runtime
- **Azure Functions V4** - Serverless compute platform
- **PostgreSQL** - Primary database for metadata
- **Redis** - High-performance caching layer
- **Azure Blob Storage** - Document file storage
- **Entity Framework Core** - ORM for data access
- **FluentValidation** - Input validation framework

## 📋 **Core Features**

### 🤖 **AI Document Generation**
- Generate legal documents using natural language prompts
- Support for 13+ Spanish legal document types
- Context-aware content generation
- PDF output with proper formatting

### 👥 **User Management**
- Spanish citizen identification (DNI/CIF validation)
- Business entity management
- Role-based access control
- User activity tracking

### 📄 **Document Lifecycle Management**
- Draft → In Progress → Generated → Submitted → Approved/Rejected → Archived
- Version control and document history
- Template management for reusable documents
- Batch processing capabilities

### 🔍 **Search & Discovery**
- Full-text search across documents
- Filter by type, status, date, amount
- User-specific document collections
- Template library access

## 📚 **Supported Document Types**

| Document Type | Spanish Name | Use Case |
|---------------|-------------|----------|
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

#### **AIDocumentGenerationService**
- **Purpose**: AI-powered document content generation
- **Key Methods**:
  - `GenerateDocumentAsync()` - Create new AI documents
- **Features**: Prompt processing, content generation, PDF conversion

#### **DocumentService**
- **Purpose**: Document lifecycle management
- **Key Methods**:
  - `CreateDocumentAsync()` - Create new documents
  - `UpdateDocumentAsync()` - Modify existing documents
  - `SearchDocumentsAsync()` - Search and filter documents
  - `UpdateDocumentFileInfoAsync()` - Update file metadata
- **Features**: CRUD operations, caching, status management

#### **UserService**
- **Purpose**: User account and profile management
- **Key Methods**:
  - `CreateUserAsync()` - Register new users
  - `UpdateUserAsync()` - Update user profiles
  - `GetUserAsync()` - Retrieve user information
- **Features**: Spanish ID validation, business entity support

#### **FileStorageService**
- **Purpose**: Document file storage and retrieval
- **Key Methods**:
  - `SaveDocumentBytesAsync()` - Store PDF files
  - `GetDocumentBytesAsync()` - Retrieve stored files
  - `GetDocumentMetadataAsync()` - File information
- **Features**: Azure Blob integration, metadata management

#### **CacheService**
- **Purpose**: Performance optimization through caching
- **Key Methods**:
  - `GetAsync<T>()` - Retrieve cached data
  - `SetAsync<T>()` - Store data in cache
  - `RemoveAsync()` - Invalidate cache entries
- **Features**: Redis integration, pattern-based invalidation

#### **NotificationService**
- **Purpose**: User communication and alerts
- **Key Methods**:
  - `SendDocumentGenerationNotificationAsync()` - Generation alerts
  - `SendDocumentStatusChangeNotificationAsync()` - Status updates
- **Features**: Multi-channel notifications, event-driven messaging

## 🌐 **API Endpoints**

### **User Management** (`/api/users`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/users` | Create new user account |
| `GET` | `/api/users/{id}` | Get user by ID |
| `PUT` | `/api/users/{id}` | Update user profile |
| `DELETE` | `/api/users/{id}` | Deactivate user account |
| `GET` | `/api/users` | List all users (admin) |

### **Document Management** (`/api/documents`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/documents` | Create new document |
| `GET` | `/api/documents/{id}` | Get document by ID |
| `PUT` | `/api/documents/{id}` | Update document |
| `DELETE` | `/api/documents/{id}` | Delete document |
| `GET` | `/api/documents/user/{userId}` | Get user's documents |
| `GET` | `/api/documents/type/{type}` | Get documents by type |
| `GET` | `/api/documents/status/{status}` | Get documents by status |
| `GET` | `/api/documents/templates` | Get available templates |
| `GET` | `/api/documents/search?term={term}` | Search documents |

### **AI Document Generation** (`/ai/documents`)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/ai/documents/generate` | Generate new AI document |
| `POST` | `/ai/documents/{id}/regenerate` | Regenerate with updated prompts |
| `GET` | `/ai/documents/{id}/download` | Download generated PDF |
| `GET` | `/ai/documents/{id}/status` | Check generation status |

## 📊 **Data Models**

### **User Entity**
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

### **Document Entity**
```csharp
public class Document
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; }
    public DocumentType Type { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public string FilePath { get; set; }     // Blob storage path
    public string FileName { get; set; }
    public string FileFormat { get; set; }
    public long FileSize { get; set; }
    public DocumentStatus Status { get; set; }
    public decimal? Amount { get; set; }
    public string Currency { get; set; }
    public int? Quarter { get; set; }
    public int? Year { get; set; }
    public bool IsTemplate { get; set; }
    // ... timestamps and navigation properties
}
```

## 🔧 **Development Setup**

### **Prerequisites**
- .NET 8 SDK
- Docker & Docker Compose
- Azure Functions Core Tools
- PostgreSQL client (optional)

### **Quick Start**

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Legal-IA
   ```

2. **Start infrastructure services**
   ```bash
   docker-compose up -d
   ```
   This starts:
   - PostgreSQL (port 5433)
   - Redis (port 6380)
   - Azurite (blob storage on port 10000)

3. **Configure connection strings**
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

4. **Run database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Start the application**
   ```bash
   func start
   ```

6. **Access the API**
   - Functions: `http://localhost:7071`
   - Admin interface: `http://localhost:7071/admin`

## 📁 **Project Structure**

```
Legal-IA/
├── 📂 Data/                    # Database context and configuration
├── 📂 DTOs/                    # Data Transfer Objects
├── 📂 Functions/               # Azure Functions (HTTP triggers)
│   ├── 📂 Activities/          # Durable Functions activities
│   └── 📂 Orchestrators/       # Workflow orchestrators
├── 📂 Interfaces/              # Service and repository contracts
│   ├── 📂 Repositories/        # Data access interfaces
│   └── 📂 Services/            # Business logic interfaces
├── 📂 Models/                  # Domain entities
├── 📂 Repositories/            # Data access implementations
├── 📂 Services/                # Business logic implementations
├── 📂 Validators/              # Input validation rules
├── 📂 Migrations/              # EF Core database migrations
└── 📄 Program.cs               # Dependency injection configuration
```

## 🧪 **Testing**

### **Postman Collections**
The project includes comprehensive Postman collections for testing:

- **Legal-IA Main Collection** - Core API endpoints
- **User Management** - User CRUD operations
- **Document Management** - Document lifecycle testing
- **Spanish Bureaucratic Documents** - Document type-specific tests
- **Validation & Edge Cases** - Error handling and validation
- **Workflow Testing** - End-to-end process validation

### **Test Scenarios**
1. **User Registration** - Spanish DNI/CIF validation
2. **Document Creation** - Various document types
3. **AI Generation** - Prompt-based document creation
4. **File Operations** - Upload, download, metadata
5. **Search & Filtering** - Content discovery
6. **Error Handling** - Validation and edge cases

## 🔐 **Security & Compliance**

### **Data Protection**
- **GDPR Compliant** - EU data protection regulations
- **Spanish LOPD** - Local data protection compliance
- **Encryption** - Data at rest and in transit
- **Access Control** - Role-based permissions

### **Validation**
- **Spanish ID Validation** - DNI/CIF format verification
- **Business Rules** - Legal document requirements
- **Input Sanitization** - XSS and injection protection
- **Rate Limiting** - API abuse prevention

## 📈 **Monitoring & Observability**

### **Application Insights**
- **Performance Metrics** - Response times, throughput
- **Error Tracking** - Exception monitoring and alerting
- **User Analytics** - Usage patterns and trends
- **Custom Telemetry** - Business-specific metrics

### **Logging**
- **Structured Logging** - JSON-formatted logs
- **Correlation IDs** - Request tracing across services
- **Audit Trail** - Document lifecycle tracking
- **Performance Monitoring** - Slow query detection

## 🚀 **Deployment**

### **Azure Deployment**
1. **Azure Functions App** - Serverless compute
2. **Azure Database for PostgreSQL** - Managed database
3. **Azure Redis Cache** - Managed caching
4. **Azure Blob Storage** - Document storage
5. **Application Insights** - Monitoring and analytics

### **Environment Configuration**
- **Development** - Local Docker containers
- **Staging** - Azure services with dev data
- **Production** - Full Azure stack with monitoring

## 🤝 **Contributing**

### **Development Guidelines**
1. Follow C# coding standards and conventions
2. Implement comprehensive unit tests
3. Update documentation for new features
4. Validate Spanish legal compliance requirements
5. Ensure API backward compatibility

### **Code Quality**
- **FluentValidation** - Input validation
- **Entity Framework** - Data access patterns
- **Dependency Injection** - Loose coupling
- **Repository Pattern** - Data abstraction
- **Clean Architecture** - Separation of concerns

## 📞 **Support & Contact**

For questions, issues, or contributions related to Legal-IA:

- **Technical Issues** - GitHub Issues
- **Feature Requests** - GitHub Discussions
- **Legal Compliance** - Consult with Spanish legal experts
- **Documentation** - Refer to inline code documentation

---

**Legal-IA** - Empowering Spanish legal professionals with AI-driven document automation 🇪🇸⚖️

*Built with .NET 8, Azure Functions, and modern cloud-native architecture for scalable legal document processing.*
