# Analytics Implementation Summary

This document provides an overview of the analytics features added to the Medicare API project.

## Overview

Azure Application Insights integration has been added to the Medicare API to track user activities and system metrics. This enables comprehensive monitoring of the application's usage patterns and performance.

## What Was Implemented

### 1. Analytics Service (`MedicareApi/Services/AnalyticsService.cs`)

A new `AnalyticsService` class was created that uses Azure Application Insights TelemetryClient to track events and metrics. The service includes:

**Interface:** `IAnalyticsService` - Defines the contract for analytics tracking

**Implementation:** `AnalyticsService` - Concrete implementation using Application Insights

**Tracked Events:**
- User sign-in
- User registration
- Appointment creation
- Appointment updates
- Appointment deletion
- Doctor search (with query parameters and result count)
- Doctor details viewed
- Doctor profile updates
- Doctor profile picture updates
- Daily system metrics (doctor and patient counts)

### 2. Daily Metrics Background Service (`MedicareApi/BackgroundServices/DailyMetricsService.cs`)

A background service that runs continuously and automatically sends daily metrics at midnight (00:00 UTC):
- Total number of doctors
- Total number of patients

The service calculates the time until midnight and schedules the next metric collection automatically.

### 3. Controller Integration

Analytics tracking has been integrated into the following controllers:

**AuthController:**
- Tracks user registration events
- Tracks user sign-in events

**DoctorAppointmentsController:**
- Tracks appointment creation
- Tracks appointment updates
- Tracks appointment deletion

**PatientAppointmentController:**
- Tracks appointment deletion (from patient side)

**DoctorsController:**
- Tracks doctor search with filters and result count
- Tracks doctor details views

**ProfileController:**
- Tracks doctor profile updates
- Tracks doctor profile picture updates

### 4. Configuration

**Dependencies Added:**
- `Microsoft.ApplicationInsights.AspNetCore` (v2.23.0) - Main Application Insights package

**Configuration in `appsettings.json`:**
```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=your-instrumentation-key-here;IngestionEndpoint=https://your-region.in.applicationinsights.azure.com/"
  }
}
```

**Service Registration in `Program.cs`:**
- Application Insights telemetry
- AnalyticsService (scoped)
- DailyMetricsService (hosted service)

### 5. Testing

**New Test Suite:** `MedicareApi.Tests/Services/AnalyticsServiceTests.cs`
- 10 unit tests covering all analytics service methods
- All tests passing ✅

**Updated Test Files:**
- All existing controller tests updated to mock the `IAnalyticsService` dependency
- No regression in existing functionality

## How to Use

### 1. Configure Application Insights

1. Create an Application Insights resource in Azure Portal
2. Copy the connection string
3. Update `appsettings.json` with your connection string:

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=xxxxx;IngestionEndpoint=https://xxxx.in.applicationinsights.azure.com/"
  }
}
```

### 2. View Analytics Data

Once configured, analytics data will be sent to Azure Application Insights where you can:
- View custom events in the Events blade
- Create custom dashboards
- Set up alerts based on metrics
- Analyze user behavior patterns
- Track application performance

### 3. Custom Queries

You can query the tracked events using Kusto Query Language (KQL) in Application Insights:

```kql
// View all user sign-ins
customEvents
| where name == "UserSignIn"
| project timestamp, userId = customDimensions.UserId, userType = customDimensions.UserType

// View doctor searches with filters
customEvents
| where name == "DoctorSearch"
| project timestamp, searchQuery = customDimensions.SearchQuery, specialization = customDimensions.Specialization, resultsCount = customDimensions.ResultsCount

// View daily metrics
customEvents
| where name == "DailyMetrics"
| project timestamp, doctorCount = customMetrics.DoctorCount, patientCount = customMetrics.PatientCount
```

## Analytics Events Reference

| Event Name | Properties | Description |
|-----------|-----------|-------------|
| UserSignIn | UserId, UserType | User logs into the system |
| UserRegistration | UserId, UserType | New user registers |
| AppointmentCreated | AppointmentId, PatientId, DoctorId | New appointment created |
| AppointmentUpdated | AppointmentId | Appointment details updated |
| AppointmentDeleted | AppointmentId | Appointment cancelled |
| DoctorSearch | SearchQuery, Specialization, Location, ResultsCount | User searches for doctors |
| DoctorDetailsViewed | DoctorId | User views doctor profile |
| DoctorProfileUpdated | DoctorId | Doctor updates their profile |
| DoctorProfilePictureUpdated | DoctorId | Doctor updates profile picture |
| DailyMetrics | DoctorCount, PatientCount | Daily system statistics |

## Benefits

1. **Usage Insights** - Understand how users interact with the system
2. **Performance Monitoring** - Track API performance and identify bottlenecks
3. **User Behavior** - Analyze search patterns and user preferences
4. **System Growth** - Monitor daily metrics for doctors and patients
5. **Data-Driven Decisions** - Use analytics to improve features and user experience
6. **Anomaly Detection** - Identify unusual patterns or potential issues
7. **Business Intelligence** - Generate reports on system usage and growth

## Future Enhancements

Potential improvements to the analytics system:
- Track appointment completion rates
- Monitor response times for API endpoints
- Track error rates and failure patterns
- Add user session tracking
- Implement custom metrics for business KPIs
- Add geographical tracking for user locations
- Track feature adoption rates

## Notes

- The analytics service is non-blocking and uses try-catch to ensure failures don't affect the main application flow
- All analytics calls are logged for debugging purposes
- The daily metrics service runs continuously and automatically recovers from failures
- Analytics data is sent asynchronously to minimize impact on API response times
