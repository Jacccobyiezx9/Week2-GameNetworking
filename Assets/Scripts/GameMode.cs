using UnityEngine;
using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    [SerializeField] private MeshRenderer m_MeshRenderer;
    #region Fusion Callbacks
    //Initialization Logic (New Start/Awake)
    public override void Spawned()
    {
        
    }

    //OnDestroy
    public override void Despawned(NetworkRunner runner, bool hasState)
    {

    }

    //Update
    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData input))
        {
            this.transform.position += 
                new Vector3(input.InputVector.normalized.x, input.InputVector.normalized.y)
                * Runner.DeltaTime;
        }
    }

    //Happens FixedUpdateNetwork, for non server objects
    public override void Render()
    {
        
    }
    #endregion

}
