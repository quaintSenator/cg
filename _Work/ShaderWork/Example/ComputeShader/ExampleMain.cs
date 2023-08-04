using UnityEngine;
using UnityEngine.Serialization;

namespace ShaderExample
{
    public class ExampleMain : MonoBehaviour
    {
        private enum DebugType
        {
            ShowThread,
            ShowGroupThread,
            ShowGroup,
            ShowBinarization
        }
        
        [SerializeField]
        private ComputeShader m_ExampleCs;
    
        [SerializeField]
        private Camera m_Camera;

        [SerializeField] 
        private Texture2D m_InputTex;

        [Range(0, 1), SerializeField] 
        private float m_Threshold = 0.28f;

        [SerializeField]
        private DebugType m_DebugType;

        #region BaseConfig

        //Thread
        private const int THREADS_PER_GROUP_X = 32;
        private const int THREADS_PER_GROUP_Y = 24;
    
        private int m_ThreadGroupSizeX;
        private int m_ThreadGroupSizeY;

        //Screen
        private Vector4 m_ScreenResolution;

        private Vector4 m_ThreadConfig;
    
        #endregion
    
        private int m_ExampleKernel;

        private RenderTexture m_ResultRT;

        #region ShaderID
    
        private static readonly int s_ThresholdShaderID = Shader.PropertyToID("_Threshold");
        private static readonly int s_DebugTypeShaderID = Shader.PropertyToID("_DebugType");
        private static readonly int s_InputTexShaderID = Shader.PropertyToID("_InputTex");
        private static readonly int s_ResultRTShaderID = Shader.PropertyToID("_ResultRT");
        private static readonly int s_ScreenResolutionShaderID = Shader.PropertyToID("_ScreenResolution");
        private static readonly int s_ThreadConfigShaderID = Shader.PropertyToID("_ThreadConfig");

        #endregion
    
        private void Start()
        {
            InitConfig();

            InitResources();
        }

        private void InitConfig()
        {
            m_ExampleKernel = m_ExampleCs.FindKernel("CSMain");

            m_ScreenResolution.x = Screen.width;
            m_ScreenResolution.y = Screen.height;
            m_ThreadGroupSizeX = Mathf.CeilToInt(m_ScreenResolution.x / THREADS_PER_GROUP_X);
            m_ThreadGroupSizeY = Mathf.CeilToInt(m_ScreenResolution.y / THREADS_PER_GROUP_Y);

            m_ThreadConfig = new Vector4(m_ThreadGroupSizeX, m_ThreadGroupSizeY, THREADS_PER_GROUP_X, THREADS_PER_GROUP_Y);
        }

        private void InitResources()
        {
            var texDescriptor = new RenderTextureDescriptor((int)m_ScreenResolution.x, (int)m_ScreenResolution.y, RenderTextureFormat.ARGBFloat, 0, 0)
            {
                enableRandomWrite = true
            };
        
            m_ResultRT = new RenderTexture(texDescriptor);
            
            m_ExampleCs.SetTexture(m_ExampleKernel, s_ResultRTShaderID, m_ResultRT);
            m_ExampleCs.SetTexture(m_ExampleKernel, s_InputTexShaderID, m_InputTex);
            m_ExampleCs.SetVector(s_ThreadConfigShaderID, m_ThreadConfig);
            m_ExampleCs.SetVector(s_ScreenResolutionShaderID, m_ScreenResolution);
        }

        private void Update()
        {
            m_ExampleCs.SetInt(s_DebugTypeShaderID, (int)m_DebugType);
            m_ExampleCs.SetFloat(s_ThresholdShaderID, m_Threshold);
            m_ExampleCs.Dispatch(m_ExampleKernel, m_ThreadGroupSizeX, m_ThreadGroupSizeY, 1);
        }
    
        private void OnGUI()
        {
            if (m_ResultRT != null)
            {
                GUI.DrawTexture(new Rect(0, 0, m_ScreenResolution.x, m_ScreenResolution.y), m_ResultRT);
            }
        }
    }
}
