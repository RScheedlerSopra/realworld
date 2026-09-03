# RealWorld Conduit (ASP.NET Core + Angular)

This repository contains a full-stack implementation of the Conduit app from the RealWorld spec (a Medium-style social publishing platform).

The project is split into:

- `backend/`: ASP.NET Core API with Entity Framework Core, CQRS/MediatR, JWT auth, and automatic DB migrations on startup.
- `frontend/`: Angular single-page app that consumes the backend API.

## What the app does

Conduit supports:

- User registration and login
- JWT-based authentication
- CRUD articles
- Add/delete comments
- Follow/unfollow users
- Favorite/unfavorite articles
- Profile pages and feeds
- Tag-based filtering and pagination

## Repository layout

```text
realworld/
  backend/   # .NET API
  frontend/  # Angular SPA
```

## Prerequisites

Install these before setup:

- Git
- .NET SDK 10.x (the backend pins SDK version in `backend/global.json`)
- Node.js 20.11.1 or newer
- npm (comes with Node.js)

Optional:

- Docker Desktop (if you want to use containerized backend workflows)

## Setup

### 1. Clone and enter the repository

```bash
git clone <your-repo-url>
cd realworld
```

### 2. Restore backend dependencies

The first build/test run restores NuGet packages automatically.

### 3. Install frontend dependencies

```bash
cd frontend
npm install
cd ..
```

## Build and test

> Recommended validation workflow: run backend build + backend unit tests + frontend build + frontend unit tests.

### Linux/macOS

#### Backend build

```bash
cd backend
dotnet build Conduit.sln
```

#### Backend unit tests

```bash
cd backend
dotnet test tests/Conduit.UnitTests/
```

#### Frontend build

```bash
cd frontend
npm run build
```

#### Frontend unit tests

```bash
cd frontend
npm test
```

### Windows (PowerShell)

#### Backend build

```powershell
cd backend
dotnet build Conduit.sln
```

#### Backend unit tests

```powershell
cd backend
dotnet test tests/Conduit.UnitTests/
```

#### Frontend build

```powershell
cd frontend
npm run build
```

#### Frontend unit tests

```powershell
cd frontend
npm test
```

## Running locally (optional)

If you want to run the app locally for manual testing:

### Start backend API

Linux/macOS:

```bash
cd backend
dotnet run --project src/Conduit/Conduit.csproj
```

Windows (PowerShell):

```powershell
cd backend
dotnet run --project src/Conduit/Conduit.csproj
```

The backend automatically applies pending EF Core migrations on startup.

### Start frontend app

Linux/macOS:

```bash
cd frontend
npm start
```

Windows (PowerShell):

```powershell
cd frontend
npm start
```

Default local URLs:

- Frontend: http://localhost:4200
- Backend Swagger: http://localhost:5000/swagger

## Backend build helper scripts

The backend includes helper scripts:

- Linux/macOS: `backend/build.sh`
- Windows: `backend/build.ps1`

## Notes

- The backend uses SQLite by default for local development unless overridden with environment variables.
- End-to-end frontend tests exist in `frontend/e2e/`, but unit tests are the recommended default validation path for day-to-day development.

## License

This project follows the licenses included in each subproject directory.
