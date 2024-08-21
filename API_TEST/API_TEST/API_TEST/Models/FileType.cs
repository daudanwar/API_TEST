namespace API_TEST.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class FileTypeAttribute : ValidationAttribute
    {
        private readonly string _allowedExtension=".jpg";

        public FileTypeAttribute(string allowedExtension)
        {
            _allowedExtension = allowedExtension.ToLowerInvariant();
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var files = value as IFormFile[];
            if (files != null)
            {
                foreach (var file in files)
                {
                    var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (fileExtension != _allowedExtension)
                    {
                        return new ValidationResult($"Only {_allowedExtension.ToUpperInvariant()} files are allowed.");
                    }
                }
            }

            return ValidationResult.Success;
        }
    }
}
