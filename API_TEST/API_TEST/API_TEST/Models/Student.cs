using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;
namespace API_TEST.Models
{/// <summary>
/// Add New Student Profile
/// </summary>
    public class Student
    {
      

        [Required]
        [StringLength(50, ErrorMessage = "Student Name cannot exceed 50 characters.")]
        public string StudentName { get; set; }

        [Required]
        [StringLength(10, ErrorMessage = "Student Registration Number cannot exceed 10 characters.")]
        public string StudentRegNo { get; set; }
       
        [FileType(".jpg", ErrorMessage = "Only JPG files are allowed.")]
        [Required (ErrorMessage= "Main Profile Image Mandatory")]
        public IFormFile MainImage { get; set; }

        /// <summary>
        /// optional images
        /// </summary>
        [MaxFileCount(5, ErrorMessage = "You can upload a maximum of 5 secondary images.")]
        [FileType(".jpg", ErrorMessage = "Only JPG files are allowed.")]
        public IFormFile[]?  SecondaryImages { get; set; }

        /// <summary>
        /// optional date of registeration
        /// </summary>
        [DataType(DataType.DateTime)]
        [FutureDate(ErrorMessage = "Registration Date must be in the future.")]
        public DateTime? RegistrationDate { get; set; } 
    }


    public class RetrieveImage 
    {
        [Required]
        public required string Token { get; set; }
        /// <summary>
        /// select image type        0 = Main Image, 1= Secondary Image
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [Description("Select Image Type")]
        [Required]
        public ImageType imageType { get; set; }
    }

    /// <summary>
    ///Image Type Enum
    /// </summary>
    public enum ImageType
    {
        [Display(Name = "Main Image")]
        Main,
        [Display(Name = "Secondary Image")]
        SecondaryImage
    }
}
