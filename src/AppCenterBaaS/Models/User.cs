using System;

namespace AppCenterBaaS.Models
{
    public class User
    {
        public User(string name, string email, string number)
        {
            Name = name;
            Email = email;
            PhoneNumber = number;
        }

        public string Name { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public Guid Id { get; set; } = Guid.NewGuid(); // generates random id
    }
}
