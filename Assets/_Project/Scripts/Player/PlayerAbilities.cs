using UnityEngine;

namespace Fangbound.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerAbilities : MonoBehaviour
    {
        [SerializeField] private bool dashUnlocked;
        [SerializeField] private bool doubleJumpUnlocked;
        // Legacy/test scenes retain their existing combat. Narrative rooms opt out at session start.
        [SerializeField] private bool fangOwned = true;

        public bool DashUnlocked => dashUnlocked;
        public bool DoubleJumpUnlocked => doubleJumpUnlocked;
        public bool FangOwned => fangOwned;

        public void SetFangOwned(bool owned) => fangOwned = owned;

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
