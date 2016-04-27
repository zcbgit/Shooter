using UnityEngine;
using System.Collections;
using System.Reflection;
using LitJson;
using Messages;

public class Processor {

	public static string C2SLogin(string userId, string password)
	{
		Login data = new Login (userId, password);
		return JsonMapper.ToJson (data);
	}

	public static string C2SRegister(string userId, string password)
	{
		Register data = new Register (userId, password);
		return JsonMapper.ToJson (data);
	}

	public static string C2SEcho(string msg)
	{
		Echo data = new Echo (msg);
		return JsonMapper.ToJson (data);
	}

	public static string C2SCreateRole(string userId, Role role){
		CreateRole data = new CreateRole (userId, role);
		return JsonMapper.ToJson (data);
	}

	public static string C2SDeleteRole(string userId, int roleId){
		DeleteRole data = new DeleteRole (userId, roleId);
		return JsonMapper.ToJson (data);		
	}

	public static void Process(JsonData data)
	{
		string msgname = "S2C" + (string)data ["msgname"];
		MethodInfo mi = typeof(Processor).GetMethod (msgname);
		if (mi != null) {
			mi.Invoke (null, new JsonData[]{ data });
		}
	}

	public static void S2CEcho(JsonData data)
	{
		string msg = (string)data ["msg"];
		Debug.Log (msg);
	}

	public static void S2CRespone(JsonData data)
	{
		int errcode = (int)data ["errcode"];
		string errmsg = (string)data ["errmsg"];
		Debug.Log (string.Format ("errcode[{0}], errmsg[{0}]", errcode, errmsg));
	}


}
