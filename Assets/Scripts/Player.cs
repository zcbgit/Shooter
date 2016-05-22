using UnityEngine;
using System.Collections.Generic;
using LitJson;

public class Player{
	public string userId;
	public List<Role> roles;
	public int selectedRole;
	public int roleId;

	private SocketConnector connector;
	private bool isLogined;

	private static Player instance = new Player ();

	Player(){
		isLogined = false;
		userId = "";
		selectedRole = -1;
		roles = new List<Role> ();
		connector = SocketConnector.GetInstance ();
	}

	public static Player GetInstance(){
		return instance;
	}

	public bool IsConnected(){
		return connector != null && connector.IsConnected ();
	}

	public void LevelUp(JsonData data) {
		
	}

	public bool IsLogined(){
		return IsConnected () && isLogined;
	}

	public bool Connect(string ip, int port){
		return connector.Connect (ip, port);
	}

	public void Send(string str){
		connector.Send (str);
	}

	public JsonData Receive(){
		if (connector.messages.Count > 0) {
			JsonData data = connector.messages [0];
			connector.messages.RemoveAt (0);
			return data;
		}
		return null;
	}


	public void Login(string userId) {
		this.userId = userId;
		this.isLogined = true;
		selectedRole = -1;
	}

	public void Logout(){
		this.userId = "";
		this.isLogined = false;
		selectedRole = -1;
		roles.Clear ();
	}

	public void SetRoles(JsonData data){
		roles.Clear();
		for (int i = 0; i < data.Count; ++i) {
			JsonData d = data [i];
			Role r = new Role ();
			r.id = (int)d ["roleid"];
			r.name = (string)d ["name"];
			r.level = (int)d ["level"];
			r.maxHP = (int)d ["HP"];
			r.HP = r.maxHP;
			r.exp = (int)d ["EXP"];
			r.nextLevelExp = (int)d ["NextLevelExp"];
			r.weapon = (string)d ["weapon"];
			r.attack = (int)d ["attack"];
			r.ammunition = (int)d ["ammunition"];
			Debug.Log (r);
			roles.Add (r);
		}
	}

	public void Disconnect(){
		Logout ();
		connector.Disconnect ();
	}

	public void Closed(){
		connector.Closed ();
	}

	public Role GetSelectedRole() {
		if (selectedRole >= 0 && selectedRole < 3)
			return roles [selectedRole];
		else
			return null;
	}
}
