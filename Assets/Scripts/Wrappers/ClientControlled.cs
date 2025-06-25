using Unity.Netcode;

// Wrapper around Network Behaviour to stop silly mistakes //
public class ClientControlled : NetworkBehaviour
{
    // Seperates the call depending on if the client owns the object or not //

    protected void Start()
    {
        if (IsOwner) { OnStart(); }
        else { OnForeignStart(); }
    }

    protected void Update()
    {
        if (IsOwner) { OnUpdate(); }
        else { OnForiegnUpdate(); }
    }

    protected void FixedUpdate()
    {
        if (IsOwner) { OnFixedUpdate(); }
        else { OnForiegnFixedUpdate(); }
    }

    protected void LateUpdate()
    {
        if (IsOwner) { OnLateUpdate(); }
        else { OnForeignLateUpdate(); }
    }

    // Virtual functions to allow overiding //

    public virtual void OnStart() { }
    public virtual void OnForeignStart() { }
    public virtual void OnUpdate() { }
    public virtual void OnForiegnUpdate() { }
    public virtual void OnFixedUpdate() { }
    public virtual void OnForiegnFixedUpdate() { }
    public virtual void OnLateUpdate() { }
    public virtual void OnForeignLateUpdate() { }
}
