namespace ImportData.IntegrationServicesClient.Models
{
    [EntityName("Город")]
    public class ICities
    {
        public string Name { get; set; }
		public IRegions Region { get; set; }
		public ICountries Country { get; set; }
        public string Status { get; set; }
        public int Id { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
