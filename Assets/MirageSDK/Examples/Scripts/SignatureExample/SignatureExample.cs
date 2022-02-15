﻿using MirageSDK.Core.Implementation;
using MirageSDK.Core.Infrastructure;
using UnityEngine;

namespace MirageSDK.Examples.Scripts.SignatureExample
{
	public class SignatureExample : MonoBehaviour
	{
		private const string Message = "Hahaha!";
		private string _signature;
		private IMirageSDK _mirageSDKWrapper;
	
		private void Start()
		{
			_mirageSDKWrapper = MirageSDKWrapper.GetSDKInstance();
		}
	
		public void Sign1()
		{
			Debug.Log($"Signature: {_signature}");
		}

		public async void Sign()
		{
			Debug.Log(_mirageSDKWrapper);
			_signature = await _mirageSDKWrapper.Sign(Message);
			Debug.Log($"Signature: {_signature}");
		}
	
		public void CheckSignature()
		{
			var address = _mirageSDKWrapper.CheckSignature(Message, _signature);
			Debug.Log($"Address from signature: {address}");
		}
	}
}
