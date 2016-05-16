using System;
using UnityEngine;
using System.Collections.Generic;

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
	public class GetRoles
	{
		public const string msgname = "GetRoles";
		public string userId;

		public GetRoles(string userId){
			this.userId = userId;
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

	[System.Serializable]
	public class PlayerData
	{
		public const string msgname = "PlayerData";
		public string userId;
		public int roleId;
		public List<double> data;

		public PlayerData(string userId, int roleId, GameObject player){
			this.userId = userId;
			this.roleId = roleId;
			data = new List<double> ();
			float HP = player.GetComponent<Health> ().HP;
			Vector3 postion = player.transform.position;
			data.Add (HP); data.Add (postion.x);data.Add (postion.y);
		}
	}

	[System.Serializable]
	public class EnemyData
	{
		public const string msgname = "EnemyData";
		public string userId;
		public List<double> data;

		public EnemyData(string userId, int id, GameObject enemy){
			this.userId = userId;
			data = new List<double> ();
			float HP = enemy.GetComponent<Health> ().HP;
			Vector3 postion = enemy.transform.position;
			int type = 0;
			if ("Spider".Equals (enemy.tag))
				type = 0;
			else if ("Mech".Equals (enemy.tag))
				type = 1;
			data.Add (type); data.Add (id); data.Add (HP); data.Add (postion.x);data.Add (postion.y);
		}
	}

	[System.Serializable]
	public class Damage
	{
		public const string msgname = "Damage";
		public int type;
		public int victim;
		public int id;

		public Damage(int type, int victim, int id = -1){
			this.type = type;
			this.victim = victim;
			this.id = id;
		}
	}

	[System.Serializable]
	public class EnterGame
	{
		public const string msgname = "EnterGame";
		public string userId;
		public int roleId;

		public EnterGame(string userId, int roleId){
			this.userId = userId;
			this.roleId = roleId;
		}
	}
}

