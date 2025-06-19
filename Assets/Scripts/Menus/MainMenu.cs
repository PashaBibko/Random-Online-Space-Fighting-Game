using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas and elements")]
    [SerializeField] private Canvas m_Canvas;

    #if UNITY_EDITOR
        [Header("Dev settings")]
        [SerializeField] private OnlineState.TransferProtocol d_Protocol;
    #endif

    public void OnButtonClicked_Solo()
    {
        // Singleplayer uses localhost so logic does not require a non-network solution //
        OnlineState.Init(OnlineState.TransferProtocol.LOCALHOST, isHost: true);
    }

    public void OnButtonClicked_Host()
    {
        // Allows choosing of network protocol in editor else uses relay //
        #if UNITY_EDITOR
            OnlineState.Init(d_Protocol, isHost: true);
        #else
            OnlineState.Init(OnlineState.TransferProtocol.RELAY, isHost: true);
        #endif
    }

    public void OnButtonClicked_Join()
    {
        // Allows choosing of network protocol in editor else uses relay //
        #if UNITY_EDITOR
            OnlineState.Init(d_Protocol, isHost: false);
        #else
            OnlineState.Init(OnlineState.TransferProtocol.RELAY, isHost: false);
        #endif
    }
}
