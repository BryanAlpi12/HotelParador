using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelParador.Models;
    public class User
    {
        public string email { get; set; }
        public string username { get; set; }
        public string password { get; set; }
       


    }
