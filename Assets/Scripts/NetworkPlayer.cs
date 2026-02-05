using UnityEngine;
using Fusion;
using TMPro;

public class NetworkPlayer : NetworkBehaviour
{

    [SerializeField] private SkinnedMeshRenderer m_MeshRenderer;
    [SerializeField] private Animator anim;
    [SerializeField] public TextMeshProUGUI nameText;
    [SerializeField] private float moveSpeed = 5f;

    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Jump = Animator.StringToHash("Jump");

    [Header("Networked Properties")]
    [Networked] public Vector3 NetworkedPosition { get; set; }
    [Networked] public Color PlayerColor { get; set; }
    [Networked] public NetworkString<_32> PlayerName { get; set; }
    [Networked] public int PlayerTeam { get; set; }

    [Networked] public NetworkAnimatorData PlayerAnimatorData { get; set; }

    private Vector3 lastKnownPosition;
    [SerializeField] private float lerpSpeed = 3f;
    #region Fusion Callbacks

    //Initialization Logic (New Start/Awake)
    public override void Spawned()
    {

        if (HasInputAuthority) //client
        {
            Transform cameraSpot = transform.Find("CameraSpot");
            if (cameraSpot != null)
            {
                Camera.main.transform.SetParent(cameraSpot);
                Camera.main.transform.localPosition = Vector3.zero;
                Camera.main.transform.localRotation = Quaternion.identity;
            }

            var manager = NetworkSessionManager.Instance;
            if (manager != null)
            {
                RPC_SetPlayerName(manager.playerName);
                RPC_SetPlayerColor(manager.playerColor);
                RPC_SetTeam(manager.playerTeam);
            }
        }

        if (HasStateAuthority) //server
        {
            NetworkedPosition = new Vector3(0, 0, 0);
            transform.position = NetworkedPosition;

            PlayerAnimatorData = new NetworkAnimatorData()
            {
                Speed = 0,
                Jump = false
            };

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

        if (GetInput(out NetworkInputData input))
        {
            if (input.InputVector != Vector3.zero)
            {
                transform.position += input.InputVector * moveSpeed * Runner.DeltaTime;

            }

            if(input.jumpInput)
                anim.SetTrigger(Jump.ToString());

            anim.SetFloat(Speed, input.sprintInput ? 1f : 0);

            NetworkedPosition = transform.position;
            PlayerAnimatorData = new NetworkAnimatorData()
            {
                Speed = input.sprintInput ? 1f : 0,
                Jump = input.jumpInput
            };
    }
    }


    //Happens after FixedUpdateNetwork, for non server objects
    public override void Render()
    {
        
        if (m_MeshRenderer != null && m_MeshRenderer.material.color != PlayerColor)
        {
            m_MeshRenderer.material.color = PlayerColor;
        }

        if (nameText != null)
        {
            nameText.text = PlayerName.ToString();
            nameText.transform.rotation = Quaternion.LookRotation(nameText.transform.position - Camera.main.transform.position);
        }

        anim.SetFloat(Speed, PlayerAnimatorData.Speed);

    }
    private void LateUpdate()
    {
        this.transform.position = Vector3.Lerp(lastKnownPosition, NetworkedPosition, Runner.DeltaTime * lerpSpeed);
        lastKnownPosition = NetworkedPosition;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerColor(Color color)
    {
        if (HasStateAuthority)
        {
            this.PlayerColor = color;
        }

    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string name)
    {
        if (HasStateAuthority)
        {
            this.PlayerName = name;
        }

    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetTeam(int team)
    {
        this.PlayerTeam = team;

        Vector3 spawnPos = Vector3.zero;
        if (team == 1 && NetworkSessionManager.Instance.team1Spawns.Length > 0)
        {
            spawnPos = NetworkSessionManager.Instance.team1Spawns[Random.Range(0, NetworkSessionManager.Instance.team1Spawns.Length)].position;
        }
        else if (team == 2 && NetworkSessionManager.Instance.team2Spawns.Length > 0)
        {
            spawnPos = NetworkSessionManager.Instance.team2Spawns[Random.Range(0, NetworkSessionManager.Instance.team2Spawns.Length)].position;
        }

        transform.position = spawnPos;
        NetworkedPosition = spawnPos;
    }
    #endregion


    #region UnityCallbacks
    //private void Update()
    //{
    //    if (!HasInputAuthority) return;
    //    if (Input.GetKeyDown(KeyCode.Q))
    //    {
    //        var randColor = Random.ColorHSV();
    //        RPC_SetPlayerColor(randColor);
    //    }
    //}
    #endregion
}
