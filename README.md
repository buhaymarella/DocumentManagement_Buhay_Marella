# Document Management System

## 1. Overview

The Document Management System is an ASP.NET Core Web API application designed to manage document uploads and retrieval.

It allows users to upload, view, download, and delete documents through RESTful API endpoints.

The application uses Microsoft SQL Server to store document metadata and the local file system to store the actual uploaded documents.

### Features

- Upload documents using multipart/form-data.
- Retrieve a list of uploaded documents.
- Retrieve document information by ID.
- Download uploaded documents.
- Delete documents.
- Validate uploaded file types and file sizes.
- Handle application exceptions through a global exception handler.
- Store uploaded files in the local file system.
- Use Entity Framework Core for database operations.
- Include automated API tests.

---

## 2. Prerequisites

Before running the application, ensure that the following are installed:

- .NET SDK compatible with the project's target framework.
- Microsoft SQL Server.
- Visual Studio 2022 or another compatible .NET IDE.
- Entity Framework Core tools.
- Git (optional, for cloning the repository).

---

## 3. Installation and Setup

### 3.1 Clone the Repository

Clone the repository or download the project files.

```bash
git clone <repository-url>
```

Navigate to the project directory:

```bash
cd DocumentManagementSystem
```

### 3.2 Restore Dependencies

Restore the required NuGet packages:

```bash
dotnet restore
```

### 3.3 Database Configuration

The application uses Microsoft SQL Server as its database provider and Entity Framework Core for database operations.

Open `appsettings.json` and configure the database connection string.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=DocumentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Replace `YOUR_SERVER` with your SQL Server instance.

Make sure the connection string name matches the configuration used in `Program.cs` or `AppDbContext.cs`.

**Important:** Do not commit actual database passwords, access tokens, or other sensitive credentials to the repository. Use environment variables or .NET User Secrets for sensitive configuration.

### 3.4 Apply Database Migrations

The project uses Entity Framework Core migrations to create and update the database schema.

After configuring the database connection, open the Package Manager Console in Visual Studio and execute:

```powershell
Update-Database
```

Alternatively, use the .NET CLI:

```bash
dotnet ef database update
```

This applies the existing migrations and creates or updates the database.

No manual database table creation is required when the migrations are properly configured.

---

## 4. File Storage Configuration

The application uses local file system storage for uploaded documents.

The default storage directory is:

```text
Storage/
└── Documents/
```

The storage location is configured through `StorageOptions.cs`.

Default configuration:

```csharp
namespace DocumentManagementSystem.Options
{
    public class StorageOptions
    {
        public string RootPath { get; set; } = "Storage/Documents";
    }
}
```

The application uses the following components:

- `IFileStorage.cs` – Defines the file storage operations.
- `LocalFileStorage.cs` – Implements the actual local file storage operations.
- `StorageOptions.cs` – Defines the root directory for stored documents.

Document metadata is stored in SQL Server, while the actual uploaded files are stored in the `Storage/Documents` directory.

This approach keeps document information separate from physical file storage.

---

## 5. Running the Application

There are two ways to run the application.

### Option 1: Visual Studio

1. Open the project in Visual Studio.
2. Set `DocumentManagementSystem` as the startup project.
3. Press **F5** to run with debugging.
4. Alternatively, press **Ctrl + F5** to run without debugging.

### Option 2: .NET CLI

Open a terminal in the project directory and execute:

```bash
dotnet run
```

The application will start using the configured launch settings.

### Application URLs

The application URLs depend on the configured launch profile in `Properties/launchSettings.json`.

Swagger UI is available when running in the Development environment, if configured in `Program.cs`.

---

## 6. Running Automated Tests

The project includes automated API tests located in the `Tests` directory.

The test files include:

- `CustomWebApplicationFactory.cs`
- `DocumentApiTests.cs`

### Option 1: Visual Studio Test Explorer

1. Open the project in Visual Studio.
2. Navigate to **Test → Test Explorer**.
3. Wait for the automated tests to be discovered.
4. Click the play button or select **Run All Tests**.

### Option 2: .NET CLI

Run the following command in the project directory:

```bash
dotnet test
```

This builds the test project and executes the available automated tests.

---

