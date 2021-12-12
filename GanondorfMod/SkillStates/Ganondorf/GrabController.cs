using RoR2;
using UnityEngine;

namespace GanondorfMod.SkillStates
{
    public class GrabController : MonoBehaviour
    {
        public Transform pivotTransform;

        private CharacterBody body;
        private CharacterMotor motor;
        private CharacterDirection direction;
        private ModelLocator modelLocator;
        private Transform modelTransform;
        private Quaternion originalRotation;
        private Collider collider;
        private SphereCollider sphCollider;
        private CapsuleCollider capCollider;

        private void Awake()
        {
            this.body = this.GetComponent<CharacterBody>();
            this.motor = this.GetComponent<CharacterMotor>();
            this.direction = this.GetComponent<CharacterDirection>();
            this.modelLocator = this.GetComponent<ModelLocator>();
            this.collider = this.gameObject.GetComponent<Collider>();
            this.sphCollider = this.gameObject.GetComponent<SphereCollider>();
            this.capCollider = this.gameObject.GetComponent<CapsuleCollider>();

            if (this.collider) {
                collider.enabled = false;
            }
            if (this.sphCollider) {
                sphCollider.enabled = false;
            }
            if (this.capCollider) {
                capCollider.enabled = false;
            }
            
            
            if (this.direction) this.direction.enabled = false;

            if (this.modelLocator)
            {
                if (this.modelLocator.modelTransform)
                {
                    this.modelTransform = modelLocator.modelTransform;
                    this.originalRotation = this.modelTransform.rotation;

                    if (this.modelLocator.gameObject.name == "GreaterWispBody(Clone)") {
                        this.modelLocator.dontDetatchFromParent = true;
                        this.modelLocator.dontReleaseModelOnDeath = true;
                    }

                    this.modelLocator.enabled = false;
                }
            }
        }

        private void FixedUpdate()
        {
            if (this.motor)
            {
                this.motor.disableAirControlUntilCollision = true;
                this.motor.velocity = Vector3.zero;
                this.motor.rootMotion = Vector3.zero;

                this.motor.Motor.SetPosition(this.pivotTransform.position, true);
            }

            if (this.pivotTransform)
            {
                this.transform.position = this.pivotTransform.position;
            }
            else
            {
                this.Release();
            }

            if (this.modelTransform)
            {
                this.modelTransform.position = this.pivotTransform.position;
                this.modelTransform.rotation = this.pivotTransform.rotation;
            }
        }

        public void Release()
        {
            if (this.modelLocator) this.modelLocator.enabled = true;
            if (this.modelTransform) this.modelTransform.rotation = this.originalRotation;
            if (this.direction) this.direction.enabled = true;
            if (this.collider) this.collider.enabled = true;
            if (this.sphCollider) this.sphCollider.enabled = true;
            if (this.capCollider) this.capCollider.enabled = true;
            Destroy(this);
        }
    }
}