using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

//-----------------------------------------------------------------------
// Author:  Colby-O
// File:    PSXEffectPass.cs
//-----------------------------------------------------------------------
namespace ColbyO.VNTG.PSX
{
    public class PSXSetGlobalParamters : ScriptableRenderPass
    {
        private static readonly int AmbientColorID = Shader.PropertyToID("_VNTGAmbientColor");

        private const string _passName = "PSXSetGlobalParamters";

        private Color _ambientColor;

        public PSXSetGlobalParamters()
        {
        }

        public void Setup(Color color)
        {
            _ambientColor = color;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            Shader.SetGlobalColor(AmbientColorID, _ambientColor);
        }

        private class PassData
        {
            public TextureHandle src;
            public Material material;
            public PSXEffectSettings settings;
        }
    }
}