## 7. Implemented API Endpoints

The application exposes the following RESTful API endpoints.

Base route:

```text
/api/documents
```

| HTTP Method | Endpoint | Description | Success Response |
|---|---|---|---|
| POST | `/api/documents` | Upload a new document. | 201 Created |
| GET | `/api/documents` | Retrieve the list of documents. | 200 OK |
| GET | `/api/documents/{id}` | Retrieve document information by ID. | 200 OK |
| GET | `/api/documents/{id}/download` | Download a document by ID. | 200 OK |
| DELETE | `/api/documents/{id}` | Delete a document by ID. | 204 No Content |

### 7.1 Upload Document

**POST** `/api/documents`

Uploads a document using `multipart/form-data`.

The endpoint accepts an `UploadFileCommand` object containing the upload information.

The upload request has an 11 MB request size limit.

The endpoint returns HTTP 201 Created when the upload operation completes.

### 7.2 Get All Documents

**GET** `/api/documents`

Retrieves the list of uploaded documents.

The endpoint accepts query parameters through `GetListDocQuery`, allowing the repository to process the requested document listing.

Returns HTTP 200 OK.

### 7.3 Get Document by ID

**GET** `/api/documents/{id}`

Retrieves document information using its unique identifier.

If the document does not exist, the endpoint returns HTTP 404 Not Found.

### 7.4 Download Document

**GET** `/api/documents/{id}/download`

Downloads the physical document associated with the specified document ID.

The endpoint returns the file stream with its corresponding content type and file name.

If the document or its stored file cannot be found, it returns HTTP 404 Not Found.

### 7.5 Delete Document

**DELETE** `/api/documents/{id}`

Deletes the document associated with the specified ID.

Returns HTTP 204 No Content when the deletion is successful.

If the document does not exist, it returns HTTP 404 Not Found.

---

## 8. Architecture and Design Decisions

The application follows a layered architecture to separate responsibilities and improve maintainability.

### 8.1 Project Structure

```text
DocumentManagementSystem/
│
├── Properties/
│   └── launchSettings.json
│
├── Command/
│
├── Controllers/
│   └── DocumentController.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── DTOs/
│   └── DocumentReponse.cs
│
├── Entity/
│   └── Document.cs
│
├── Migrations/
│
├── Options/
│   ├── StorageOptions.cs
│   └── UploadOptions.cs
│
├── Repository/
│   ├── DocumentExceptions.cs
│   ├── DocumentRepository.cs
│   ├── GlobalExceptionHandler.cs
│   └── IDocumentRepository.cs
│
├── Result/
│   └── Result.cs
│
├── Storage/
│   ├── Documents/
│   ├── IFileStorage.cs
│   └── LocalFileStorage.cs
│
├── Tests/
│   ├── CustomWebApplicationFactory.cs
│   └── DocumentApiTests.cs
│
├── appsettings.json
├── DocumentManagementSystem.http
├── Program.cs
└── README.md
```

### 8.2 Controllers

The `Controllers` folder contains the API endpoints.

`DocumentController.cs` handles incoming HTTP requests and delegates document management operations to the repository.

The controller is responsible for:

- Receiving HTTP requests.
- Accepting upload and query parameters.
- Returning appropriate HTTP status codes.
- Handling not-found responses.
- Returning document information and download streams.

### 8.3 Data Access

The application uses Entity Framework Core for database operations.

`AppDbContext.cs` defines the database context and manages communication with SQL Server.

The `Document` entity represents document records stored in the database.

The repository pattern is implemented through:

- `IDocumentRepository.cs` – Defines the document repository operations.
- `DocumentRepository.cs` – Implements the document management and data access operations.

This approach separates database access logic from the controller and makes the application easier to maintain.

### 8.4 File Storage

The application uses local file system storage for uploaded documents.

The storage implementation is separated through an interface:

- `IFileStorage.cs` – Defines the file storage operations.
- `LocalFileStorage.cs` – Implements local file handling.

This abstraction separates physical file operations from the document repository and allows the storage implementation to be replaced in the future without significantly changing the rest of the application.

### 8.5 Global Exception Handling

`GlobalExceptionHandler.cs` implements ASP.NET Core's `IExceptionHandler`.

