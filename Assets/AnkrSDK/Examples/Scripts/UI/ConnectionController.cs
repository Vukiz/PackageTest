using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

namespace AnkrSDK.UI
{
	public class ConnectionController : MonoBehaviour
	{
		private const string LoginText = "Login";
		private const string ConnectingText = "Connecting...";
		private const string DisconnectedText = "Disconnected";

		[SerializeField] private TMP_Text _connectionText;
		[SerializeField] private Button _loginButton;
		[SerializeField] private GameObject _sceneChooser;
	#if !UNITY_WEBGL || UNITY_EDITOR
		[SerializeField] private ChooseWalletScreen _chooseWalletScreen;
		[SerializeField] private AnkrSDK.WalletConnectSharp.Unity.WalletConnect _walletConnect;
	#endif
	#if !UNITY_ANDROID && !UNITY_IOS || UNITY_EDITOR
		[SerializeField] private AnkrSDK.Utils.UI.QRCodeImage _qrCodeImage;
	#endif

		private void OnEnable()
		{
		#if !UNITY_WEBGL
			_connectionText.text = LoginText;
			_sceneChooser.SetActive(false);
			_loginButton.gameObject.SetActive(true);
			TrySubscribeToWalletEvents().Forget();
		#else
			_loginButton.gameObject.SetActive(false);
		#endif
		}

	#if UNITY_WEBGL && !UNITY_EDITOR
	#elif !UNITY_EDITOR && UNITY_IOS
		private UnityAction GetLoginAction()
		{
			return () => _chooseWalletScreen.Activate(_walletConnect.OpenDeepLink);
		}
	#elif !UNITY_EDITOR && UNITY_ANDROID
		private UnityAction GetLoginAction()
		{
			return _walletConnect.OpenDeepLink;
		}
	#else
		private UnityAction GetLoginAction()
		{
			var connectURL = _walletConnect.ConnectURL;
			if (_qrCodeImage != null)
			{
				return () =>
				{
					_qrCodeImage.UpdateQRCode(connectURL);
					_qrCodeImage.SetImageActive(true);
				};
			}

			return () => Debug.Log($"Trying to open {connectURL}");
		}
	#endif

	#if !UNITY_WEBGL || UNITY_EDITOR
		private void OnDisable()
		{
			UnsubscribeFromTransportEvents();
			_walletConnect.ConnectionStarted -= OnConnectionStarted;
		}

		private async UniTask TrySubscribeToWalletEvents()
		{
			await UniTask.WaitUntil(() => _walletConnect.Session != null);

			_loginButton.onClick.AddListener(GetLoginAction());

			SubscribeOnTransportEvents();
		}

		private void SubscribeOnTransportEvents()
		{
			UpdateSceneState();
			var walletConnectUnitySession = _walletConnect.Session;
			UpdateLoginButtonState(this, walletConnectUnitySession);

			_walletConnect.ConnectedEvent.AddListener(UpdateSceneState);

			walletConnectUnitySession.OnTransportConnect += UpdateLoginButtonState;
			walletConnectUnitySession.OnTransportDisconnect += UpdateLoginButtonState;
			walletConnectUnitySession.OnTransportOpen += UpdateLoginButtonState;
			walletConnectUnitySession.OnSessionDisconnect += OnSessionDisconnect;
		}

		private void OnSessionDisconnect(object sender, EventArgs e)
		{
			UnsubscribeFromTransportEvents();
			UpdateLoginButtonState(this, _walletConnect.Session);
			_walletConnect.ConnectionStarted += OnConnectionStarted;
		}

		private void OnConnectionStarted()
		{
			_walletConnect.ConnectionStarted -= OnConnectionStarted;

			TrySubscribeToWalletEvents().Forget();
		}

		private void UnsubscribeFromTransportEvents()
		{
			_loginButton.onClick.RemoveAllListeners();
			_walletConnect.ConnectedEvent.RemoveListener(UpdateSceneState);

			var walletConnectSession = _walletConnect.Session;
			if (walletConnectSession == null)
			{
				return;
			}

			walletConnectSession.OnTransportConnect -= UpdateLoginButtonState;
			walletConnectSession.OnTransportDisconnect -= UpdateLoginButtonState;
			walletConnectSession.OnTransportOpen -= UpdateLoginButtonState;
			walletConnectSession.OnSessionDisconnect -= OnSessionDisconnect;
		}

		private void UpdateLoginButtonState(object sender, AnkrSDK.WalletConnectSharp.Core.WalletConnectProtocol e)
		{
			UpdateSceneState();
			if (!e.Connected && !e.Connecting && e.Disconnected)
			{
				_connectionText.text = DisconnectedText;
			}
			else
			{
				_connectionText.text = e.TransportConnected ? LoginText : ConnectingText;
			}

			_loginButton.interactable = e.TransportConnected;
		}

		private void UpdateSceneState(AnkrSDK.WalletConnectSharp.Core.Models.WCSessionData _ = null)
		{
			var walletConnectUnitySession = _walletConnect.Session;
			if (walletConnectUnitySession == null)
			{
				return;
			}

			var isConnected = walletConnectUnitySession.Connected;
			_sceneChooser.SetActive(isConnected);
			_chooseWalletScreen.SetActive(!isConnected);
			_loginButton.gameObject.SetActive(!isConnected);
		#if !UNITY_ANDROID && !UNITY_IOS || UNITY_EDITOR
			if (_qrCodeImage != null)
			{
				_qrCodeImage.SetImageActive(false);
			}
		#endif
		}
	#endif
	}
}