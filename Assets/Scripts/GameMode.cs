using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    
    [SerializeField] private MeshRenderer m_MeshRenderer;

    [Header("Networked Properties")]
    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public Color PlayerColor { get; set; }
    [Networked] public NetworkString<_32> PlayerName { get; set; }
    #region Fusion Callbacks

    //Initialization Logic (New Start/Awake)
    public override void Spawned()
    {
        if (HasInputAuthority) //client
        {

        }

        if (HasStateAuthority) //server
        {
            PlayerColor = Random.ColorHSV();
        }
    }

    //OnDestroy
    public override void Despawned(NetworkRunner runner, bool hasState)
    {

    }

    //Update
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if(GetInput(out NetworkInputData input))
        {
            this.transform.position += 
                new Vector3(input.InputVector.normalized.x, input.InputVector.normalized.y)
                * Runner.DeltaTime;

            NetworkedPosition = this.transform.position;
        }
    }

    //Happens after FixedUpdateNetwork, for non server objects
    public override void Render()
    {
        this.transform.position = NetworkedPosition;
        if(m_MeshRenderer != null && m_MeshRenderer.material.color != PlayerColor)
        {
            m_MeshRenderer.material.color = PlayerColor;
        }
            
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)] 
    private void RPC_SetPlayerColor(Color color)
    {
        if (HasStateAuthority)
        {
            this.PlayerColor = color;
        }
            
    }private void RPC_SetPlayerName(string color)
    {
        if (HasStateAuthority)
        {
            this.PlayerName = color;
        }

        //this.PlayerName = PlayerName.ToString();
            
    }
    #endregion


    #region UnityCallbacks
    private void Update()
    {
        if (!HasInputAuthority) return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            var randColor = Random.ColorHSV();
            RPC_SetPlayerColor(randColor);
        }
    }
    #endregion
}
