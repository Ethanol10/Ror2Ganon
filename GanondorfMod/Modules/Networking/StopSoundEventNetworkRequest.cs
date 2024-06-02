using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;


namespace GanondorfMod.Modules.Networking
{
    internal class StopSoundEventNetworkRequest : INetMessage
    {
        //Network these ones.
        uint eventNum;

        //Don't network these.
        GameObject bodyObj;

        public StopSoundEventNetworkRequest()
        {

        }

        public StopSoundEventNetworkRequest(uint eventNum)
        {
            this.eventNum = eventNum;
        }

        public void Deserialize(NetworkReader reader)
        {
            eventNum = reader.ReadUInt32();
        }

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(eventNum);
        }

        public void OnReceived()
        {
            if (AkSoundEngine.IsInitialized()) 
            {
                AkSoundEngine.StopPlayingID(eventNum);
            }            
        }
    }
}
