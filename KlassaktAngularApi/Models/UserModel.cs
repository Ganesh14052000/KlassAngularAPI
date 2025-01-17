namespace KlassaktAngularApi.Models
{
    public class UserModel
    {
        // Corresponds to the 'Name' column in the database
        public required string Name { get; set; }

        // Corresponds to the 'Gender' column in the database
        public required string Gender { get; set; }

        // Corresponds to the 'Address' column in the database
        public string Address { get; set; }

        // Corresponds to the 'Course' column in the database
        public string Course { get; set; }

        // Corresponds to the 'is_active' column in the database (bit type)
        public bool IsActive { get; set; }

        // Corresponds to the 'Role' column in the database
        public string Role { get; set; }

        // Corresponds to the 'Email' column in the database
        public string Email { get; set; }

        // Corresponds to the 'LoginName' column in the database
        public string LoginName { get; set; }
    }
}
