# Security Policy & Implementation Guide

## Overview
This document outlines the security controls, policies, and secure coding practices implemented in the Medicare API. All developers must follow these guidelines when working with sensitive healthcare data.

## Authentication & Session Management

### Password Security
- **Password Requirements**: Minimum 8 characters with uppercase, lowercase, digit, and special character
- **Storage**: Passwords are hashed using ASP.NET Identity's bcrypt implementation
- **Account Lockout**: 5 failed login attempts trigger a 5-minute lockout
- **Implementation**: Configured in `Program.cs` Identity options

### JWT Token Management
- **Secret Storage**: JWT signing keys are stored in user secrets (development) or environment variables (production)
- **Token Expiration**: Access tokens expire after 60 minutes (configurable via `Jwt:AccessTokenExpirationMinutes`)
- **Clock Skew**: Set to zero for strict token validation
- **Algorithm**: HMAC-SHA256 for token signing
- **Claims**: Include user ID, email, and role information

### Configuration Example
```json
{
  "Jwt": {
    "Issuer": "medicare.app",
    "AccessTokenExpirationMinutes": 60
  }
}
```

**Environment Variables (Production):**
```bash
JWT_SECRET_KEY=<secure-256-bit-base64-key>
```

## Protected Health Information (PHI) Security

### Data Access Control
- **Authorization**: All PHI endpoints require valid JWT authentication
- **Role-Based Access**: Doctors can only access their own patient data
- **User Context**: Claims-based authorization ensures users only access authorized data

### Encryption Standards
- **In Transit**: HTTPS/TLS 1.2+ enforced for all endpoints in production
- **At Rest**: Database encryption should be enabled at the database level
- **Recommendations**: 
  - Use Azure SQL Database with Transparent Data Encryption (TDE)
  - Or SQLite with SQLCipher for file-based encryption

### PHI Handling Best Practices
```csharp
// Always validate user context before accessing PHI
var userId = User.FindFirst("uid")?.Value;
if (string.IsNullOrEmpty(userId)) return Unauthorized();

// Filter data by user authorization
var doctorData = await _db.Doctors
    .Where(d => d.UserId == userId)
    .FirstOrDefaultAsync();
```

## Input Validation & Sanitization

### Validation Implementation
All user inputs are validated using Data Annotations and server-side validation:

```csharp
[Required(ErrorMessage = "Email is required")]
[EmailAddress(ErrorMessage = "Invalid email format")]
[StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
public string Email { get; set; } = string.Empty;
```

### File Upload Security
- **Allowed Types**: JPEG, JPG, PNG, GIF only
- **Size Limits**: Maximum 5MB per file  
- **Path Validation**: Prevent directory traversal attacks
- **Content Validation**: Verify file headers match extensions
- **Filename Sanitization**: Remove unsafe characters and path traversal sequences from filenames
- **Extension Blocking**: Block dangerous executable extensions (.exe, .js, .bat, .vbs, .asp, .php, etc.)
- **Execution Prevention**: Web.config files prevent script execution in upload directories
- **Secure File Naming**: Generate secure filenames with sanitized user input and unique identifiers

### SQL Injection Prevention
- **Entity Framework**: Use parameterized queries and LINQ
- **Avoid**: Raw SQL queries or string concatenation
- **Example**: 
```csharp
// Secure: Using EF Core LINQ
var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

// Insecure: Avoid this
// var query = $"SELECT * FROM Users WHERE Email = '{userEmail}'";
```

## Rate Limiting

### Global Rate Limiting
- **Limit**: 100 requests per minute per IP address
- **Implementation**: ASP.NET Core built-in rate limiting middleware
- **Response**: HTTP 429 (Too Many Requests) when exceeded

### Configuration
```csharp
options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
    httpContext => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        factory: partition => new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1)
        }));
```

## Security Headers

### Implemented Headers
- **X-Content-Type-Options**: `nosniff` - Prevents MIME type sniffing
- **X-Frame-Options**: `DENY` - Prevents clickjacking attacks
- **X-XSS-Protection**: `1; mode=block` - Enables XSS filtering
- **Referrer-Policy**: `strict-origin-when-cross-origin` - Controls referrer information

### HTTPS Enforcement
- **Production**: Automatic HTTPS redirection enabled
- **Development**: HTTPS optional for local testing
- **Headers**: Secure cookies and HSTS should be configured in production

## Developer Guidelines

### Secure Coding Practices

1. **Never hardcode secrets** in source code
2. **Always validate user input** on both client and server side
3. **Use parameterized queries** to prevent SQL injection
4. **Implement proper error handling** without exposing sensitive information
5. **Log security events** for monitoring and auditing

### Code Review Checklist
- [ ] No hardcoded credentials or sensitive data
- [ ] Proper input validation and sanitization
- [ ] Authorization checks for PHI access
- [ ] Error handling that doesn't leak information
- [ ] Secure file handling for uploads
- [ ] Rate limiting considerations for new endpoints

### Environment Configuration

#### Development
```bash
# Set JWT secret in user secrets
dotnet user-secrets set "Jwt:Key" "$(openssl rand -base64 32)"
```

#### Production
```bash
# Environment variables
export JWT_SECRET_KEY="<secure-256-bit-base64-key>"
export ASPNETCORE_ENVIRONMENT="Production"
```

### Database Security
- **Connection Strings**: Store in user secrets or environment variables
- **Principle of Least Privilege**: Database user should have minimal required permissions
- **Encryption**: Enable TDE or similar encryption at rest
- **Backups**: Ensure encrypted backups for PHI data

## Incident Response

### Security Event Monitoring
- Monitor failed login attempts
- Track unusual API usage patterns
- Log unauthorized access attempts
- Alert on rate limiting violations

### Data Breach Procedures
1. **Immediate containment** of the security incident
2. **Assessment** of affected PHI and users
3. **Notification** of relevant authorities within required timeframes
4. **Documentation** of incident and remediation steps

## Compliance Notes

### HIPAA Considerations
- This API handles Protected Health Information (PHI)
- Ensure Business Associate Agreements are in place
- Implement audit logging for PHI access
- Regular security risk assessments required

### Regular Security Tasks
- **Weekly**: Review access logs and failed authentication attempts
- **Monthly**: Update dependencies and security patches
- **Quarterly**: Security penetration testing
- **Annually**: Complete security risk assessment

## Contact
For security concerns or to report vulnerabilities, contact the development team through secure channels.