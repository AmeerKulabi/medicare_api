namespace MedicareApi.Models
{
    public class AvailabilitySlot
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DoctorId { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string day {  get; set; }
    }
}
