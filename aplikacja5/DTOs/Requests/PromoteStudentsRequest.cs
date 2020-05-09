using System;
using System.ComponentModel.DataAnnotations;


namespace aplikacja5.DTOs.Requests
{
    public class PromoteStudentsRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [Range(1, 7)]
        public int Semester { get; set; }
    }
}
