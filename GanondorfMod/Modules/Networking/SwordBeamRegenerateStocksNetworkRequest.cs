using R2API.Networking.Interfaces;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GanondorfMod.Modules.Networking
{
    internal class SwordBeamRegenerateStocksNetworkRequest : INetMessage
    {
        public NetworkInstanceId netID;

        public SwordBeamRegenerateStocksNetworkRequest()
        {

        }

        public SwordBeamRegenerateStocksNetworkRequest(NetworkInstanceId netID)
        {
            this.netID = netID;
        }

        public void Deserialize(NetworkReader reader)
        {
            reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            GameObject masterobject = Util.FindNetworkObject(netID);
            CharacterMaster charMaster = masterobject.GetComponent<CharacterMaster>();
            CharacterBody charBody = charMaster.GetBody();
            GameObject bodyObj = charBody.gameObject;

            if (bodyObj) 
            {
                if (charBody) 
                {
                    if (charBody.hasEffectiveAuthority) 
                    {
                        TriforceBuffComponent triforceBuffComponent = bodyObj.GetComponent<TriforceBuffComponent>();
                        if (triforceBuffComponent) 
                        {
                            triforceBuffComponent.AddToBuffCount(1);
                        }
                    }                
                }
            }
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(netID);
        }
    }
}
