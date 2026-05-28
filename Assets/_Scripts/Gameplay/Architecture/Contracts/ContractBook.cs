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

    public bool CanTick { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public void Enable()
    {
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        gameObject.SetActive(false);

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
        
    }

    public bool IsInteractable(IInteractor interactor = null)
    {
        return gameObject.activeInHierarchy;
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
