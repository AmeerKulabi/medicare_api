using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace MedicareApi.Utils
{
    public static class ErrorMessages
    {
        public const string InvalidInformationSent = "حدث خطأ أثناء إرسال المعلومات.";
        public const string UserCreationFailed = "حدث خطأ أثناء إنشاء الحساب.";
        public const string UserDoesNotExistOrPasswordNotCorrect = "المستخدم غير موجود أو كلمة المرور غير صحيحة.";
        public const string EmailNotConfirmed = "لم يتم تأكيد البريد الإلكتروني بعد.";
        public const string UserLoginFailed = "حدث خطأ أثناء تسجيل الدخول.";
        public const string UserDoesNotExist = "المستخدم غير موجود.";
        public const string EmailConfirmationLinkNotValid = "رابط تأكيد البريد الإلكتروني غير صالح.";
        public const string EmailAlreadyConfirmed = "تم تأكيد البريد الإلكتروني مسبقًا.";
        public const string PasswordResetFailed = "فشل في إعادة تعيين كلمة المرور.";
        public const string EmailConfirmationFailed = "حدث خطأ أثناء تأكيد البريد الإلكتروني.";
        public const string ResendConfirmationEmailFailed = "فشل في إعادة إرسال بريد التأكيد.";
        public const string PasswordResetRequestFailed = "فشل في إرسال بريد إعادة تعيين كلمة المرور.";
        public const string PasswordChangeFailed = "فشل في تغيير كلمة المرور.";
        public const string FunctionalityAvailableOnlyForDoctors = "هذه الخاصية متاحة للأطباء فقط.";
        public const string AvailabilitySlotNotFound = "لم يتم العثور على فترة التوفر.";
        public const string EndTimeBeforeStartTime = "وقت البداية يجب أن يكون قبل وقت النهاية.";
        public const string AvailabilitySlotsCouldNotBeRetrieved = "تعذر استرجاع فترات التوفر.";
        public const string BlockedSlotsCouldNotBeRetrieved = "تعذر استرجاع الفترات المحجوزة.";
        public const string AvailabilitySlotCouldNotBeUpdated = "تعذر تحديث فترة التوفر.";
        public const string InvalidDateTimeFormat = "تنسيق الوقت والتاريخ غير صحيح.";
        public const string EndAndSrartTimeRequired = "يجب تحديد وقت البداية والنهاية عند الحجز طوال اليوم.";
        public const string InvalidTimeSpanFormat = "تنسيق الفترة الزمنية غير صحيح.";
        public const string BlockFailedDueToExistingAppointments = "لا يمكن حجز الفترة الزمنية لوجود مواعيد أخرى خلال الفترة المحجوزة.";
        public const string BlockFailedDueToOverlappingBlockedSlots = "الفترة الزمنية تتداخل مع فترات محجوزة أخرى.";
        public const string BlockFailed = "فشل في حجز الفترة الزمنية.";
        public const string BlockedTimeslotNotFound = "لم يتم العثور على الفترة الزمنية المحجوزة.";
        public const string DeletingBlockedTimeSlotFailed = "حدث خطأ أثناء حذف الفترة الزمنية المحجوزة.";
        public const string UpdatingBlockedTimeSlotFailed = "حدث خطأ أثناء تحديث الفترة الزمنية المحجوزة.";
        public const string ProfileRetrievingFailed = "حدث خطأ أثناء استرجاع ملف الطبيب الشخصي.";
        public const string ProfileUpdateFailed = "تعذر تحديث ملف الطبيب الشخصي.";
        public const string FileNotFound = "الملف غير موجود.";
        public const string PictureUploadFailed = "تعذر رفع صورة ملف الطبيب الشخصي.";
        public const string FunctionalityOnlyForPatients = "هذه الخاصية متاحة للمرضى فقط.";
        public const string AppointmentNotFound = "الموعد غير موجود.";
        public const string DeletingAppointmentFailed = "حدث خطأ أثناء حذف الموعد.";
        public const string RetrievingAppointmentsFailed = "حدث خطأ أثناء استرجاع المواعيد.";
        public const string DoctorsCouldNotBeRetrieved = "حدث خطأ أثناء استرجاع قائمة الأطباء.";
        public const string LanguagesCouldNotRetrieved = "حدث خطأ أثناء استرجاع اللغات.";
        public const string DoctorDetailsCouldNotRetrieved = "حدث خطأ أثناء استرجاع تفاصيل الطبيب.";
        public const string CannotCraeteAppointmentsForOthers = "لا يمكنك إنشاء مواعيد للآخرين.";
    }

    public static class ErrorActions
    {
        public const string RetryAndSupportConnect = "يرجى إعادة المحاولة، وفي حال استمرار المشكلة يرجى التواصل مع خدمة العملاء.";
        public const string UserDoesNotExistOrPasswordNotCorrect = "يرجى التسجيل أولاً.";
        public const string ConfirmEmail = "يرجى تأكيد بريدك الإلكتروني عبر الرابط المرسل إلى بريدك.";
        public const string UserDoesNotExist = "يرجى التسجيل أولاً.";
        public const string EmailConfirmationLinkNotValid = "يرجى تسجيل الدخول وإعادة إرسال بريد التأكيد حيث أن الرمز الحالي منتهي الصلاحية.";
        public const string EmailAlreadyConfirmed = "يمكنك الآن استخدام بريدك الإلكتروني لتسجيل الدخول.";
        public const string PasswordResetFailed = "يرجى اختيار كلمة مرور أخرى.";
        public const string EmailConfirmationFailed = "يرجى تسجيل الدخول وإعادة إرسال بريد تأكيد جديد.";
        public const string EmptyAction = "";
        public const string EndTimeBeforeStartTime = "تأكد أن وقت النهاية بعد وقت البداية لفترة التوفر.";
        public const string BlockFailedDueToExistingAppointments = "يرجى حذف المواعيد خلال الفترة المراد حجزها ثم محاولة الحجز مرة أخرى.";
        public const string BlockFailedDueToOverlappingBlockedSlots = "يرجى حذف الفترات المحجوزة المتداخلة ثم محاولة الحجز مرة أخرى.";
        public const string TimeSlotNotAvailableForBooking = "يرجى مراجعة فترات التوفر للطبيب واختيار واحدة منها.";
        public const string AppointmentsUpdateFailed = "حدث خطأ أثناء تحديث المواعيد.";
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
