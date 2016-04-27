using System;

namespace Messages
{
	[System.Serializable]
	public class Login
	{
		public const string msgname = "Login";
		public string userId;
		public string password;

		public Login(string userId, string password){
			this.userId = userId;
			this.password = password;
		}
	}

	[System.Serializable]
	public class Register
	{
		public const string msgname = "Register";
		public string userId;
		public string password;

		public Register(string userId, string password){
			this.userId = userId;
			this.password = password;
		}
	}

	[System.Serializable]
	public class Echo
	{
		public const string msgname = "Echo";
		public string msg;

		public Echo(string msg){
			this.msg = msg;
		}
	}

	[System.Serializable]
	public class CreateRole
	{
		public const string msgname = "CreateRole";
		public string userId;
		public Role role;

		public CreateRole(string userId, Role role){
			this.userId = userId;
			this.role = role;
		}
	}

	[System.Serializable]
	public class DeleteRole
	{
		public const string msgname = "DeleteRole";
		public string userId;
		public int roleId;

		public DeleteRole(string userId, int roleId){
			this.userId = userId;
			this.roleId = roleId;
		}
	}

}

