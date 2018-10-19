using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeltExam.Models
{
    public class LoginUser
    {
        [EmailAddress]
        public string LoginEmail { get; set; }

        [DataType(DataType.Password)]
        public string LoginPassword { get; set; }
    }
}