using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GanondorfMod.SkillStates.Ganondorf
{
    internal class ThrownSwordContainer : MonoBehaviour
    {
        public Vector3 targetPosition;
        public CharacterBody charBody;
        public float distanceToThrow;
        public Ray aimRay;
        public Vector3 startingPosition;
        public bool isReal;

        public void Awake() 
        {
            if (charBody)
            {
                aimRay = charBody.inputBank.GetAimRay();
            }
        }

        public void Start()
        {
            if (charBody) 
            {
                aimRay = charBody.inputBank.GetAimRay();
            }
        }

        public void Update() 
        {
            if (charBody) 
            {
                aimRay = charBody.inputBank.GetAimRay();

            }
        }

        public void FixedUpdate() 
        {
            if (isReal) 
            {
                
            }
        }

        public void OnDestroy() 
        {
            
        }
    }
}
