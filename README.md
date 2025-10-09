# Medicare API

A comprehensive RESTful API for managing medical appointments, doctor profiles, and patient interactions built with ASP.NET Core 8.0.

## 🚀 Features

- **User Authentication & Authorization** - JWT-based authentication with role-based access control
- **Doctor Management** - Profile management, availability scheduling, and specialization tracking
- **Appointment System** - Complete CRUD operations for medical appointments
- **Patient Portal** - Patient-specific appointment management and profile access
- **File Upload Support** - Profile picture management for healthcare providers
- **Comprehensive Security** - Authorization checks, input validation, and secure data handling
- **Analytics & Monitoring** - Azure Application Insights integration for tracking user activities and system metrics

## 📋 Prerequisites

Before running this project locally, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Git](https://git-scm.com/) (for version control)
- A code editor like [Visual Studio](https://visualstudio.microsoft.com/), [Visual Studio Code](https://code.visualstudio.com/), or [JetBrains Rider](https://www.jetbrains.com/rider/)

## 🛠️ Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/AmeerKulabi/medicare_api.git
cd medicare_api
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

### 4. Run the Application

```bash
dotnet run --project MedicareApi
```

The API will be available at `https://localhost:7001` and `http://localhost:5000` by default.

## 📊 Analytics & Monitoring

The Medicare API includes Azure Application Insights integration for comprehensive analytics and monitoring.

### Tracked Events

The system automatically tracks the following events:
- **User Registration** - When new users sign up (doctors and patients)
- **User Sign-In** - When users log in to the system
- **Appointment Creation** - When new appointments are created
- **Appointment Updates** - When appointments are modified
- **Appointment Deletion** - When appointments are cancelled
- **Doctor Search** - When users search for doctors (includes search query and filters)
- **Doctor Details Viewed** - When users view doctor profiles
- **Doctor Profile Updated** - When doctors update their profiles
- **Doctor Profile Picture Updated** - When doctors update their profile pictures
- **Daily Metrics** - Automated daily collection of doctor and patient counts (sent at 00:00 UTC)

### Configuration

To enable Application Insights, add your connection string to `appsettings.json`:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-instrumentation-key-here;IngestionEndpoint=https://your-region.in.applicationinsights.azure.com/"
  }
}
```

You can obtain the connection string from your Azure Application Insights resource in the Azure Portal.

### Analytics Service

The `AnalyticsService` class (`MedicareApi/Services/AnalyticsService.cs`) provides the following methods:
- `TrackSignIn(userId, isDoctor)` - Track user login
- `TrackUserRegistration(userId, isDoctor)` - Track new user registration
- `TrackAppointmentCreated(appointmentId, patientId, doctorId)` - Track appointment creation
- `TrackAppointmentUpdated(appointmentId)` - Track appointment updates
- `TrackAppointmentDeleted(appointmentId)` - Track appointment deletion
- `TrackDoctorSearch(searchQuery, specialization, location, resultsCount)` - Track doctor searches
- `TrackDoctorDetailsViewed(doctorId)` - Track doctor profile views
- `TrackDoctorProfileUpdated(doctorId)` - Track doctor profile updates
- `TrackDoctorProfilePictureUpdated(doctorId)` - Track profile picture updates
- `TrackDailyMetrics(doctorCount, patientCount)` - Track daily system metrics

### Daily Metrics Background Service

The `DailyMetricsService` background service automatically sends daily metrics at midnight (00:00 UTC), including:
- Total number of doctors in the system
- Total number of patients in the system

This service runs continuously and ensures consistent metric tracking for monitoring system growth.

## 🧪 Running Tests

This project includes a comprehensive test suite with **88 automated tests** covering all controller endpoints and business logic scenarios.

### Prerequisites for Testing

The test project (`MedicareApi.Tests`) uses:
- **xUnit** - Testing framework
- **Moq** - Mocking framework for dependencies
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for isolated testing
- **Microsoft.AspNetCore.Mvc.Testing** - ASP.NET Core integration testing tools

### Running All Tests

To run the complete test suite:

```bash
dotnet test
```

### Running Tests with Detailed Output

For more detailed test results and logging:

```bash
dotnet test --verbosity detailed
```

### Running Tests with Minimal Output

For a clean, summary view:

```bash
dotnet test --verbosity minimal
```

### Running Specific Test Classes

To run tests for a specific controller:

```bash
# Run only AuthController tests
dotnet test --filter "FullyQualifiedName~AuthControllerTests"

# Run only DoctorsController tests
dotnet test --filter "FullyQualifiedName~DoctorsControllerTests"

