using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
        [Key]
        public int WeddingId { get; set; }

        [Required]
        [MinLength(4)]
        [Display(Name = "Person 1:")]
        public string PersonOne { get; set; }
        [Required]
        [MinLength(4)]
        [Display(Name = "Person 2:")]
        public string PersonTwo { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name= "Event Date:")]
        [FutureDate]
        public DateTime EventDate { get; set; }
        
        [Required]
        [MinLength(4)]
        [Display(Name = "Event Address:")] 
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UserId { get; set; }

        [NotMapped]
        public string ActionName { get; set; }
        //Navigation Property
        public User Planner { get; set; }
        public List<Guest> Guests { get; set; }
    }
}
    public class FutureDateAttribute : ValidationAttribute 
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime dateofevent =  (DateTime)value;  

            if(dateofevent<=DateTime.Today){
                return new ValidationResult("Date must be greater than today. ");
            }
            return ValidationResult.Success;
        }
    }