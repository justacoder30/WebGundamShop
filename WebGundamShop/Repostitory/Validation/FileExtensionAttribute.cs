using System.ComponentModel.DataAnnotations;

namespace WebGundamShop.Repostitory.Validation
{
	public class FileExtensionAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			if (value is FormFile file)
			{
				var extension = Path.GetExtension(file.FileName);
				string[] extensions = { "jpg", "png", "jpeg" };

				bool result = extension.Any(x=>extension.EndsWith(x));

				if (!result)
				{
					return new ValidationResult("Allow extentions are jpg or png or jpeg");
				}
			}
			return ValidationResult.Success;
		}
	}
}