# Run only AvailabilityController tests
dotnet test --filter "FullyQualifiedName~AvailabilityControllerTests"
```

### Running Tests by Category

```bash
# Run only tests with specific naming patterns
dotnet test --filter "Name~ValidInput"
dotnet test --filter "Name~Unauthorized"
dotnet test --filter "Name~NotFound"
```

## 📊 Code Coverage

### Generate Code Coverage Reports

To run tests with code coverage analysis:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

This generates a coverage report in the `TestResults` directory with detailed line-by-line coverage statistics.

### Understanding Coverage Output

After running coverage, you'll find:
- **Coverage files** in `MedicareApi.Tests/TestResults/[guid]/coverage.cobertura.xml`
- **Coverage percentage** for each project and class
- **Line coverage details** showing which code paths are tested

### Coverage Thresholds

The CI pipeline enforces a minimum coverage threshold:
- **Minimum overall coverage**: 60%
- **Current coverage**: ~78% (varies by controller)

## 🏗️ Test Project Structure

```
MedicareApi.Tests/
├── Controllers/                    # Controller-specific test suites
│   ├── AuthControllerTests.cs     # Authentication & registration tests
│   ├── DoctorsControllerTests.cs  # Doctor management tests
│   ├── AvailabilityControllerTests.cs # Doctor availability tests
│   ├── DoctorAppointmentsControllerTests.cs # Doctor appointment CRUD tests
│   ├── PatientAppointmentControllerTests.cs # Patient appointment tests
│   └── ProfileControllerTests.cs  # Profile management tests
├── TestFixture.cs                 # Shared test infrastructure & mocking setup
├── GlobalUsings.cs               # Global using statements
└── MedicareApi.Tests.csproj      # Test project configuration
```

## 🧪 Test Categories

The test suite includes comprehensive coverage across multiple scenarios:

### ✅ Test Types Covered

- **Positive Test Cases** - Valid inputs with expected successful outcomes
- **Negative Test Cases** - Invalid data, missing parameters, malformed requests
- **Authorization Tests** - Unauthorized access attempts, role-based permission checks
- **Validation Tests** - Model validation, business rule enforcement
- **Edge Cases** - Boundary conditions, empty datasets, null value handling
- **Database Integration** - Entity creation, updates, relationships, and data persistence

### 📋 Controllers Tested

| Controller | Test Count | Coverage Focus |
|------------|------------|----------------|
| **AuthController** | 15 tests | Registration, login, JWT token validation |
| **DoctorsController** | 12 tests | Doctor profiles, filtering, updates |
| **AvailabilityController** | 15 tests | Schedule management, slot updates |
| **DoctorAppointmentsController** | 15 tests | Appointment CRUD, authorization |
| **PatientAppointmentController** | 16 tests | Patient-specific operations |
| **ProfileController** | 15 tests | Profile management, file uploads |

## 🐛 Debugging Test Failures

### Common Test Issues

1. **Authentication Context Issues**
   ```bash
   # Check authentication setup in failing tests
   dotnet test --filter "Name~Auth" --verbosity detailed
   ```

2. **Database State Issues**
   ```bash
   # Tests use in-memory database - each test should be isolated
   # Check TestFixture.cs for setup/teardown logic
   ```

3. **Configuration Issues**
   ```bash
   # Some tests may require specific appsettings
   # Check AuthController tests for JWT configuration requirements
   ```

### Test Result Interpretation

- **Passed Tests**: Green checkmarks indicate successful scenarios
- **Failed Tests**: Red X marks with detailed error messages and stack traces
- **Skipped Tests**: Yellow indicators for conditionally skipped tests

## 🔧 Build and Test Commands Reference

### Essential Commands

```bash
# Clean build artifacts
dotnet clean

# Restore NuGet packages
dotnet restore

# Build solution
dotnet build

# Build without restore (faster)
dotnet build --no-restore

# Run application
dotnet run --project MedicareApi

# Run tests
dotnet test

# Run tests without building (after successful build)
dotnet test --no-build

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test MedicareApi.Tests

# List all discovered tests without running
dotnet test --list-tests
```

## 🚀 Continuous Integration & Deployment

This project includes two GitHub Actions workflows for automated CI/CD:

### 📋 CI Pipeline (`.github/workflows/ci.yml`)
- **Runs automatically** on every push and pull request
- **Executes the full test suite** with detailed reporting
- **Generates code coverage reports** with trend analysis
- **Enforces quality gates** with minimum coverage thresholds
- **Provides test artifacts** for debugging failed test runs

### 🚢 Release Deployment (`.github/workflows/deploy_release.yml`)
- **Runs daily at midnight UTC** or can be triggered manually
- **Finds latest successful commit** from main branch
- **Creates/manages release branch** automatically
- **Increments version numbers** in `yy.mm.build_number.0` format
- **Builds, tests, and deploys** to production environment
- **Manages version files** (`build_number.txt`, `version.txt`)

### CI Test Execution

The CI pipeline runs:
```bash
dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
```

## 🤝 Contributing

When contributing to this project:

1. **Run tests locally** before submitting PRs:
   ```bash
   dotnet test --verbosity detailed
   ```

2. **Ensure coverage standards** are maintained:
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```

3. **Add tests for new features** following existing patterns in the test project

4. **Update documentation** for any API changes or new endpoints

## 📚 Additional Resources

- [ASP.NET Core Testing Documentation](https://docs.microsoft.com/en-us/aspnet/core/test/)
- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore/cmdline)
- [Moq Framework Guide](https://github.com/moq/moq4/wiki/Quickstart)
- [Entity Framework In-Memory Testing](https://docs.microsoft.com/en-us/ef/core/testing/testing-without-the-database)

---

For questions about the API or testing setup, please check the existing issues or create a new issue with detailed information about your problem.