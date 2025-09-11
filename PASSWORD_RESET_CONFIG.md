# Password Reset Email Configuration

This document explains how to configure the forgot password email functionality that was added to the Medicare API.

## New Features

### Endpoints Added
- `POST /api/auth/forgot-password` - Sends password reset email
- `POST /api/auth/reset-password` - Resets password using token

### Email Template
- Professional Arabic email template with HTML styling
- RTL (right-to-left) layout for proper Arabic text rendering
- Responsive design that works across email clients
- Modern gradient design with Medicare App branding

## Configuration Required

### Email Settings (appsettings.json)
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "EnableSsl": "true",
    "FromAddress": "noreply@yourdomain.com",
    "FromName": "Your App Name",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  },
  "AppSettings": {
    "AppName": "Medicare App",
    "FrontendUrl": "https://yourdomain.com"
  }
}
```

### Email Provider Setup
For Gmail:
1. Enable 2-factor authentication
2. Generate an app-specific password
3. Use the app password in the configuration

### Frontend Integration
The reset link will be in this format:
```
{FrontendUrl}/reset-password?token={encoded_token}&email={encoded_email}
```

Your frontend should:
1. Parse the token and email from URL parameters
2. Send a POST request to `/api/auth/reset-password` with the decoded values
3. Handle the response appropriately

## API Usage

### Forgot Password Request
```json
POST /api/auth/forgot-password
{
  "email": "user@example.com"
}
```

### Reset Password Request
```json
POST /api/auth/reset-password
{
  "email": "user@example.com",
  "token": "reset_token_from_email",
  "newPassword": "new_password",
  "confirmPassword": "new_password"
}
```

## Security Features
- Tokens are URL-encoded for safe transmission
- Generic success messages to prevent email enumeration
- Password complexity validation
- Token expiration (handled by ASP.NET Identity)

## Testing
- Main project compiles successfully
- Email template generation tested and verified
- Arabic content rendering confirmed
- Placeholder replacement working correctly