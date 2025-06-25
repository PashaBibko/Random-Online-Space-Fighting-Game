using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas and elements")]
    [SerializeField] Text m_PlayercountText;
    [SerializeField] Text m_JoincodeText;
    [SerializeField] InputField m_JoincodeInput;
    [SerializeField] Button m_StartButton;

    #if UNITY_EDITOR
        [Header("Dev settings")]
        [SerializeField] OnlineState.TransferProtocol d_Protocol;
    #endif

    // Trackers of the state of the menu //

    private bool mConnectedToServer = false;

    // ===== General functions ===== //

    private void Start()
    {
        // Disabled by default //
        m_StartButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Does not attempt to message server if not connected //
        if (mConnectedToServer == false) { return; }

        // Updates the player count text //
        m_PlayercountText.text = "Playercount: " + ServerController.Playercount();
        m_JoincodeText.text = "Code: " + OnlineState.Joincode();
    }

    private PlayerGlobalState.PlayerClass Int32ToPlayerClass(Int32 value)
    {
        switch (value)
        {
            case 0:
                return PlayerGlobalState.PlayerClass.GUNNER;

            case 1:
                return PlayerGlobalState.PlayerClass.SCOUT;

            case 3:
                return PlayerGlobalState.PlayerClass.ENGIE;

            default:
                Debug.LogError("Something has gone horribly wrong");
                return PlayerGlobalState.PlayerClass.GUNNER;
        }
    }

    // ===== Button click events ===== //

    public void OnButtonClicked_Solo()
    {
        // Singleplayer uses localhost so logic does not require a non-network solution //
        OnlineState.Init(OnlineState.TransferProtocol.LOCALHOST, isHost: true);

        // Simulates the start button being clicked //
        OnButtonClicked_Start();
    }

    public void OnButtonClicked_Host()
    {
        // Allows choosing of network protocol in editor else uses relay //
        #if UNITY_EDITOR
            OnlineState.Init(d_Protocol, isHost: true);
        #else
            OnlineState.Init(OnlineState.TransferProtocol.RELAY, isHost: true);
        #endif

        // The user is now connected to a server //
        mConnectedToServer = true;
        m_StartButton.gameObject.SetActive(true);
    }

    public void OnButtonClicked_Join()
    {
        // Allows choosing of network protocol in editor else uses relay //
        #if UNITY_EDITOR
            OnlineState.Init(d_Protocol, isHost: false, joincode: m_JoincodeInput.text);
        #else
            OnlineState.Init(OnlineState.TransferProtocol.RELAY, isHost: false, joincode: m_JoincodeInput.text);
        #endif

        // The user is now connected to a server //
        mConnectedToServer = true;
        m_StartButton.gameObject.SetActive(true);
    }

    public void OnButtonClicked_Start()
    {
        // Loads the game scene //
         //TODO: Set up a server RPC so anyone can click the button //
        SceneController.Load("TestScene");
    }

    // ===== Dropdown events ===== //
    public void OnDropdownItemChange_PrimaryClass(Int32 value)
        => PlayerGlobalState.PrimaryClass = Int32ToPlayerClass(value);

    public void OnDropdownItemChange_SecondaryClass(Int32 value)
        => PlayerGlobalState.SecondaryClass = Int32ToPlayerClass(value);
}
