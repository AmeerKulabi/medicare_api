namespace MedicareApi.ViewModels
{
    public class ApiError
    {
        public string errorCode { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public string action {  get; set; } = string.Empty;
    }
}
