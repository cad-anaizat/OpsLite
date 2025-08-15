# OpsLite — Work Orders MVP (C# / .NET 8 + Azure)

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/Language-C%23-178600?logo=csharp&logoColor=white)
![Azure](https://img.shields.io/badge/Cloud-Azure-0078D4?logo=microsoft-azure&logoColor=white)
![Build](https://img.shields.io/github/actions/workflow/status/cad-anaizat/OpsLite/build.yml?branch=main&label=Build&logo=githubactions&logoColor=white)
![Contributors](https://img.shields.io/github/contributors/cad-anaizat/OpsLite?logo=github)
![License](https://img.shields.io/badge/License-MIT-informational)
![Last Commit](https://img.shields.io/github/last-commit/cad-anaizat/OpsLite?logo=git)

A **production-ready** framework for managing **Operations Work Orders**, designed for local execution and seamless Azure deployment.  
Built with **.NET 8**, **Razor Pages**, **Minimal API**, and **EF Core** — authored by **Anaizat Hereim**, a frontier-ready technologist.

---

## ✨ Features

- **Modern UI & API** — Razor Pages + Minimal API
- **EF Core Dual-Mode** — SQLite local / Azure SQL cloud
- **Full CRUD** — Work Orders, Notes, immutable Events (audit trail)
- **OpenAPI** — Integrated Swagger UI
- **Observability** — Structured logging with Serilog

---

## 🧱 Architecture

```mermaid
flowchart TB
    subgraph Client
      UI["Browser<br/>(Razor Pages)"]
    end
    subgraph Server["OpsLite Web App (.NET 8)"]
      API["Minimal API /api/*"]
      Pages["Razor Pages UI"]
      EF["Entity Framework Core"]
    end
    subgraph Data
      DB["SQLite (Local) / Azure SQL (Cloud)"]
    end
    subgraph Telemetry
      AI["Azure Application Insights"]
    end

    UI --> Pages
    Pages --> EF
    API --> EF
    EF --> DB
    Server --> AI
````

---

## 📸 Demo & Screenshots

> Images folder visually demonstrate OpsLite in action.

---

## 🚀 Quick Start

### Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* (Optional) [Visual Studio Code](https://code.visualstudio.com/) with C# Dev Kit

```bash
cd src/OpsLite
dotnet restore
dotnet run
```

**Local URLs**:

* App: [https://localhost:5001](https://localhost:5001)
* Swagger: [https://localhost:5001/swagger](https://localhost:5001/swagger)

---

## ☁️ Azure Deployment Summary

> This repository includes **end-to-end deployment instructions** for Azure using the CLI.
> Includes resource group creation, App Service deployment, Azure SQL provisioning, and connection string configuration.

**Example — Create Resource Group**:

```bash
az group create -n opslite-rg -l eastus
```

**Example — Deploy ZIP**:

```bash
az webapp deploy -g opslite-rg -n opslite-web-d0001 --src-path app.zip --type zip
```

---

## 🧰 Tooling & Services

* **.NET 8 SDK** — runtime and tooling
* **Azure CLI** — deployment automation
* **EF Core Tools** — DB migrations & updates
* **Serilog** — structured logging
* **Swagger / OpenAPI** — API documentation

---

## 📦 Project Structure

```
src/OpsLite
├─ Pages/             # Razor Pages UI
├─ wwwroot/           # Static assets
├─ Program.cs         # Minimal API + EF + DI
├─ appsettings.json   # Local SQLite config
└─ OpsLite.csproj
```

---

## 🤝 Contributing

This project welcomes contributions from the **Microsoft MSSA CCAD19** cohort and the wider developer community.

1. Fork the repo
2. Create a feature branch (`git checkout -b feature/new-feature`)
3. Commit changes (`git commit -m 'Add some feature'`)
4. Push to branch (`git push origin feature/new-feature`)
5. Open a Pull Request

---

## 🛣 Roadmap

* Azure Key Vault for secrets
* ASP.NET Identity authentication
* Azure Blob Storage attachments + AV scanning
* Event-driven architecture with Service Bus
* SLA timers & escalation workflows

---

## 📜 License

MIT License — see `LICENSE` file for details.

