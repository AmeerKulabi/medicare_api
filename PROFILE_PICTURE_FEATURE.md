# Doctor Profile Picture Feature Documentation

## Overview
This feature allows doctors to upload optional profile pictures during registration and in their dashboard. Profile pictures are visible in patient search results.

## API Endpoints

### 1. Upload Profile Picture
- **Endpoint**: `POST /api/upload/profile-picture`
- **Authentication**: Required (Bearer token, isDoctor=true)
- **Content-Type**: `multipart/form-data`
- **Parameters**: 
  - `file`: Image file (JPG, JPEG, PNG, GIF)
- **File Constraints**:
  - Maximum size: 5MB
  - Allowed formats: .jpg, .jpeg, .png, .gif
- **Response**: 
  ```json
  {
    "message": "Profile picture uploaded successfully",
    "profilePictureUrl": "/profile_pictures/filename.jpg"
  }
  ```

### 2. Delete Profile Picture
- **Endpoint**: `DELETE /api/upload/profile-picture`
- **Authentication**: Required (Bearer token, isDoctor=true)
- **Response**: 
  ```json
  {
    "message": "Profile picture deleted successfully"
  }
  ```

### 3. Get Doctor Information (Updated)
- **Endpoint**: `GET /api/doctors` or `GET /api/doctors/{id}`
- **Authentication**: Not required
- **Response**: Now includes `profilePictureUrl` field
  ```json
  {
    "id": "doctor-id",
    "name": "Doctor Name",
    "specialization": "Cardiology",
    "profilePictureUrl": "/profile_pictures/default-silhouette.svg",
    // ... other fields
  }
  ```

### 4. Update Profile (Updated)
- **Endpoint**: `PUT /api/profile`
- **Authentication**: Required (Bearer token, isDoctor=true)
- **Body**: Can now include `profilePictureUrl` field
  ```json
  {
    "email": "doctor@example.com",
    "profilePictureUrl": "/profile_pictures/custom-image.jpg",
    // ... other fields
  }
  ```

## Static File Serving

### Default Profile Picture
- **URL**: `/profile_pictures/default-silhouette.svg`
- **Description**: Default silhouette image shown when doctor has no profile picture

### Custom Profile Pictures
- **URL Pattern**: `/profile_pictures/{filename}`
- **Storage Location**: `wwwroot/profile_pictures/`
- **File Naming**: UUID-based to prevent conflicts

## Implementation Details

### Database Changes
- Added `ProfilePictureUrl` field to `Doctor` model (nullable string)
- Applied migration to update database schema

### File Management
- Images stored in `wwwroot/profile_pictures/` directory
- Automatic cleanup: old images deleted when new ones uploaded
- Unique filenames generated using GUID to prevent conflicts
- File validation for security and size constraints

### Security Features
- Authentication required for upload/delete operations
- Only doctors can upload profile pictures
- File type validation (whitelist approach)
- File size limits to prevent abuse
- Secure file storage in web-accessible directory

## Frontend Integration

### For Registration Forms
1. Add file input for profile picture (optional)
2. Use multipart form upload to `/api/upload/profile-picture`
3. Handle success/error responses appropriately

### For Dashboard/Profile Editing
1. Display current profile picture or default silhouette
2. Provide upload button for new images
3. Provide delete button to remove current image
4. Use the upload/delete endpoints as needed

### For Patient Search Results
1. Display `profilePictureUrl` from doctor data
2. Show default silhouette if no custom image
3. Ensure images are properly cached for performance

## Example Usage

### Upload Profile Picture (curl)
```bash
curl -X POST http://localhost:5058/api/upload/profile-picture \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@profile-picture.jpg"
```

### Get Doctor with Profile Picture
```bash
curl http://localhost:5058/api/doctors/doctor-id
```

## Testing
- All endpoints tested and working
- File validation working correctly
- Authentication properly enforced
- Default images served correctly
- Database migrations applied successfully