namespace MedicareApi.Models
{
    public class PatientAppointment
    {
        public string id { get; set; }
        public string doctorName { get; set; }
        public string doctorId { get; set; }
        public string doctorSpecialization { get; set; }
        public string clinicName { get; set; }
        public string date {  get; set; }
        public string time { get; set; }
        public int duration { get; set; }
        public string reason { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public int consultationFee { get; set; }

    }
}
