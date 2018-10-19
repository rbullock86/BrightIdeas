using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeltExam.Models
{   
    public class User
    {
        [Key]
        public int UserId {get;set;}

        [Required]
        [MinLength(2)]
        [MaxLength(45)]
        public string FirstName {get;set;}

        [Required]
        [MinLength(2)]
        [MaxLength(45)]  
        public string LastName {get;set;}

        [Required]
        [EmailAddress]
        public string Email {get;set;}

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password {get;set;}

        public DateTime CreatedAt {get;set;} = DateTime.Now;

        public DateTime UpdatedAt {get;set;} = DateTime.Now;

        [NotMapped]
        [Compare("Password", ErrorMessage="Passwords do not match.")]
        [DataType(DataType.Password)]
        public string Confirm {get;set;}

        public List<Like> Likes { get; set; }

        public List<Post> Posts { get; set; }

        public User()
        {
            this.Likes = new List<Like>();
            this.Posts = new List<Post>();
        }
    }
}
