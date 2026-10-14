using UnityEngine;
using RoR2;
using RoR2.SurvivorMannequins;

namespace GanondorfMod.Modules.Survivors
{
    internal class GanondorfDisplaySwordController : MonoBehaviour
    {
        public Transform bustLoc;
        public Transform handLoc;
        public Transform meshLoc;
        public Transform targetTrans;
        public Animator anim;

        private bool isInHand;
        private bool startTimer;
        private float stopwatch;
        private const float grabbedinterval = 0.4f;

        private SurvivorMannequinSlotController mannequinSlot;
        private NetworkUser loadoutOwner;
        private bool loadoutDirty;
        private bool hasSwordState;

        public void Awake()
        {
            anim = GetComponent<Animator>();
        }

        public void OnEnable()
        {
            NetworkUser.onLoadoutChangedGlobal += OnLoadoutChanged;
            loadoutDirty = true;
        }

        public void OnDisable()
        {
            NetworkUser.onLoadoutChangedGlobal -= OnLoadoutChanged;
        }

        public void OnTransformParentChanged()
        {
            // Lobby ordering can move this display to another mannequin slot.
            mannequinSlot = null;
            loadoutDirty = true;
        }

        public void Start()
        {
            RefreshLoadout();
        }

        public void Update()
        {
            RefreshLoadout();

            if (startTimer)
            {
                stopwatch += Time.deltaTime;
                if (stopwatch >= grabbedinterval)
                {
                    SetTransformTarget();
                    stopwatch = 0f;
                    startTimer = false;
                }
            }

            if (meshLoc && targetTrans)
            {
                meshLoc.rotation = targetTrans.rotation;
                meshLoc.position = targetTrans.position;
            }

            if (!anim) 
            {
                anim = GetComponent<Animator>();
            }
        }

        public void SetTransformTarget()
        {
            if (isInHand)
            {
                targetTrans = handLoc;
            }
            else 
            {
                targetTrans = bustLoc;
            }
        }

        private void OnLoadoutChanged(NetworkUser changedUser)
        {
            if (mannequinSlot && changedUser == mannequinSlot.networkUser)
            {
                loadoutDirty = true;
            }
        }

        private void RefreshLoadout()
        {
            if (!mannequinSlot)
            {
                mannequinSlot = GetComponentInParent<SurvivorMannequinSlotController>();
            }

            // Each lobby display is parented to its player's mannequin slot. Slots can be reassigned without changing the selected survivor.
            NetworkUser owner = mannequinSlot ? mannequinSlot.networkUser : null;
            if (!loadoutDirty && owner == loadoutOwner) return;

            loadoutOwner = owner;
            bool swordEquipped = false;
            if (owner)
            {
                Loadout loadout = Loadout.RequestInstance();
                try
                {
                    owner.networkLoadout.CopyLoadout(loadout);
                    BodyIndex bodyIndex = BodyCatalog.FindBodyIndex("GanondorfBody");
                    if (bodyIndex != BodyIndex.None)
                    {
                        // Evaluate one player's complete selection, rather than remembering global skill reads from different players.
                        Loadout.BodyLoadoutManager skills = loadout.bodyLoadoutManager;
                        swordEquipped = skills.GetSkillVariant(bodyIndex, 0) == 1 || skills.GetSkillVariant(bodyIndex, 1) == 3 || skills.GetSkillVariant(bodyIndex, 2) == 3 || skills.GetSkillVariant(bodyIndex, 3) == 2;
                    }
                }
                finally
                {
                    Loadout.ReturnInstance(loadout);
                }
            }

            SetSwordEquipped(swordEquipped);
            loadoutDirty = false;
        }

        private void SetSwordEquipped(bool swordEquipped)
        {
            if (hasSwordState && isInHand == swordEquipped) return;

            hasSwordState = true;
            isInHand = swordEquipped;
            if (anim) anim.SetBool("SwordEquipped", swordEquipped);
            stopwatch = 0f;
            startTimer = true;
        }
    }
}
