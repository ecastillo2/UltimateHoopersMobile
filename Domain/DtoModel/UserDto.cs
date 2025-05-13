using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DtoModel
{
    public class UserDto
    {
        public string UserId { get; set; }
        public string AccessLevel { get; set; }
        public string Token { get; set; }
        //public User User { get; set; }
    }
}
