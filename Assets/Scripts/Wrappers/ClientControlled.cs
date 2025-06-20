using Unity.Netcode;

// Wrapper around Network Behaviour to stop silly mistakes //
public class ClientControlled : NetworkBehaviour
{
    // Only calls the On version of the function if they are owned by the client //

    protected void Start()
    {
        if (IsOwner) { OnStart(); }
    }

    protected void Update()
    {
        if (IsOwner) { OnUpdate(); }
    }

    protected void FixedUpdate()
    {
        if (IsOwner) { OnFixedUpdate(); }
    }

    protected void LateUpdate()
    {
        if (IsOwner) { OnLateUpdate(); }
    }

    // Virtual functions to allow overiding //

    public virtual void OnStart() { }
    public virtual void OnUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnLateUpdate() { }
}
