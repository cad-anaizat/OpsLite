# OpsLite — Work Orders MVP (C#/.NET 8 + Azure)

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/Language-C%23-178600?logo=csharp&logoColor=white)
![Azure](https://img.shields.io/badge/Cloud-Azure-0078D4?logo=microsoft-azure&logoColor=white)
![Build](https://img.shields.io/badge/Build-passing-brightgreen?logo=githubactions&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-informational)

A production-ready baseline for **Operations Work Orders** you can run locally and deploy to **Azure**. Built with **.NET 8**, Razor Pages, Minimal API, and EF Core.

---

## ✨ Features
- Razor Pages UI + Minimal API
- EF Core with SQLite locally / Azure SQL in cloud
- CRUD for Work Orders, Notes, immutable Events (audit trail)
- OpenAPI (Swagger UI)
- Structured logging (Serilog)

---

## 🧱 Architecture (MVP)

```mermaid
flowchart LR
    A["Browser (Razor Pages)"] --> B["Web App (ASP.NET Core)"]
    subgraph B1[OpsLite]
      B -->|"Razor Pages"| C["EF Core"]
      B -->|"Minimal API /api/*"| C
    end
    C -->|"Connection"| D["SQLite Local / Azure SQL"]
    B -->|"Telemetry"| E["Application Insights"]
````

---

## 🚀 Run Locally

**Prereqs:**

* .NET 8 SDK
* VS Code + C# Dev Kit (optional, recommended)

```bash
cd src/OpsLite

# Restore and run
dotnet restore
dotnet run

# App will start on:
#   https://localhost:5001 (UI + API)
#   http://localhost:5000  (UI + API)
# Swagger UI:
#   https://localhost:5001/swagger
```

---

## 🔌 API Quick Test (Local)

```bash
curl -k -X POST https://localhost:5001/api/workorders \
 -H "Content-Type: application/json" \
 -d '{"title":"Replace belt","description":"Conveyor B-3","priority":2,"category":"Maintenance","requester":"tester"}'
```

---

## ☁️ Azure Deployment (Up to this Point)

**1) Install Azure CLI**
[Download Azure CLI](https://aka.ms/installazurecliwindows)
Restart Command Prompt after install.

**2) Login to Azure**

```bash
az login --tenant <YOUR_TENANT_ID>
az account set --subscription "<YOUR_SUBSCRIPTION_NAME>"
```

**3) Register required resource providers**

```bash
az provider register --namespace Microsoft.Web
az provider register --namespace Microsoft.Sql

# Wait until both show "Registered"
az provider show --namespace Microsoft.Web --query "registrationState" -o tsv
az provider show --namespace Microsoft.Sql  --query "registrationState" -o tsv
```

**4) Prepare variables (no secrets in repo)**
These will be set in your session only — **do not commit to Git**.

```bash
set RG=opslite-rg
set LOC=eastus
set PLAN=opslite-plan
set APP=opslite-web-%RANDOM%
set SQLSV=opslitesql%RANDOM%
set SQLADMIN=sqladminuser
set SQLPASS=StrongPassword123!
set DB=opslitedb
```

**5) Resource group**

```bash
az group create -n %RG% -l %LOC%
```

> **Next steps after this point**:
>
> * Create App Service Plan + Web App
> * Create Azure SQL + firewall rules
> * Configure connection strings
> * Publish and zip-deploy
>   (See “Deployment Completion” section to be added after final deployment)

---

## 🧰 Tools & Services Used

* **.NET 8 SDK** — application runtime and tooling
* **Azure CLI** — resource provisioning & deployment
* **EF Core Tools** — migrations and DB updates (`dotnet tool install --global dotnet-ef`)

---

## 🔒 Security & Config

* Connection strings loaded from `ConnectionStrings:DefaultConnection`.
* In Azure: store them as **Connection Strings** in App Settings.
* Plan to integrate **Azure Key Vault** + Managed Identity for secrets.
* Enforce HTTPS in Azure (Portal → TLS/SSL settings → HTTPS Only: On).

---

## 📦 Project Layout

```
src/OpsLite
├─ Pages/            # Razor Pages UI
├─ wwwroot/          # Static assets
├─ Program.cs        # Minimal API + DI + EF setup
├─ appsettings.json  # Local config (SQLite)
└─ OpsLite.csproj
```

---

## 🗺️ Roadmap (Post-Deployment)

* ASP.NET Identity (auth & roles)
* Blob Storage attachments + virus scan
* Service Bus + Functions for notifications
* SLA timers & escalations
* Multi-tenant support

```
