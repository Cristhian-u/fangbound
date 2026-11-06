using UnityEngine;

namespace Fangbound.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerAbilities : MonoBehaviour
    {
        [SerializeField] private bool dashUnlocked;
        [SerializeField] private bool doubleJumpUnlocked;

        public bool DashUnlocked => dashUnlocked;
        public bool DoubleJumpUnlocked => doubleJumpUnlocked;

        public void SetDashUnlocked(bool unlocked)
        {
            dashUnlocked = unlocked;
        }

        public void SetDoubleJumpUnlocked(bool unlocked)
        {
            doubleJumpUnlocked = unlocked;
        }
    }
}