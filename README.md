# Quantaris

A multi-tenant Six Sigma DMAIC project management platform. Two architectures coexist in this repository:

1. **Legacy** — ASP.NET WebForms (.NET Framework 4.8 / VB.NET)
2. **Modern** — ASP.NET Core Web API (C#) + Clean Architecture

---

## Modern Architecture (src/)

### Prerequisites

- .NET 10 SDK (or .NET 8 SDK with Docker)
- MySQL 8.0+ / MariaDB 10.11+
- Docker & Docker Compose (optional)

### Quick Start — Docker

```bash
cd src
docker-compose up --build
```

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- Database: `localhost:3306` (user: `quantaris`, password: `quantaris123`)

### Quick Start — Local

```bash
cd src
dotnet run --project Quantaris.Api
```

Ensure MySQL is running on port 3306. The app auto-creates the database on startup.

### Solution Structure

```
src/
├── Quantaris.Core/                  Domain layer (zero dependencies)
│   └── Entities/
│       ├── Common/                  BaseEntity, ITenantEntity
│       ├── Dmaic/                   16 entities (FMEA, VOC, CTQ, Charter, SIPOC, etc.)
│       ├── Tools/                   13 entities (ControlCharts, Regression, DOE, etc.)
│       ├── Training/                Courses, Certifications
│       ├── Workflow/                Definitions, States, Transitions, Instances, Approvals
│       └── AI/                      Root Cause Models, Patterns, Predictions
│
├── Quantaris.Infrastructure/        Data + Services layer
│   ├── Data/
│   │   ├── QuantarisDbContext        42 DbSets, global query filters for multi-tenancy
│   │   ├── IUnitOfWork / UnitOfWork
│   │   └── ITenantResolver
│   ├── Repositories/
│   │   ├── IRepository<T>            Generic CRUD
│   │   └── IProjectRepository        Project-specific queries
│   └── Services/
│       ├── DmaicService              15-step DMAIC lifecycle
│       ├── JwtTokenService           JWT token generation
│       └── AuditService              Audit trail logging
│
├── Quantaris.Api/                   ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── AuthController            /api/auth/login, /register, /me, /tenants
│   │   ├── ProjectsController        /api/projects (CRUD + FMEA sub-resource)
│   │   └── DmaicController           /api/dmaic/{id}/steps, /progress
│   ├── Middleware/
│   │   └── TenantMiddleware          Extracts TenantId from JWT claims
│   └── Program.cs                    DI, JWT auth, Swagger, CORS, auto-migrate
│
├── Quantaris.Tests/                 xUnit tests (23 passing)
│   └── StatisticalCalculationTests  Cp, Cpk, DPMO, I-MR charts, FMEA RPN
│
├── Dockerfile                       Multi-stage .NET 8 build
└── docker-compose.yml               MariaDB 10.11 + API with healthchecks
```

### Architecture Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| ORM | EF Core 8 + Pomelo MySQL | Mature MySQL provider, LINQ, migrations |
| Auth | JWT Bearer tokens | Stateless, SPA-friendly, multi-tenant claims |
| Multi-tenancy | Global query filters | Row-level isolation enforced at EF Core level |
| Pattern | Repository + Unit of Work | Clean separation, testable, industry standard |
| Container | Docker Compose | One-command dev environment with healthchecks |

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Authenticate (returns JWT) |
| POST | `/api/auth/register` | Create account |
| GET | `/api/auth/me` | Current user info |
| GET | `/api/auth/tenants` | List active tenants |
| GET | `/api/projects` | List projects (tenant-scoped) |
| POST | `/api/projects` | Create project |
| GET | `/api/projects/{id}` | Get project with DMAIC steps |
| PUT | `/api/projects/{id}` | Update project |
| DELETE | `/api/projects/{id}` | Delete project |
| GET | `/api/projects/{id}/fmea` | Get FMEA records |
| POST | `/api/projects/{id}/fmea` | Create FMEA record |
| PUT | `/api/projects/{id}/fmea/{fid}` | Update FMEA record |
| DELETE | `/api/projects/{id}/fmea/{fid}` | Delete FMEA record |
| GET | `/api/dmaic/{id}/steps` | Get DMAIC steps |
| PUT | `/api/dmaic/{id}/steps/{num}` | Update step status |
| GET | `/api/dmaic/{id}/progress` | Get progress summary |

### Database Schema (42 Tables)

Mapped via EF Core entity models with global query filters for multi-tenancy:

| Category | Entities |
|----------|----------|
| **Core** | Tenant, User, AuditLog, Project, DmaicStep |
| **Define** | VocRecord, CtqRequirement, ProjectCharter, Stakeholder, SipocMap |
| **Measure** | MeasurementPlan, Metric, MetricsHistory, GageRnRStudy |
| **Analyze** | RootCauseAnalysis, AiRootCauseModel, AiRootCausePattern, AiRootCausePrediction |
| **Improve** | SolutionIdea, PilotPlan, FmeaRecord |
| **Control** | ControlPlan, BestPractice, Celebration |
| **Quality Tools** | ControlChartModel/Data/Limits, ProcessCapabilityRecord, CopqRecord, DoeExperiment, RegressionModel/Data, QfdModel/Requirement/Matrix/Correlation, MonteCarloModel/Input/Result, SampleSizeCalculation |
| **Project Mgmt** | GanttTask, GanttMilestone, BenefitsTracking, BenefitsActual, HoshinKanriModel/Item |
| **Training** | TrainingCourse, TrainingCertification |
| **Workflow** | WorkflowDefinition, WorkflowState, WorkflowTransition, WorkflowInstance, WorkflowApproval |

### Testing

```bash
cd src
dotnet test
```

23 tests covering:
- Process Capability (Cp, Cpk, DPMO calculations)
- Control Charts (I-MR center line, moving range, UCL/LCL)
- DMAIC phase mapping (step 1-15 → phase assignment)
- FMEA RPN calculations

---

## Legacy Architecture (Quantaris/)

### Prerequisites

- Windows 7/10/11 or Windows Server
- .NET Framework 4.8
- MySQL 8.0+ / MariaDB 10.4+ on `localhost:3306`
  - **XAMPP** recommended: https://www.apachefriends.org

### Quick Start

1. Start MySQL via XAMPP Control Panel
2. Run `Database\SetupDatabase.bat`
3. Run `start-server.bat` (server at `http://localhost:8082`)
4. Open `http://localhost:8082/Setup.aspx` → Run Setup
5. Login: `admin` / `Admin@123` / Tenant: `DEMO`

### Project Structure

```
Quantaris/
├── Account/         Login, Register, AccessDenied
├── Admin/           Tenant and user management
├── API/             AIHandler.ashx (grammar/rephrasing API)
├── App_Code/        Shared VB code (BasePage, helpers, DbHelper, AuditHelper, ExportHelper)
├── Content/         CSS, images, grammar.js, spreadsheet.js
├── Database/        SQL scripts (MySQL) and setup
├── DMAIC/           Define, Measure, Analyze, Improve, Control phases
├── Projects/        Project list, create, edit
├── Reports/         Reports dashboard with CSV export and charts
├── Tools/           FMEA, Process Capability, COPQ, DOE, AI Root Cause, Workflow, Gantt,
│                    Gage R&R, Monte Carlo, Control Charts, QFD, Regression, Sample Size,
│                    Benefits, Hoshin Kanri, Training LMS
├── 404.aspx         Custom 404 error page
├── 500.aspx         Custom 500 error page
├── Default.aspx     Main dashboard with Chart.js charts
├── kill-server.bat  Stop server and clear cache
├── start-server.bat Clear cache and start server
└── Web.config       Application configuration
```

### Legacy Features

- **Dashboard**: Chart.js doughnut, bar, and radar charts
- **DMAIC Workflow**: 15-step framework with audit trail
- **AI Grammar Assist**: Local spellcheck + optional OpenAI rephrase/simplify
- **Minitab Spreadsheet**: Reusable component for ControlCharts and Regression
- **CSV Export**: FMEA, COPQ, BenefitsTracking, Projects, Reports
- **Toast Notifications**: Bootstrap 5 popups on all CRUD operations
- **PBKDF2 Hashing**: 100K iterations, backward-compatible with old SHA256
- **Custom Error Pages**: 404 and 500 with Bootstrap styling

### Legacy Tools

| Tool | URL | Description |
|------|-----|-------------|
| FMEA | `/Tools/FMEA` | Failure Mode and Effects Analysis |
| Process Capability | `/Tools/ProcessCapability` | Cp, Cpk, Ppk analysis |
| COPQ | `/Tools/COPQ` | Cost of Poor Quality calculator |
| DOE | `/Tools/DOE` | Design of Experiments |
| AI Root Cause | `/Tools/AIRootCause` | Bayesian pattern matching |
| Workflow Designer | `/Tools/WorkflowDesigner` | Custom workflow / approval engine |
| Gantt Chart | `/Tools/ProjectGantt` | Project Gantt chart |
| Gage R&R | `/Tools/GageRnR` | Measurement System Analysis |
| Monte Carlo | `/Tools/MonteCarlo` | Simulation with histogram |
| Control Charts | `/Tools/ControlCharts` | I-MR, X-bar R, p, u, c charts |
| QFD | `/Tools/QFD` | Quality Function Deployment |
| Regression | `/Tools/Regression` | Linear & multiple regression |
| Sample Size | `/Tools/SampleSize` | Sample size calculator |
| Benefits | `/Tools/BenefitsTracking` | Financial benefits tracking |
| Hoshin Kanri | `/Tools/HoshinKanri` | Policy deployment matrix |
| Training LMS | `/Tools/TrainingLMS` | Courses, quizzes, certifications |

### Configuration (Web.config)

- **Database**: `connectionStrings/QuantarisDb` (default: `server=localhost;port=3306;database=quantaris;uid=root;pwd=;SslMode=None`)
- **Port**: Edit `start-server.bat` (default: 8082)
- **OpenAI**: `appSettings/OpenAIKey` (optional, enables rephrase/simplify)

---

## License

Internal use only.
