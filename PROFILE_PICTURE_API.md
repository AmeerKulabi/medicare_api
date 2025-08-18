# Profile Picture Upload Feature - API Documentation

This document describes the profile picture upload functionality implemented for the Medicare API.

## Overview

The profile picture upload feature allows doctors to:
1. Upload a profile picture during registration
2. Update their profile picture through the profile editing endpoint
3. Have a default silhouette image if no picture is uploaded

## API Endpoints

### 1. Upload Profile Picture
**POST** `/api/profile/upload-picture`

**Authentication:** Bearer Token required
**Content-Type:** `multipart/form-data`

**Parameters:**
- `profilePicture` (file): Image file (JPEG, JPG, PNG, GIF)
- Maximum file size: 5MB

**Response:**
```json
{
  "message": "Profile picture uploaded successfully",
  "profilePictureUrl": "/profile_pictures/doctor-id_unique-id.jpg"
}
```

**Example using curl:**
```bash
curl -X POST "http://localhost:5058/api/profile/upload-picture" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "profilePicture=@/path/to/your/image.jpg"
```

### 2. Update Doctor Registration (with Profile Picture)
**PUT** `/api/doctors`

**Authentication:** Bearer Token required
**Content-Type:** `multipart/form-data`

**Parameters:**
- All existing doctor registration fields
- `profilePicture` (file, optional): Image file (JPEG, JPG, PNG, GIF)

**Example using curl:**
```bash
curl -X PUT "http://localhost:5058/api/doctors" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "profilePicture=@/path/to/your/image.jpg" \
  -F "Specialization=Cardiology" \
  -F "Phone=+1234567890" \
  -F "Gender=Male" \
  # ... other form fields
```

### 3. Get Doctor Profile
**GET** `/api/profile`

**Authentication:** Bearer Token required

**Response includes:**
```json
{
  "id": "doctor-id",
  "name": "Dr. John Doe",
  "email": "john@example.com",
  "profilePictureUrl": "/profile_pictures/doctor-id_unique-id.jpg",
  // ... other doctor fields
}
```

### 4. Get Doctors List
**GET** `/api/doctors`

Each doctor in the response includes:
```json
{
  "id": "doctor-id",
  "name": "Dr. John Doe",
  "profilePictureUrl": "/profile_pictures/default-silhouette.svg",
  // ... other doctor fields
}
```

## File Validation

- **Allowed file types:** JPEG, JPG, PNG, GIF
- **Maximum file size:** 5MB
- **File naming:** `{doctorId}_{uniqueGuid}.{extension}`

## Default Profile Picture

If a doctor has no uploaded profile picture, the API automatically returns:
```
/profile_pictures/default-silhouette.svg
```

## File Storage

- Profile pictures are stored in: `wwwroot/profile_pictures/`
- Old profile pictures are automatically deleted when new ones are uploaded
- Static files are served directly by the ASP.NET Core application

## Error Responses

### Invalid file type:
```json
{
  "error": "Invalid file type. Only JPEG, JPG, PNG, and GIF files are allowed."
}
```

### File too large:
```json
{
  "error": "File size too large. Maximum size is 5MB."
}
```

### No file uploaded:
```json
{
  "error": "No file uploaded"
}
```

### Unauthorized:
```json
{
  "error": "Unauthorized"
}
```

## Frontend Integration

For the frontend (Ghadeer635/medicare), you can use FormData to upload files:

```javascript
// Example JavaScript code for uploading profile picture
const uploadProfilePicture = async (file, token) => {
  const formData = new FormData();
  formData.append('profilePicture', file);
  
  const response = await fetch('/api/profile/upload-picture', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });
  
  return await response.json();
};

// Example for registration with profile picture
const registerWithProfilePicture = async (doctorData, profilePicture, token) => {
  const formData = new FormData();
  
  // Add all doctor registration fields
  Object.keys(doctorData).forEach(key => {
    formData.append(key, doctorData[key]);
  });
  
  // Add profile picture if provided
  if (profilePicture) {
    formData.append('profilePicture', profilePicture);
  }
  
  const response = await fetch('/api/doctors', {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });
  
  return await response.json();
};
```

## Testing

The implementation has been tested for:
- ✅ Static file serving (default silhouette)
- ✅ API endpoints availability (returns 401 without auth as expected)
- ✅ Swagger UI accessibility
- ✅ Default profile picture URLs in doctor responses

To test with authentication, register/login as a doctor and use the provided JWT token.