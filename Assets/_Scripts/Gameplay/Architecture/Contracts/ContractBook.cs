using _Scripts.Gameplay.Architecture.Contracts;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.General.Morgue;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using UnityEngine;

public class ContractBook : MorgueActor, IInteractable
{
    [Header("Setup")]
    [SerializeField] private MeshRenderer targetRenderer;

    [Header("Material Element Indices")]
    [SerializeField] private int firstMaterialIndex = 0;
    [SerializeField] private int secondMaterialIndex = 1;

    [SerializeField]
    private ContractActor _contractPageL;
    [SerializeField]
    private ContractActor _contractPageR;

    [SerializeField]
    private FVirtualCamera _contractCamera;

    // Cache the shader property ID for performance
    private static readonly int BaseMapPropertyId = Shader.PropertyToID("_BaseMap");

    public bool CanTick { get; set; }

    public void Enable()
    {
        CanTick = true;
    }

    public void Disable()
    {
        CanTick = false;
    }

    public override void EnterHouseThroughChute()
    {
    }

    public override void ToggleProne(bool set)
    {
    }

    public override void ToggleCollision(bool set)
    {
    }

    public override void Setup()
    {
        RuntimeID = GetComponent<RuntimeID>();
        CameraManager.Instance.AssignVirtualCameraType(RuntimeID, EVirtualCameraType.Contract_Book_Overview, _contractCamera.VirtualCamera);
    }

    public override void Tick()
    {
        Debug.Log("Ticking book");
        if (CanTick)
        {
            bool show = ContractsManager.Instance.PlayerChosenContract == null;
            
            gameObject.SetActive(show);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public bool IsInteractable(IInteractor interactor = null)
    {
        return gameObject.activeInHierarchy && ContractsManager.Instance.SelectableContractsCount > 0;
    }

    public bool OnInteract(IInteractor interactor = null)
    {
        PlayerController pc = interactor as PlayerController;
        if (pc != null)
        {
            pc.RequestPlayerControllerState(EPlayerControllerState.Contracts);
            return true;
        }

        return false;
    }
}
