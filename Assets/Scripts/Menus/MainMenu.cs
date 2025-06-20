using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas and elements")]
    [SerializeField] private Canvas m_Canvas;
    [SerializeField] private Text m_PlayercountText;
    [SerializeField] private Text m_JoincodeText;
    [SerializeField] private InputField m_JoincodeInput;
    [SerializeField] private Button m_StartButton;

    #if UNITY_EDITOR
        [Header("Dev settings")]
        [SerializeField] private OnlineState.TransferProtocol d_Protocol;
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
        SceneController.Load("TestScene");
    }
}
