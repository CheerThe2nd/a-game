using Godot;
using System;

public partial class WateringCan : Node
{
    private float _waterAmmount = 10.0f;
    private float _flowPerSec = 0.3f;
    private ulong _lastPourTs = 0;

    [Export]
    private CpuParticles3D _waterParticle;

    public void setPouring(bool startPour)
    {
        if (_waterParticle != null)
        {
            _waterParticle.Emitting = startPour;        
            if (startPour)
            {
                _lastPourTs = Time.GetTicksMsec();
            }
            else
            {
                ulong sinceLastMs = Time.GetTicksMsec() - _lastPourTs;
                float lostWater = (float)sinceLastMs * _flowPerSec;
                _waterAmmount -= lostWater;
            }
        }
    }


}
