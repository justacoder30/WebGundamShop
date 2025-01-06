﻿using Microsoft.AspNetCore.Identity;

namespace WebGundamShop.Models
{
	public class AppUserModel : IdentityUser
	{
		public string Occupation { get; set; }
		public string RoleId { get; set; }
		public string Token { get; set; }
	}
}