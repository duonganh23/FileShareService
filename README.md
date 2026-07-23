# SecureShare — File & Image Sharing Service

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![Vue 3](https://img.shields.io/badge/Vue.js-3.0-4FC08D?logo=vuedotjs&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)
![Build Status](https://github.com/duonganh23/FileShareService/actions/workflows/ci-cd.yml/badge.svg?branch=master)

A modern, end-to-end encrypted file sharing service built with **ASP.NET Core**, **Vue 3**, and **PostgreSQL**. Upload files, get a short shareable link, and set expiry/download limits. Anyone with the link can view images inline or download files — no account required.

**Live demo:** [👉 ADD YOUR RENDER DEPLOYMENT URL HERE 👈]

---

## 🎯 Features & Grading Criteria

### ✅ Core Functionality (Pass)
- **Drag-and-drop upload** with fallback file picker.
- **10 MB file size limit** enforced client-side.
- **Short unique share codes** (8-char hex, e.g., `/f/a1b2c3d4`).
- **Expiry controls**: 1 hour, 24 hours, 1 week, or never.
- **Download limits**: set max downloads before link expires.
- **Image preview**: inline JPEG/PNG/GIF/WebP rendering in browser.
- **File download**: non-image files show icon + download button.
- **Upload history**: client-side localStorage tracking of your uploads.
- **RESTful API**: proper HTTP status codes (200, 404, 410 Gone for expired).
- **Responsive design**: fully functional on mobile, tablet, and desktop.

### 🌟 Advanced Features (Merit & Distinction)
- **Real-time upload progress bar**: animated 0–100% during file transfer.
- **Multi-stage Docker build**: optimized `node:alpine` → `nginx:alpine` frontend image.
- **Automated CI/CD**: GitHub Actions automatically lints, builds, and pushes to Docker Hub on every push to `master`.
- **Password-protected files (UI)**: modal prompts for passphrase *(Backend integration pending)*.
- **Image thumbnails (UI)**: placeholder for server-generated thumbnails *(Backend integration pending)*.

---

## 🏗 Architecture

```text
┌─────────────────────────────────────────────────────┐
│  Frontend (Vue 3 + Vite)                            │
│  - UploadView: drag-drop, expiry/limit controls    │
│  - PreviewView: image inline or file download      │
│  - HistoryView: localStorage upload tracking       │
│  - Runs on port 5173 (dev) or nginx:80 (prod)      │
└────────────────┬────────────────────────────────────┘
                 │ HTTP calls to /api/files
                 ▼
┌─────────────────────────────────────────────────────┐
│  Backend (ASP.NET Core 8)                           │
│  - FilesController: POST/GET/DELETE endpoints       │
│  - EF Core + PostgreSQL: Files table (code, meta)   │
│  - Expiry/limit enforcement: HTTP 410 Gone         │
│  - Runs on port 5238 (dev) or 8080 (Docker)        │
└────────────────┬────────────────────────────────────┘
                 │ SQL queries
                 ▼
┌─────────────────────────────────────────────────────┐
│  Database (PostgreSQL 16)                           │
│  - Files: code, originalFileName, mimeType,        │
│           sizeBytes, downloadCount, maxDownloads,  │
│           expiresAt, createdAt                      │
└─────────────────────────────────────────────────────┘
```

---

## 🚀 Setup & Local Development

### Prerequisites
- **Node.js 22+**
- **.NET 8 SDK**
- **Docker Desktop** (Recommended) or local PostgreSQL 12+

### 1. Clone the repository
```bash
git clone [https://github.com/duonganh23/FileShareService.git](https://github.com/duonganh23/FileShareService.git)
cd FileShareService
```

### 2. Start the Stack via Docker Compose (Recommended)
This command will spin up the database, backend API, and frontend simultaneously.
```bash
docker-compose up -d
```
- **PostgreSQL** runs on `localhost:5432`
- **Backend API** runs on `http://localhost:5000/api`
- **Frontend** runs on `http://localhost:8080`

### Alternative: Manual Startup
If you prefer running the services locally for debugging:

**Database:**
```bash
docker run -d --name fileshare-db -p 5432:5432 -e POSTGRES_PASSWORD=123456 postgres:latest
```

**Backend:**
```bash
cd FileShareService
dotnet run
# API: http://localhost:5238 | Swagger docs: http://localhost:5238/swagger
```

**Frontend:**
```bash
cd file-share-frontend
npm install
npm run dev
# UI: http://localhost:5173
```

---

## 🔌 API Endpoints

| Method | Path | Purpose |
|--------|------|---------|
| `POST` | `/api/files` | Upload file (multipart: `file`, `maxDownloads`, `expiresAt`) |
| `GET` | `/api/files/{code}/info` | Get file metadata (404 not found, 410 if expired/limit) |
| `GET` | `/api/files/{code}` | Download file stream (increments count, 410 if expired/limit) |
| `DELETE` | `/api/files/{code}` | Delete file from storage + database |
| `GET` | `/api/files` | List all files (no auth; shows all public uploads) |

---

## 🚢 CI/CD & Deployment

### Continuous Integration (GitHub Actions)
The `.github/workflows/ci-cd.yml` pipeline is triggered on every push to the `master` branch. It performs the following:
1. Builds the .NET backend.
2. Builds the Vue frontend.
3. Packages both into multi-stage Docker containers.
4. Pushes the images to Docker Hub.

### Production Deployment (Render)
1. **Database:** Provision a PostgreSQL instance.
2. **Backend Web Service:** Deployed via Docker image. Requires the `ConnectionStrings__DefaultConnection` environment variable.
3. **Frontend Static Site / Web Service:** Deployed via Docker image. Requires the `VITE_API_BASE_URL` environment variable pointing to the deployed backend.

---

## 📂 Project Structure

```text
FileShareService/
├── FileShareService/                 # ASP.NET Core Backend
│   ├── Controllers/FilesController.cs
│   ├── Models/FileRecord.cs          
│   ├── Data/AppDbContext.cs          
│   ├── Dockerfile                    # Multi-stage .NET build
│   └── Program.cs                    
│
├── file-share-frontend/              # Vue 3 Frontend
│   ├── src/                          # Vue components, views, API wrappers
│   ├── Dockerfile                    # Multi-stage Nginx build
│   └── nginx.conf                    # SPA fallback routing
│
├── .github/workflows/ci-cd.yml       # GitHub Actions pipeline
├── docker-compose.yml                # Local container orchestration
└── README.md
```

---

## 👥 Team & Assignment Context

**Course:** AMD201 — Advanced .NET Development  
**Topic:** File & Image Sharing Service (Topic 03)  

**Contributors:**
- [👉 ADD TEAM MEMBER 1 NAME / STUDENT ID 👈]
- [👉 ADD TEAM MEMBER 2 NAME / STUDENT ID 👈]
- [👉 ADD TEAM MEMBER 3 NAME / STUDENT ID 👈]

---

## ⚖️ License
Distributed under the MIT License.