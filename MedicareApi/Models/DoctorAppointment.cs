namespace MedicareApi.Models
{
    public class DoctorAppointment
    {
        public string id { get; set; }
        public string patientName{  get; set; }
        public string patientEmail { get; set; }
        public string patientPhone { get; set; }
        public string date {  get; set; }
        public string time { get; set; }
        public string duration { get; set; }
        public string reason { get; set; }
        public string status { get; set; }
        public string type { get; set; }
    }
}
