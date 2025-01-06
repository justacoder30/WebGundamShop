using System.ComponentModel.DataAnnotations;

namespace WebGundamShop.Models.ViewModel
{
	public class LoginViewModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Vui lòng nhập UserName!")]
		public string Username { get; set; }
		[DataType(DataType.Password), Required(ErrorMessage = "Vui lòng nhập Password!")]
		public string Password { get; set; }
		public string ReturnUrl { get; set; }
	}
}