It handles application exceptions and converts them into appropriate HTTP error responses using `ProblemDetails`.

The handler covers the following cases:

| Exception | HTTP Status Code | Description |
|---|---|---|
| InvalidUploadException | 400 Bad Request | Invalid document upload. |
| FileTooLargeException | 413 Payload Too Large | Uploaded file exceeds the allowed size. |
| UnsupportedFileTypeException | 415 Unsupported Media Type | Unsupported file type. |
| BadHttpRequestException | Depends on request error | Invalid or oversized HTTP request. |
| InvalidDataException | 400 Bad Request | Malformed or oversized multipart request. |
| Other unexpected exceptions | 500 Internal Server Error | Unexpected application error. |

Unexpected server-side exceptions are also logged through the application's logging mechanism.

### 8.6 Configuration

The `Options` folder contains configuration classes:

- `StorageOptions.cs` – Defines the local file storage directory.
- `UploadOptions.cs` – Holds upload-related configuration.

This keeps application configuration separate from the main implementation.

---

## 9. Known Limitations and Assumptions

The following are known limitations and assumptions of the current implementation:

1. **Local File Storage:** Uploaded files are stored on the local machine. The application does not currently use cloud storage or distributed file storage.

2. **Upload Size Limit:** The document upload endpoint has an 11 MB HTTP request size limit.

3. **Database and File Separation:** Document metadata is stored in SQL Server, while the physical document files are stored separately in the local file system.

4. **Storage Dependency:** The application requires access to its configured storage directory to upload, retrieve, and download documents.

5. **File Deletion:** Database and file system operations are separate. If physical file deletion fails, database and storage states may become inconsistent.

6. **Deployment Considerations:** When deploying the application to another machine or hosting environment, the local storage directory must be properly configured and accessible.

7. **Database Availability:** The application requires a properly configured and accessible SQL Server instance.

---

## 10. AI-Assisted Development

AI-assisted development tools were used throughout the development of this project as learning and development resources.

### AI Tools Used

- **ChatGPT (OpenAI)**
- **Claude (Anthropic)**

### How AI Was Used

ChatGPT and Claude were used as development assistance tools for:

- Understanding ASP.NET Core concepts and implementation details.
- Troubleshooting programming errors and debugging issues.
- Understanding Entity Framework Core and database migrations.
- Reviewing code structure and suggesting possible improvements.
- Exploring implementation approaches and design decisions.
- Assisting with technical documentation.
- Learning and implementing automated API testing, as this was my first experience creating automated tests.
- Understanding the purpose of test cases, test setup, and how to execute automated tests.

### Code Review and Modifications

AI-generated suggestions and code were reviewed, modified, and adapted to the project's requirements.

Particular attention was given to understanding the automated testing implementation, including the test setup using `CustomWebApplicationFactory.cs` and the API test cases in `DocumentApiTests.cs`.

The implementation was tested and adjusted during development to ensure that the application components work together.

### Areas of Least Confidence

The areas requiring further review and testing include:

- Automated testing, particularly test coverage and handling additional edge cases, since this was my first time implementing automated tests.
- Handling unexpected file system errors during document deletion.
- Maintaining consistency between database records and physical files when an operation fails.
- Handling malformed or oversized multipart upload requests.
- Testing edge cases involving invalid files and missing document records.

AI tools were used as development assistance rather than substitutes for understanding the implementation. The developer is responsible for understanding and explaining the submitted code, its architecture, dependencies, design decisions, and limitations.

---

## 11. Security and Credentials

The application uses a database connection string to communicate with SQL Server.

To protect sensitive information:

- Do not commit database passwords or access tokens.
- Do not store sensitive credentials directly in source code.
- Use environment variables or .NET User Secrets for sensitive configuration.
- Ensure that private credentials are excluded from the repository.

---

## 12. Summary

The Document Management System provides a RESTful API for managing uploaded documents using ASP.NET Core, Entity Framework Core, and Microsoft SQL Server.

It separates document metadata management from physical file storage through the repository pattern and file storage abstractions.

The application also includes document upload validation, global exception handling, database migrations, and automated API tests.

These components provide a structured foundation for managing documents while keeping the application maintainable and extensible.
