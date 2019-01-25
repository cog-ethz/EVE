 
Shader "FX/Diamond"
{
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Shininess ("Shininess", Range (0, 10)) = 1
        _ReflectTex ("Reflection Texture", Cube) = "dummy.jpg"
        _RefractTex ("Refraction Texture", Cube) = "dummy.jpg"
    }  
    SubShader {
        Tags {
            "RenderType" = "Transparent"
             "Queue" = "Transparent"
        }
        // First pass - here we render the backfaces of the diamonds. Since those diamonds are more-or-less
        // convex objects, this is effectively rendering the inside of them
        Cull Front
        ZWrite Off
        Blend SrcAlpha Zero
        CGPROGRAM
            #pragma surface surf BlinnPhong
 
            samplerCUBE _ReflectTex;
            samplerCUBE _RefractTex;
         
            fixed4 _Color;
         
            struct Input {
                float3 worldRefl;
            };
         
            void surf (Input IN, inout SurfaceOutput o) {
                o.Albedo = texCUBE(_RefractTex, IN.worldRefl) * _Color;
                o.Emission = texCUBE(_ReflectTex, IN.worldRefl);
            }
        ENDCG
 
        // Second pass - here we render the front faces of the diamonds.
        Cull Back
        Blend One One
        ZWrite On
 
        CGPROGRAM
            #pragma surface surf BlinnPhong
 
            samplerCUBE _ReflectTex;
         
            fixed4 _Color;
            half _Shininess;
         
            struct Input {
                float3 worldRefl;
            };
            samplerCUBE _RefractTex;
         
            void surf (Input IN, inout SurfaceOutput o) {
                o.Albedo = texCUBE(_RefractTex, IN.worldRefl) * _Color;
                o.Emission = texCUBE(_ReflectTex, IN.worldRefl) * _Shininess;
            }
 
        ENDCG
        

    }
 
    Fallback "VertexLit"
}