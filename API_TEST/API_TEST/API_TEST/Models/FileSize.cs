namespace API_TEST.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.IO;

    public class MaxFileCountAttribute : ValidationAttribute
    {
        private readonly int _maxCount = 5;

        public MaxFileCountAttribute(int maxFileSize)
        {
            _maxCount = maxFileSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var files = value as IFormFile[];
            if (files != null && files.Length > _maxCount)
            {
                return new ValidationResult($"You can upload a maximum of {_maxCount} files.");
            }

            return ValidationResult.Success;
        }
    }
}
