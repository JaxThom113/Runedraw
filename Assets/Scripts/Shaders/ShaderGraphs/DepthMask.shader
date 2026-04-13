Shader "VFX/TrueDepthMask"
{
    SubShader
    {
        // Render right before Opaque geometry (2000)
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        
        // The Magic Command: Do not write ANY color to the screen.
        ColorMask 0 
        
        // Force the Depth Buffer to write
        ZWrite On

        Pass
        {
            // Empty pass - we don't need to calculate lighting or textures 
            // because we are drawing invisible pixels!
        }
    }
}