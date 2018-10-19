using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeltExam.Models
{
    public class Like
    {
        [Key]
        public int LikeId { get; set; }

        public DateTime CreatedAt {get;set;} = DateTime.Now;

        public DateTime UpdatedAt {get;set;} = DateTime.Now;

        public int UserId { get; set; }

        public int PostId { get; set; }

        [ForeignKey("UserId")]
        public User LikeUser { get; set; }

        [ForeignKey("PostId")]
        public Post LikePost { get; set; }
    }
}