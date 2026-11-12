using _Scripts.Gameplay.General.Identification;
using _Scripts.Gameplay.General.Morgue;
using UnityEngine;

namespace _Scripts.Gameplay.Characters.Receptionist {

    public class Receptionist : MorgueActor
    {
        [SerializeField]
        private GameObject _receptionChair;

        public override void EnterHouseThroughChute()
        {
        }

        public override void Setup()
        {
            RuntimeID = GetComponent<RuntimeID>();
        }

        public override void Tick()
        {
        }

        public override void ToggleCollision(bool set)
        {
        }

        public override void ToggleProne(bool set)
        {
        }

        public void ToggleHidden(bool hide, bool includeChair)
        {
            gameObject.SetActive(!hide);
            if (includeChair)
            {
                _receptionChair.SetActive(!hide);
            }
        }
    }

}
