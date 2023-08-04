using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace ShaderWork
{
    public class ScanRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] 
        private ScanPass.ScanShaderParams m_ScanShaderParams;
        
        private ScanPass m_ScanPass;
        
        public override void Create()
        {
            if (m_ScanPass != null)
            {
                return;
            }
            
            m_ScanPass = new ScanPass(m_ScanShaderParams)
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_ScanPass);
        }
    }
}