using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lofelt.NiceVibrations;

public class HapticsManager : MonoBehaviour
{
    public void PlayHaptic(int index)
    {
        bool hapticsSupported = DeviceCapabilities.isVersionSupported;
        // Ensure haptics are enabled and supported
        if (HapticController.hapticsEnabled && hapticsSupported)
        {
            // Play a built-in short vibration pattern
            
            if(index == 1)
            {
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.Selection);
            }
            else if (index == 2)
            {
                HapticPatterns.PlayPreset(HapticPatterns.PresetType.LightImpact);
            }           
        }
    }
}
