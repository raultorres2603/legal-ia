# Legal-IA: Unified Legal AI Assistant for Spanish Freelancers

A comprehensive legal and tax advisory system specifically designed for Spanish freelancers (autónomos), featuring a unified AI endpoint that provides personalized legal advice based on user's complete financial context.

## 🚀 Features

### Unified Legal AI Endpoint
- **Single API Endpoint**: All legal AI functionality consolidated into one intelligent endpoint
- **Personalized Responses**: AI includes user's name, business details, and financial context in responses
- **Complete Financial Context**: AI has access to user's invoices, income, VAT, and IRPF data
- **Multi-Query Support**: Handles classification, form guidance, quarterly/annual obligations, and general legal questions

### Core Functionality
- **Legal Question Processing**: Intelligent classification and responses to legal, tax, and judicial questions
- **Form Guidance**: Personalized help with Spanish tax forms (Modelo 303, 130, 100, etc.)
- **Tax Obligations**: Quarterly and annual tax obligation reminders with personalized calculations
- **Invoice Management**: Complete invoice and invoice item management system
- **User Management**: Secure user registration, authentication, and profile management

## 🏗️ Architecture

### Technology Stack
- **.NET 8**: Modern C# application framework
- **Azure Functions**: Serverless compute platform
- **Azure Durable Functions**: Workflow orchestration
- **Entity Framework Core**: Object-relational mapping
- **OpenAI API**: AI-powered legal advice via OpenRouter
- **JWT Authentication**: Secure user authentication

### Project Structure
```
Legal-IA/
├── AI-Agent/                 # AI logic and prompt management
│   ├── Models/              # AI request/response models
│   ├── Services/            # AI service implementations
│   └── Helpers/             # Prompt builders and parsers
├── Legal-IA/                # Main Azure Functions project
│   ├── Functions/           # HTTP triggers and orchestrators
│   ├── Activities/          # Durable function activities
│   └── Bruno/              # API test collections
└── Legal-IA.Shared/        # Shared models and repositories
    ├── Models/             # Data models
    ├── Repositories/       # Data access layer
    └── Data/               # Database context
```

## 📡 API Endpoints

### Unified Legal AI Endpoint
```
POST /ai/legal/question
```

**Request Body:**
```json
{
  "question": "Your legal question in Spanish",
  "queryType": "general|form-guidance|quarterly-obligations|annual-obligations|classify",
  "formType": "modelo-303",
  "quarter": 3,
  "year": 2024,
  "includeUserContext": true,
  "includeInvoiceData": true
}
```

**Response:**
```json
{
  "success": true,
  "answer": "Hola Juan, basándome en tus facturas...",
  "queryType": "form-guidance",
  "formType": "modelo-303",
  "formGuidance": "Detailed form instructions...",
  "obligations": "Tax obligations...",
  "userContextIncluded": true,
  "timestamp": "2024-08-23T..."
}
```

### Other Core Endpoints
- **User Management**: `/auth/register`, `/auth/login`
- **Invoice Management**: `/invoices/*`
- **Invoice Items**: `/invoice-items/*`

## 🤖 AI Capabilities

### Personalization Features
- **User-Aware Responses**: AI addresses users by name
- **Financial Context Integration**: Responses consider user's actual income, VAT, and IRPF data
- **Invoice-Based Calculations**: Tax advice based on real invoice data
- **Business Profile Awareness**: Considers user's business type, location, and tax regime

### Query Types Supported
1. **General Legal Questions**: Comprehensive legal and tax advice
2. **Form Guidance**: Step-by-step help with Spanish tax forms
3. **Quarterly Obligations**: Period-specific tax requirements
4. **Annual Obligations**: Yearly tax filing requirements
5. **Classification**: Determines if questions are legal/tax-related

### Smart Context Handling
- **UserFullContext**: Complete user profile including personal and business information
- **Invoice Integration**: Real-time access to user's invoice history and financial totals
- **Period-Specific Data**: Quarterly and annual financial calculations
- **Personalized Prompts**: AI receives detailed context about user's specific situation

## 🧪 Testing

### Bruno API Collections
Comprehensive API testing suites included:

- **E2E Complete Flow**: End-to-end user journey testing
- **Legal AI Agent Tests**: Comprehensive AI functionality testing
- **Unified API Tests**: New consolidated endpoint testing

### Test Coverage
- ✅ Legal question classification
- ✅ Personalized form guidance
- ✅ Quarterly obligations with financial context
- ✅ Annual obligations with invoice data
- ✅ Error handling and validation
- ✅ Performance testing for complex queries
- ✅ User context integration

## 🔧 Configuration

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection=your_database_connection

# AI Services
OpenRouterApiKey=your_openrouter_api_key

# JWT
JwtSecretKey=your_jwt_secret
JwtIssuer=your_issuer
JwtAudience=your_audience
```

### Local Development
1. Clone the repository
2. Set up local database connection
3. Configure environment variables
4. Run with `dotnet run` or your preferred IDE

## 📋 Recent Major Updates

### Unified AI Endpoint (v2.0)
- **Consolidated Architecture**: Multiple AI endpoints merged into single intelligent endpoint
- **Enhanced Personalization**: AI now includes complete user financial context
- **Improved Response Structure**: Unified response format for all query types
- **Full Invoice Integration**: AI has access to user's complete invoice history

### Key Architectural Changes
- **Removed Separate Endpoints**: Eliminated `/classify`, `/form-guidance`, `/quarterly-obligations`, `/annual-obligations`
- **Single Point of Entry**: All AI functionality now accessible via `/ai/legal/question`
- **Enhanced Data Flow**: UserFullContext model provides complete user and financial data
- **Improved Testing**: Updated Bruno collections for comprehensive unified API testing

### Benefits of Consolidation
- **Simplified Integration**: Single endpoint for all AI functionality
- **Better User Experience**: More personalized and context-aware responses
- **Improved Maintainability**: Reduced code duplication and complexity
- **Enhanced Performance**: More efficient data handling and AI processing

## 🎯 Use Cases

### For Spanish Freelancers
- Get personalized tax advice based on actual income
- Understand quarterly and annual tax obligations
- Receive step-by-step form completion guidance
- Calculate VAT and IRPF based on real invoice data
- Plan tax optimizations for the upcoming year

### Example Interactions
```
User: "¿Cuánto IVA debo declarar este trimestre?"
AI: "Hola María, basándome en tus 12 facturas de este trimestre por un total de €15,240..."

User: "¿Cómo relleno el modelo 303?"
AI: "Hola Carlos, para completar tu modelo 303 con tus datos actuales..."
```

## 🔒 Security & Privacy

- **JWT Authentication**: Secure user authentication and authorization
- **Data Encryption**: Sensitive data encrypted in transit and at rest
- **User Isolation**: Each user's data completely isolated
- **GDPR Compliance**: Built with European privacy regulations in mind

## 🤝 Contributing

This project follows standard .NET development practices:
- Clean Architecture principles
- SOLID design patterns
- Comprehensive unit and integration testing
- API-first design approach

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For questions about Spanish tax law and regulations, the AI assistant provides general guidance only. Always consult with a qualified tax professional for specific situations.

---

**Built with ❤️ for the Spanish freelancer community**
