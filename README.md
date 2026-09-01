# MediFlow Claims Engine 🏥

[![.NET 9](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture%20%2B%20CQRS-blue)](https://github.com/siddharth161)
[![Tests](https://img.shields.io/badge/Tests-xUnit%20%2B%20FluentAssertions-green)](https://github.com/siddharth161)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)

A high-performance, distributed **Healthcare Claims Adjudication & Provider Validation Pipeline** built with **.NET 9**, implementing **Domain-Driven Design (DDD)**, **Clean Architecture**, **CQRS with MediatR**, and the **Transactional Outbox Pattern**.

Designed to process Medicaid & Medicare (EDI 837P / 837I) claim standards, enforcing strict validation, provider eligibility lookups, and event publishing.

---

## 🏛️ Architecture Overview

```
                            ┌───────────────────────────────┐
                            │        MediFlow.Api           │
                            │ (REST Minimal/Controllers)    │
                            └──────────────┬────────────────┘
                                           │
                                           ▼
                            ┌───────────────────────────────┐
                            │     MediFlow.Application      │
                            │ (CQRS, MediatR, FluentValid)  │
                            └───────┬──────────────┬────────┘
                                    │              │
                    ┌───────────────┘              └────────────────┐
                    ▼                                               ▼
    ┌───────────────────────────────┐               ┌───────────────────────────────┐
    │        MediFlow.Domain        │               │    MediFlow.Infrastructure    │
    │ (Entities, Value Objects,     │               │ (EF Core 9, Transactional     │
    │  Domain Events, Contracts)    │               │  Outbox, InMemory/SQL Server) │
    └───────────────────────────────┘               └───────────────────────────────┘
```

### ✨ Key Technical Highlights
- **Clean Architecture & DDD**: Strict separation of concerns with Rich Domain Models, Value Objects (`Money`, `NationalProviderId`, `DiagnosisCode`), and Domain Events.
- **CQRS via MediatR**: Segregated Command and Query pipelines with automated validation behaviors.
- **Transactional Outbox Pattern**: Atomic database persistence of business entities and integration events, background worker processing for guaranteed delivery.
- **FluentValidation**: Declarative business rule validation with custom ICD-10 and NPI format checkers.
- **Resilience & Fault Tolerance**: Polly policies for circuit breakers, retry with exponential backoff, and graceful fallbacks.
- **RFC 7807 ProblemDetails**: Standardized API error responses across domain exceptions, validation failures, and not-found states.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) (optional, for containerized deployment)

### 1. Clone & Build
```bash
git clone https://github.com/siddharth161/MediFlow-Claims-Engine.git
cd MediFlow-Claims-Engine
dotnet build
```

### 2. Run Tests
```bash
dotnet test --verbosity normal
```

### 3. Run Application
```bash
dotnet run --project src/MediFlow.Api
```

Open your browser to `http://localhost:5000` (or `http://localhost:8080` in Docker) to explore the interactive **Swagger / OpenAPI Documentation**.

---

## 📦 API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/claims` | Submit a new healthcare claim |
| `GET` | `/api/claims` | Retrieve all submitted claims |
| `GET` | `/api/claims/{id}` | Get claim by ID with line items |
| `POST` | `/api/claims/{id}/adjudicate` | Approve or Deny a submitted claim |
| `GET` | `/api/claims/summary` | Get aggregated claims dashboard metrics |
| `GET` | `/api/providers` | List active in-network providers |

---

## 🐳 Docker Deployment

```bash
# Build Docker image
docker build -t mediflow-claims-engine:latest .

# Run container
docker run -d -p 8080:8080 --name mediflow-api mediflow-claims-engine:latest
```

---

## 🧪 Testing Suite
- **Domain Unit Tests**: Validation of Value Objects (`NationalProviderId`, `Money`), aggregate boundary invariant checks, domain event generation.
- **Application CQRS Tests**: Mocked repository validation for `SubmitClaimHandler`, `AdjudicateClaimHandler`, and `GetClaimQueryHandlers`.
- **Validation Tests**: FluentValidation rule verification for positive and edge cases.

---

## 👨‍💻 Author
**Siddharth Shankar**  
- Email: sidds4970@gmail.com  
- GitHub: [@siddharth161](https://github.com/siddharth161)  
- LinkedIn: [Siddharth Shankar](https://www.linkedin.com/in/siddharth-shankar-869272213/)
