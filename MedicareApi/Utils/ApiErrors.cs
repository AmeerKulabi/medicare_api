using MedicareApi.ViewModels;
using MedicareApi.Utils;

namespace MedicareApi.Utils
{
    public static class ApiErrors
    {
        public static readonly ApiError ModelNotValid = new ApiError()
        {
            errorCode = ErrorCodes.ModelStateNotValid,
            message = ErrorMessages.InvalidInformationSent,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError UserCreationFailed = new ApiError()
        {
            errorCode = ErrorCodes.UserCreationFailed,
            message = ErrorMessages.UserCreationFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError UserDoesNotExistOrPasswordNotCorrect = new ApiError()
        {
            errorCode = ErrorCodes.UserDoesNotExistOrPasswordNotCorrect,
            message = ErrorMessages.UserDoesNotExistOrPasswordNotCorrect,
            action = ErrorActions.UserDoesNotExistOrPasswordNotCorrect
        };

        public static readonly ApiError EmailNotConfirmed = new ApiError()
        {
            errorCode = ErrorCodes.EmailNotConfirmed,
            message = ErrorMessages.EmailNotConfirmed,
            action = ErrorActions.ConfirmEmail
        };

        public static readonly ApiError UserLoginFailed = new ApiError()
        {
            errorCode = ErrorCodes.LoginFailed,
            message = ErrorMessages.UserLoginFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError UserDoesNotExist = new ApiError()
        {
            errorCode = ErrorCodes.UserDoesNotExist,
            message = ErrorMessages.UserDoesNotExist,
            action = ErrorActions.UserDoesNotExist
        };

        public static readonly ApiError EmailConfirmationLinkNotValid = new ApiError()
        {
            errorCode = ErrorCodes.EmailConfirmationLinkNotValid,
            message = ErrorMessages.EmailConfirmationLinkNotValid,
            action = ErrorActions.EmailConfirmationLinkNotValid
        };

        public static readonly ApiError EmailAlreadyConfirmed = new ApiError()
        {
            errorCode = ErrorCodes.EmailAlreadyConfirmed,
            message = ErrorMessages.EmailAlreadyConfirmed,
            action = ErrorActions.EmailAlreadyConfirmed
        };

        public static readonly ApiError PasswordResetFailed = new ApiError()
        {
            errorCode= ErrorCodes.PasswordResetFailed,
            message = ErrorMessages.PasswordResetFailed,
            action = ErrorActions.PasswordResetFailed
        };

        public static readonly ApiError EmailConfirmationFailed = new ApiError()
        {
            errorCode = ErrorCodes.EmailConfirmationFailed,
            message = ErrorMessages.EmailConfirmationFailed,
            action = ErrorActions.EmailConfirmationFailed
        };

        public static readonly ApiError ResendEmailConfirmationFailed = new ApiError()
        {
            errorCode = ErrorCodes.ResendConfirmationEmailFailed,
            message = ErrorMessages.ResendConfirmationEmailFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError PasswordResetRequestFailed = new ApiError()
        {
            errorCode = ErrorCodes.PasswordResetRequestFailed,
            message = ErrorMessages.PasswordResetRequestFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError PasswordChangeFailed = new ApiError()
        {
            errorCode = ErrorCodes.PasswordChangeFailed,
            message = ErrorMessages.PasswordChangeFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError FunctionalityAvailableOnlyForDoctors = new ApiError()
        {
            errorCode = ErrorCodes.FunctionalityAvailableOnlyForDoctors,
            message = ErrorMessages.FunctionalityAvailableOnlyForDoctors,
            action = ErrorActions.EmptyAction
        };

        public static readonly ApiError AvailabilitySlotNotFound = new ApiError()
        {
            errorCode = ErrorCodes.AvailabilitySlotNotFound,
            message = ErrorMessages.AvailabilitySlotNotFound,
            action = ErrorActions.EmptyAction
        };

        public static readonly ApiError EndTimeBeforeStartTime = new ApiError()
        {
            errorCode = ErrorCodes.EndTimeBeforeStartTime,
            message = ErrorMessages.EndTimeBeforeStartTime,
            action = ErrorActions.EndTimeBeforeStartTime
        };

        public static readonly ApiError AvailabilitySlotsCouldNotBeRetrieved = new ApiError()
        {
            errorCode = ErrorCodes.AvailabilitySlotsCouldNotBeRetrieved,
            message = ErrorMessages.AvailabilitySlotsCouldNotBeRetrieved,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError BlockedSlotsCouldNotBeRetrieved = new ApiError()
        {
            errorCode = ErrorCodes.BlockedSlotsCouldNotBeRetrieved,
            message = ErrorMessages.BlockedSlotsCouldNotBeRetrieved,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError InvalidDateTimeFormat = new ApiError()
        {
            errorCode = ErrorCodes.InvalidDateTimeFormat,
            message = ErrorMessages.InvalidDateTimeFormat,
            action = ErrorActions.EmptyAction
        };

        public static readonly ApiError EndAndSrartTimeRequired = new ApiError()
        {
            errorCode = ErrorCodes.EndAndSrartTimeRequired,
            message = ErrorMessages.EndAndSrartTimeRequired,
            action = ErrorActions.EmptyAction
        };

        public static readonly ApiError InvalidTimeSpanFormat = new ApiError()
        {
            errorCode = ErrorCodes.InvalidTimeSpanFormat,
            message = ErrorMessages.InvalidTimeSpanFormat,
            action = ErrorActions.EmptyAction
        };

        public static readonly ApiError BlockFailedDueToExistingAppointments = new ApiError()
        {
            errorCode = ErrorCodes.BlockFailedDueToExistingAppointments,
            message = ErrorMessages.BlockFailedDueToExistingAppointments,
            action = ErrorActions.BlockFailedDueToExistingAppointments
        };

        public static readonly ApiError BlockFailedDueToOverlappingBlockedSlots = new ApiError()
        {
            errorCode = ErrorActions.BlockFailedDueToOverlappingBlockedSlots,
            message = ErrorMessages.BlockFailedDueToOverlappingBlockedSlots,
            action = ErrorActions.BlockFailedDueToOverlappingBlockedSlots
        };

        public static readonly ApiError BlockFailed = new ApiError()
        {
            errorCode = ErrorCodes.BlockFailed,
            message = ErrorMessages.BlockFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError BlockedTimeslotNotFound = new ApiError()
        {
            errorCode = ErrorCodes.BlockedTimeslotNotFound,
            message = ErrorMessages.BlockedTimeslotNotFound,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError DeletingBlockedTimeSlotFailed = new ApiError()
        {
            errorCode = ErrorCodes.DeletingBlockedTimeSlotFailed,
            message = ErrorMessages.DeletingBlockedTimeSlotFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError UpdatingBlockedTimeSlotFailed = new ApiError()
        {
            errorCode = ErrorCodes.UpdatingBlockedTimeSlotFailed,
            message = ErrorMessages.UpdatingBlockedTimeSlotFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError ProfileRetrievingFailed = new ApiError()
        {
            errorCode = ErrorCodes.ProfileRetrievingFailed,
            message = ErrorMessages.ProfileRetrievingFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError ProfileUpdateFailed = new ApiError()
        {
            errorCode = ErrorCodes.ProfileUpdateFailed,
            message = ErrorMessages.ProfileUpdateFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError FileNotFound = new ApiError()
        {
            errorCode = ErrorCodes.FileNotFound,
            message = ErrorMessages.FileNotFound,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError PictureUploadFailed = new ApiError()
        {
            errorCode = ErrorCodes.PictureUploadFailed,
            message = ErrorMessages.PictureUploadFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError FunctionalityOnlyForPatients = new ApiError()
        {
            errorCode = ErrorCodes.FunctionalityOnlyForPatients,
            message = ErrorMessages.FunctionalityOnlyForPatients,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError AppointmentNotFound = new ApiError()
        {
            errorCode = ErrorCodes.AppointmentNotFound,
            message = ErrorMessages.AppointmentNotFound,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError DeletingAppointmentFailed = new ApiError()
        {
            errorCode = ErrorCodes.DeletingAppointmentFailed,
            message = ErrorMessages.DeletingAppointmentFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError RetrievingAppointmentsFailed = new ApiError()
        {
            errorCode = ErrorCodes.RetrievingAppointmentsFailed,
            message = ErrorMessages.RetrievingAppointmentsFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError DoctorsCouldNotBeRetrieved = new ApiError()
        {
            errorCode = ErrorCodes.DoctorsCouldNotBeRetrieved,
            message = ErrorMessages.DoctorsCouldNotBeRetrieved,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError LanguagesCouldNotRetrieved = new ApiError()
        {
            errorCode = ErrorCodes.LanguagesCouldNotRetrieved,
            message = ErrorMessages.LanguagesCouldNotRetrieved,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError DoctorDetailsCouldNotRetrieved = new ApiError()
        {
            errorCode = ErrorCodes.DoctorDetailsCouldNotRetrieved,
            message = ErrorMessages.DoctorDetailsCouldNotRetrieved,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError CannotCraeteAppointmentsForOthers = new ApiError()
        {
            errorCode = ErrorCodes.CannotCraeteAppointmentsForOthers,
            message = ErrorMessages.CannotCraeteAppointmentsForOthers,
            action = ErrorActions.EmptyAction
        };

        public static readonly ApiError CannotCreateAppointmentsForOtherDoctors = new ApiError()
        {
            errorCode = ErrorCodes.CannotCreateAppointmentsForOtherDoctors,
            message = ErrorMessages.CannotCreateAppointmentsForOtherDoctors,
            action = ErrorActions.EmptyAction
        };

        public static readonly ApiError AppointmentAlreadyBooked = new ApiError()
        {
            errorCode = ErrorCodes.AppointmentAlreadyBooked,
            message = ErrorMessages.AppointmentAlreadyBooked,
            action = ErrorActions.EmptyAction
        };

        public static readonly ApiError TimeSlotNotAvailableForBooking = new ApiError()
        {
            errorCode = ErrorCodes.TimeSlotNotAvailableForBooking,
            message = ErrorMessages.TimeSlotNotAvailableForBooking,
            action = ErrorActions.TimeSlotNotAvailableForBooking
        };

        public static readonly ApiError TimeSlotCouldNotBeBooked = new ApiError()
        {
            errorCode = ErrorCodes.TimeSlotCouldNotBeBooked,
            message = ErrorMessages.TimeSlotCouldNotBeBooked,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError CannotUpdateAppointmentsOfOtherDoctors = new ApiError()
        {
            errorCode = ErrorCodes.CannotUpdateAppointmentsOfOtherDoctors,
            message = ErrorMessages.CannotUpdateAppointmentsOfOtherDoctors,
            action = ErrorActions.EmptyAction
        };

        public static readonly ApiError AppointmentsUpdateFailed = new ApiError()
        {
            errorCode = ErrorCodes.AppointmentsUpdateFailed,
            message = ErrorMessages.AppointmentsUpdateFailed,
            action = ErrorActions.RetryAndSupportConnect
        };

        public static readonly ApiError CannotDeleteAppointmentsOfOthers = new ApiError()
        {
            errorCode = ErrorCodes.CannotDeleteAppointmentsOfOthers,
            message = ErrorMessages.CannotDeleteAppointmentsOfOthers,
            action = ErrorActions.EmptyAction
        };

        public static readonly ApiError AppointmentsDeletionFailed = new ApiError()
        {
            errorCode = ErrorCodes.AppointmentsDeletionFailed,
            message = ErrorMessages.AppointmentsDeletionFailed,
            action = ErrorActions.RetryAndSupportConnect
        };
    }
}
