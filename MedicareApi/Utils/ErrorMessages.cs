using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace MedicareApi.Utils
{
    public static class ErrorMessages
    {
        public const string InvalidInformationSent = "حدث خطا في ارسال المعلومات";
        public const string UserCreationFailed = "حدث الخطا في انشاء السحاب";
        public const string UserDoesNotExistOrPasswordNotCorrect = "User does not exist";
        public const string EmailNotConfirmed = "Email is not confirmed yet";
        public const string UserLoginFailed = "Failure hapened duing log in";
        public const string UserDoesNotExist = "User does not exist";
        public const string EmailConfirmationLinkNotValid = "Email confimation not valid";
        public const string EmailAlreadyConfirmed = "Email is already confirmed";
        public const string PasswordResetFailed = "Password reset failed";
        public const string EmailConfirmationFailed = "Error hapened during email confirmation";
        public const string ResendConfirmationEmailFailed = "Resend confirmation email failed";
        public const string PasswordResetRequestFailed = "Sending password resetting email failed";
        public const string PasswordChangeFailed = "Changing password failed";
        public const string FunctionalityAvailableOnlyForDoctors = "This functionality is available only for doctors";
        public const string AvailabilitySlotNotFound = "Availability is not found";
        public const string EndTimeBeforeStartTime = "وقت البداية يجب يكون قبل وقت النهاية";
        public const string AvailabilitySlotsCouldNotBeRetrieved = "Availability slots could not be retrieved";
        public const string BlockedSlotsCouldNotBeRetrieved = "Blocked slots could not be retrieved";
        public const string AvailabilitySlotCouldNotBeUpdated = "Availability slot could not be updated";
        public const string InvalidDateTimeFormat = "Datetime format is invalid";
        public const string EndAndSrartTimeRequired = "Time slots start adn end time are required when booking the whole day";
        public const string InvalidTimeSpanFormat = "Time span format is not valid";
        public const string BlockFailedDueToExistingAppointments = "Time slot cannot be blocked due to existing appointments during the time being blocked";
        public const string BlockFailedDueToOverlappingBlockedSlots = "This time range overlaps with existing blocked time slots";
        public const string BlockFailed = "Blocking time slot failed";
        public const string BlockedTimeslotNotFound = "Blocked timeslot not found";
        public const string DeletingBlockedTimeSlotFailed = "Error hapened during deleting blocked time slot";
        public const string UpdatingBlockedTimeSlotFailed = "Error hapened duing updating blocked time slot";
        public const string ProfileRetrievingFailed = "Retrieving doctor profile failed";
        public const string ProfileUpdateFailed = "Doctor Profile could not be updated";
        public const string FileNotFound = "File is not found";
        public const string PictureUploadFailed = "Doctor profile picture could not be uploaded";
        public const string FunctionalityOnlyForPatients = "This functionality is availabe only for patients";
        public const string AppointmentNotFound = "Appointment is not found";
        public const string DeletingAppointmentFailed = "Error hapened during appointment deletion";
        public const string RetrievingAppointmentsFailed = "Error hapened during appointments retrieval";
        public const string DoctorsCouldNotBeRetrieved = "Error hapened during retrieving doctors";
        public const string LanguagesCouldNotRetrieved = "Error hapened during retrieving languages";
        public const string DoctorDetailsCouldNotRetrieved = "Error hapened during retrieving doctor details";
        public const string CannotCraeteAppointmentsForOthers = "You cannot create appointments for others";
        public const string CannotCreateAppointmentsForOtherDoctors = "You cannot create appointments for other doctors";
        public const string AppointmentAlreadyBooked = "Appointment is already booked";
        public const string TimeSlotNotAvailableForBooking = "Selected time slot is not available for booking as it is blocked by the doctor";
        public const string TimeSlotCouldNotBeBooked = "Error hapened during booking appointment";
        public const string CannotUpdateAppointmentsOfOtherDoctors = "You cannot update appointments of other doctors";
        public const string AppointmentsUpdateFailed = "Error hapened during appointment update";
        public const string AppointmentsDeletionFailed = "Error hapened during appointment deletion";
        public const string CannotDeleteAppointmentsOfOthers = "You cannot delete appointments of other doctors";
    }

    public static class ErrorActions
    {
        public const string RetryAndSupportConnect = "يرجى اعادة المحاوله وفي حالة استمرار المشكله يرجى اتصال بخدمة العملاء";
        public const string UserDoesNotExistOrPasswordNotCorrect = "Register first please";
        public const string ConfirmEmail = "Confirm your email using the link sent to the email";
        public const string UserDoesNotExist = "Register first please";
        public const string EmailConfirmationLinkNotValid = "Please, sign in and resend confimation email as the current token expired";
        public const string EmailAlreadyConfirmed = "You can use now your email to login";
        public const string PasswordResetFailed = "Choose another password";
        public const string EmailConfirmationFailed = "Please, login and resend new confirmaion email";
        public const string EmptyAction = "";
        public const string EndTimeBeforeStartTime = "Ensure that end time is after start time of the availability slot";
        public const string BlockFailedDueToExistingAppointments = "Remove appointments during the time wanted to be blocked and try to block the time again";
        public const string BlockFailedDueToOverlappingBlockedSlots = "Remove overlapping blocked slot and try blocking the time slot again";
        public const string TimeSlotNotAvailableForBooking = "Check doctor's available time slots and select one of them";
        public const string AppointmentsUpdateFailed = "Error hapened during appointments update";
    }

    public static class ErrorCodes
    {
        public const string ModelStateNotValid = "MODEL_NOT_VALID";
        public const string UserCreationFailed = "USER_CREATION_ERROR";
        public const string UserDoesNotExistOrPasswordNotCorrect = "USER_NOT_REGISTERED_OR_PASSWORD_NOT_CORRECT";
        public const string EmailNotConfirmed = "EMAIL_NOT_CONFIRMED";
        public const string LoginFailed = "LOGIN_FAILED";
        public const string UserDoesNotExist = "USER_NOT_FOUND";
        public const string EmailConfirmationLinkNotValid = "EMAIL_CONFIRMATION_LINK_NOT_VALID";
        public const string EmailAlreadyConfirmed = "EMAIL_ALREADY_CONFIRMED";
        public const string PasswordResetFailed = "PASSWORD_RESET_FAILED";
        public const string EmailConfirmationFailed = "EMAIL_CONFIRMATION_FAILED";
        public const string ResendConfirmationEmailFailed = "RESEND_CONFIRMTIiON_FAILED";
        public const string PasswordResetRequestFailed = "PASSWORD_RESET_REQUEST_FAILED";
        public const string PasswordChangeFailed = "PASSWORD_CHANGE_FAILED";
        public const string FunctionalityAvailableOnlyForDoctors = "ONLY_DOCTORS";
        public const string AvailabilitySlotNotFound = "AVAILABILITY_NOT_FOUND";
        public const string EndTimeBeforeStartTime = "END_SLOTTIME_BEFORE_START_SLOTTIME";
        public const string AvailabilitySlotsCouldNotBeRetrieved = "AVAILABILITY_SLOTS_COULD_NOT_BE_RETRIEVED";
        public const string BlockedSlotsCouldNotBeRetrieved = "BLOCKED_SLOTS_COULD_NOT_BE_RETRIEVED";
        public const string AvailabilitySlotCouldNotBeUpdated = "AVAILABILITY_SLOT_COULD_NOT_BE_UPDATED";
        public const string InvalidDateTimeFormat = "INVALID_DATETIME_FORMAT";
        public const string EndAndSrartTimeRequired = "END_&_START_REQUIRED";
        public const string InvalidTimeSpanFormat = "INVALID_TIMESPAN_FORMAT";
        public const string BlockFailedDueToExistingAppointments = "BLOCKED_FAILED_DUE_TO_EXISTING_APPOINTMENTS";
        public const string BlockFailedDueToOverlappingBlockedSlots = "BLOCKED_FAILED_DUE_TO_OVERLAPPING_SLOTS";
        public const string BlockFailed = "TIMESLOT_BLOCK_FAILED";
        public const string BlockedTimeslotNotFound = "BLOCKED_TIMESLOT_NOT_FOUND";
        public const string DeletingBlockedTimeSlotFailed = "DELETING_BLOCKED_TIMESLOT_FAILED";
        public const string UpdatingBlockedTimeSlotFailed = "UPDATING_BLOCKED_TIMESLOT_FAILED";
        public const string ProfileRetrievingFailed = "PROFILE_RETRIEVING_FAILED";
        public const string ProfileUpdateFailed = "PROFILE_UPDATE_FAILED";
        public const string FileNotFound = "FILE_NOT_FOUND";
        public const string PictureUploadFailed = "PROFILE_PICTURE_UPLOAD_FAILED";
        public const string FunctionalityOnlyForPatients = "ONLY_FOR_PATIENTS";
        public const string AppointmentNotFound = "APPOINTMENT_NOT_FOUND";
        public const string DeletingAppointmentFailed = "DELETING_APPOINTMENT_FAILED";
        public const string RetrievingAppointmentsFailed = "APPOINTMENTS_RETRIEVAL_FAILED";
        public const string DoctorsCouldNotBeRetrieved = "DOCTORS_RETRIEVAL_FAILED";
        public const string LanguagesCouldNotRetrieved = "LANGUAGES_RETRIEVAL_FAILED";
        public const string DoctorDetailsCouldNotRetrieved = "DOCTOR_DETAILS_RETRIVEVAL_FAILED";
        public const string CannotCraeteAppointmentsForOthers = "CANNOT_CREATE_APPOINTMENTS_FOR_OTHERS";
        public const string CannotCreateAppointmentsForOtherDoctors = "CANNOT_CREATE_APPOINTMENTS_FOR_OTHER_DOCTORS";
        public const string AppointmentAlreadyBooked = "APPOINTMENT_ALREADY_BOOKED";
        public const string TimeSlotNotAvailableForBooking = "TIMESLOT_NOT_AVAILABLE_FOR_BOOKING";
        public const string TimeSlotCouldNotBeBooked = "TIMESLOT_COULD_NOT_BE_BOOKED";
        public const string CannotUpdateAppointmentsOfOtherDoctors = "CANNOT_UPDATE_APPOINTMENTS_OF_OTHER_DOCTORS";
        public const string AppointmentsUpdateFailed = "COULD_NOT_UPDATE_APPOINTMENT";
        public const string AppointmentsDeletionFailed = "COULD_NOT_DELETE_APPOINTMENT";
        public const string CannotDeleteAppointmentsOfOthers = "CANNOT_DELETE_APPOINTMENT_OF_OTHER_DOCTORS";
    }
}
