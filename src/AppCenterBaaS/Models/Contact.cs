using System;
using Newtonsoft.Json;

namespace AppCenterBaaS.Models
{
    public class Contact
    {
        public Contact()
        {
        }

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; } = Guid.NewGuid(); // generates random id

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "notes")]
        public string Notes { get; set; }

        [JsonProperty(PropertyName = "_ts")]
        public long TimeStamp { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }


        [JsonIgnore]
        public DateTime LastUpdated
        {
            get
            {
                return DateTimeOffset.FromUnixTimeSeconds(TimeStamp).DateTime.ToLocalTime();
            }
        }
    }
}
