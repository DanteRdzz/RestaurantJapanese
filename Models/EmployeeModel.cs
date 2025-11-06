namespace RestaurantJapanese.Models
{
    public class EmployeeModel
    {
        public int IdEmployee { get; set; }
        public int? IdUser { get; set; }                // puede ser null si no tiene acceso
        public string FullName { get; set; } = "";
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string Role { get; set; } = "Empleado";  // "Admin" | "Empleado"
        public bool IsActive { get; set; } = true;
        public System.DateTime? CreatedAt { get; set; }
        public System.DateTime? UpdatedAt { get; set; }

        // Campos del usuario (si el SP los devuelve)
        public string? UserName { get; set; }
        public string? DisplayName { get; set; }
        public bool? UserIsActive { get; set; }
    }
}