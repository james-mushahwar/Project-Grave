using _Scripts.Gameplay.Architecture.Contracts;
using _Scripts.Gameplay.Architecture.Managers;
using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.General.Morgue;
using _Scripts.Gameplay.Player.Controller;
using _Scripts.Org;
using System.Collections.Generic;
using UnityEngine;

public class ContractBook : MorgueActor, IInteractable
{
    [Header("Setup")]
    [SerializeField] private MeshRenderer targetRenderer;

    [SerializeField]
    private List<ContractActor> _contractPages;

    public List<ContractActor> ContractPages
    {
        get { return _contractPages; }
    }

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
