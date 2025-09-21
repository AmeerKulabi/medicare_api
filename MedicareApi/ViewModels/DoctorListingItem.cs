namespace MedicareApi.ViewModels
{
    public class DoctorListingItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<string> Languages { get; set; }
        public int? Experience { get; set; }
        public int ConsultationFee { get; set; }
        public string Specialization {  get; set; }
        public string SubSpecialization { get; set; }
        public string? ProfilePictureUrl { get; set; }

        public string City { get; set; }

    }
}
