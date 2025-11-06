namespace RestaurantJapanese.Models
{
    public class EmployeeCreateRequest
    {
        // Empleado
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = "Empleado";   // "Admin" | "Empleado"
        public bool IsActive { get; set; } = true;

        // Usuario
        public string UserName { get; set; } = "";
        public string PasswordText { get; set; } = "";
        public string? DisplayName { get; set; }         // si no se manda, el SP usa FullName
    }
}
