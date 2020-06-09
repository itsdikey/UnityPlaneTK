using PlaneTK.Configuration;
using PlaneTK.StreamManager.DataPacket;
using UnityEngine;

namespace PlaneTK.PlaneControls.ControlHandlers
{
    public interface IControlHandler
    {
        void Init(ConfigModel configModel);
        ControlPacket Handle();
    }
}