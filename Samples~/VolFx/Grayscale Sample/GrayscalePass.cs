using UnityEngine;

namespace VolFx
{
    [ShaderName("Hidden/VolFx/Grayscale")] // shader name for pass material
    public class GrayscalePass : VolFx.Pass
    {
        // =======================================================================
        public override bool Validate(Material mat)
        {
            // use stack from feature settings
            var settings = Stack.GetComponent<GrayscaleVol>();
            
            // return false if we don't want to execute pass
            if (settings.IsActive() == false)
                return false;
            
            // setup material before drawing
            mat.SetFloat("_Weight", settings.m_Weight.value);
            return true;
        }
    }
}