# Quantaris

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-4.8%20%7C%208.0%20%7C%2010.0-512BD4)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/Database-MySQL%20%2F%20MariaDB-003545)](https://mariadb.org/)

A multi-tenant **Six Sigma DMAIC & Operational Excellence** SaaS platform built for quality engineering and continuous improvement teams. Two architectures coexist in this repository:

1. **Legacy Web App (`Quantaris/`)** — Full-featured ASP.NET WebForms (.NET Framework 4.8 / VB.NET) application with interactive dashboards, 15-step DMAIC workspaces, and 16 statistical/quality tools
2. **Modern Web API (`src/`)** — Clean Architecture ASP.NET Core Web API (C#) with Entity Framework Core global query filters, JWT authentication, and Docker support

---

## About

**Quantaris** is an end-to-end Six Sigma project management, statistical analysis, and quality governance platform. In many organizations, continuous improvement teams split their work across disconnected spreadsheets, statistical packages, slide decks, and task trackers. Quantaris unifies project execution, statistical validation, financial benefit tracking, and team certification inside a single multi-tenant workspace.

### What Problem Does Quantaris Solve?

- **Fragmented DMAIC Execution**: Standardizes every project around a structured **15-step Define–Measure–Analyze–Improve–Control** roadmap so nothing falls through the cracks between project chartering and control-plan handoff.
- **Built-In Statistical Rigor**: Provides Minitab-style spreadsheet entry and instant statistical calculations ($C_p$, $C_{pk}$, $P_{pk}$, DPMO, Z-score, I-MR / $\bar{X}$-R / attribute control limits, Gage R&R ANOVA, full-factorial DOE effects, regression, and Monte Carlo simulations) directly linked to project metrics.
- **Executive Visibility & ROI**: Connects shop-floor quality improvements to strategic goals (**Hoshin Kanri**), **Cost of Poor Quality (COPQ)** reduction, and verified **Financial Benefits Tracking**.
- **Enterprise Multi-Tenancy & Auditability**: Isolates tenant data at the database and ORM query-filter level while recording an immutable audit trail of project and step changes.

### Key Capabilities

- **15-Step DMAIC Lifecycle**
  - **Define**: Voice of the Customer (VOC), Critical-to-Quality (CTQ) trees, Project Charter & Stakeholder registry, and high-level SIPOC process mapping
  - **Measure**: Metric definitions, Data Collection Plans, Measurement System Analysis (Gage R&R), and baseline process capability
  - **Analyze**: Performance gap analysis, fishbone / root-cause prioritization, statistical hypothesis validation, and Bayesian AI root-cause pattern matching
  - **Improve**: Solution brainstorming & prioritization matrices, pilot execution plans, impact validation, and Failure Mode & Effects Analysis (FMEA)
  - **Control**: Control plans & SPC monitoring, best-practice replication across sites, and team recognition/celebration
- **16 Integrated Quality & Statistical Tools** — FMEA, SPC Control Charts, Process Capability, COPQ Calculator, Design of Experiments (DOE), AI Root Cause Predictor, Workflow & Approval Designer, Project Gantt Chart, Gage R&R (MSA), Monte Carlo Simulation, QFD (House of Quality), Linear & Multiple Regression, Sample Size Calculator, Financial Benefits Tracking, Hoshin Kanri Policy Deployment, and a built-in Training & Certification LMS
- **AI Writing & Grammar Assistant** — Built-in grammar/spellcheck diagnostics on project text fields with optional OpenAI-powered tone rephrasing and simplification
- **Interactive Analytics & Reporting** — KPI cards, Chart.js doughnut/bar/radar visualizations, printable executive summaries, and one-click CSV exports
- **Customizable UI Themes** — 4 selectable visual themes (*Modern*, *Classical*, *Premium*, *Minimal*) persisted per user

### Use Cases & Target Teams

- **Manufacturing & Process Industries**: Defect reduction, yield optimization, fill-weight/dimensional capability studies, and Gage R&R calibration
- **Food & Beverage / Pharmaceuticals**: Batch consistency, SPC control charting, CAPA/FMEA risk mitigation, and audit readiness
- **Service, Healthcare & Logistics**: Cycle-time reduction, SLA compliance, COPQ tracking, and cross-functional workflow approvals
- **Lean Six Sigma Belt Programs**: Managing Yellow Belt, Green Belt, Black Belt, and Master Black Belt project portfolios alongside integrated LMS quizzes and certifications

### Tech Stack

| Layer | Legacy (`Quantaris/`) | Modern (`src/`) |
|-------|-----------------------|-----------------|
| **Frontend** | ASP.NET WebForms, Bootstrap 5, Chart.js | REST API + Swagger/OpenAPI (SPA/Blazor ready) |
| **Backend** | VB.NET, .NET Framework 4.8 | C#, .NET 10 / ASP.NET Core Web API |
| **Architecture** | Page Controller + Shared `App_Code` Helpers | Clean Architecture (Core, Infrastructure, Api, Tests) |
| **Database** | MySQL / MariaDB via `MySql.Data` | MySQL / MariaDB via Pomelo EF Core |
| **Multi-Tenancy** | Ticket/Cookie `TenantResolver` + SQL scoping | Scoped `ITenantResolver` + EF Core Global Query Filters |
| **Security & Auth** | FormsAuth + PBKDF2-SHA256 (100k iterations) | JWT Bearer Tokens + PBKDF2-SHA256 (`FixedTimeEquals`) |
| **AI / Grammar** | Local rule engine + LanguageTool / OpenAI API | Shared database schema & AI root-cause entities |
| **Deployment** | IIS or Standalone Server (`SixSigmaSaaS-Server.exe`) | Docker & Docker Compose (`mariadb:10.11` + API) |

---

## Screenshots

| Dashboard | Projects |
|-----------|----------|
| ![Dashboard](screenshots/Screenshot%202026-08-30%20065447.png) | ![Projects](screenshots/Screenshot%202026-08-31%20064006.png) |

| DMAIC Workflow | Tools |
|----------------|-------|
| ![DMAIC](screenshots/Screenshot%202026-08-31%20064052.png) | ![Tools](screenshots/Screenshot%202026-08-31%20064146.png) |

| Reports |
|---------|
| ![Reports](screenshots/Screenshot%202026-08-31%20070231.png) |

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
├── Quantaris.Tests/                 xUnit tests (31 passing)
│   └── StatisticalCalculationTests  Cp, Cpk, DPMO, I-MR charts, FMEA RPN, Auth & Tenancy
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

31 tests covering:
- Process Capability (Cp, Cpk, DPMO calculations)
- Control Charts (I-MR center line, moving range, UCL/LCL)
- DMAIC phase & step mapping (steps 1-15 → phase/name assignment)
- FMEA RPN calculations
- PBKDF2 & legacy SHA-256 password verification and tenant isolation

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

This project is licensed under the [MIT License](LICENSE).
