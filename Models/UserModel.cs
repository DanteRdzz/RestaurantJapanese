using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantJapanese.Models
{
    public class UserModel
    {
        public int IdUser { get; set; }
        public string UserName { get; set; } = "";
        public string PasswordText { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public int IsActive { get; set; }
        public string Role {  get; set; }
    }
}
