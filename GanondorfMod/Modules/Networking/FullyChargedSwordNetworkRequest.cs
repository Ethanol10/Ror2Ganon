using GanondorfMod.Modules.Survivors;
using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GanondorfMod.Modules.Networking
{
    internal class FullyChargedSwordNetworkRequest : INetMessage
    {
        NetworkInstanceId netID;
        bool incomingChargedVal;

        public FullyChargedSwordNetworkRequest() 
        {
        
        }

        public FullyChargedSwordNetworkRequest(NetworkInstanceId netID, bool incomingChargedVal) 
        {
            this.netID = netID;
            this.incomingChargedVal = incomingChargedVal;
        }

        public void Deserialize(NetworkReader reader)
        {
            netID = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            GameObject masterobject = Util.FindNetworkObject(netID);
            CharacterMaster charMaster = masterobject.GetComponent<CharacterMaster>();
            CharacterBody charBody = charMaster.GetBody();
            GameObject bodyObj = charBody.gameObject;

            bodyObj.GetComponent<GanondorfController>().swordFullyCharged = incomingChargedVal;
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(netID);
            writer.Write(incomingChargedVal);
        }
    }
}
