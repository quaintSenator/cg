using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ShaderWork
{
    //所有需要传到CS的参数自行拓展
    public class ScanPass : ScriptableRenderPass
    {
        [Serializable]
        public class ScanShaderParams
        {
            public ComputeShader ScanCs;
            public float lineDistance;
            public float lineWidth;
        }

        private readonly ScanShaderParams m_ScanParams;

        private readonly ComputeShader m_ScanCs;
        private readonly int m_ScanKernel;
        
        //Thread
        private const int THREADS_PER_GROUP_X = 32;
        private const int THREADS_PER_GROUP_Y = 24;
    
        private readonly int m_ThreadGroupSizeX;
        private readonly int m_ThreadGroupSizeY;

        //Screen
        private readonly Vector4 m_ScreenResolution;
        
        private static readonly int s_ScanConfigShaderID = Shader.PropertyToID("_ScanConfig");
        private static readonly int s_ResultRTShaderID = Shader.PropertyToID("_ResultRT");
        private static readonly int s_CameraColorShaderID = Shader.PropertyToID("_CameraColorRT");
        private static readonly int s_ScreenResolutionShaderID = Shader.PropertyToID("_ScreenResolution");
        private static readonly int s_PlayerPosShaderID = Shader.PropertyToID("_PlayerPos");

        private RenderTargetHandle m_TempRT;

        private readonly Transform m_TargetTransform;
        
        public ScanPass(ScanShaderParams scanParams)
        {
            m_ScanParams = scanParams;
            m_ScanCs = scanParams.ScanCs;
            m_ScanKernel = m_ScanCs.FindKernel("ScanKernel");

            //需要自己去拓展 如何拿到这个角色的坐标
            m_TargetTransform = GameObject.FindWithTag("Player").transform;
            
            m_ScreenResolution.x = Screen.width;
            m_ScreenResolution.y = Screen.height;
            m_ThreadGroupSizeX = Mathf.CeilToInt(m_ScreenResolution.x / THREADS_PER_GROUP_X);
            m_ThreadGroupSizeY = Mathf.CeilToInt(m_ScreenResolution.y / THREADS_PER_GROUP_Y);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            if (!camera.name.Contains("Main"))
            {
                return;
            }

            var cmd = CommandBufferPool.Get("ScanPass");
            var cameraColor = renderingData.cameraData.renderer.cameraColorTarget;
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.enableRandomWrite = true;
            cmd.GetTemporaryRT(m_TempRT.id, desc);
            
            cmd.SetComputeVectorParam(m_ScanCs, s_ScreenResolutionShaderID, m_ScreenResolution);
            cmd.SetComputeVectorParam(m_ScanCs, s_PlayerPosShaderID, m_TargetTransform.position);
            cmd.SetComputeVectorParam(m_ScanCs, s_ScanConfigShaderID, new Vector4(m_ScanParams.lineDistance, m_ScanParams.lineWidth, 0, 0));
            cmd.SetComputeTextureParam(m_ScanCs, m_ScanKernel, s_ResultRTShaderID, m_TempRT.Identifier());
            cmd.SetComputeTextureParam(m_ScanCs, m_ScanKernel, s_CameraColorShaderID, cameraColor);
            cmd.DispatchCompute(m_ScanCs, m_ScanKernel, m_ThreadGroupSizeX, m_ThreadGroupSizeY, 1);
            
            cmd.Blit(m_TempRT.Identifier(), cameraColor);
            cmd.ReleaseTemporaryRT(m_TempRT.id);
                
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}