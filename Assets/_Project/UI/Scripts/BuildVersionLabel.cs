using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Template.UI
{
    [DisallowMultipleComponent]
    public sealed class BuildVersionLabel : MonoBehaviour
    {
        [SerializeField] TMP_Text label;

        void Reset()
        {
            label = GetComponent<TMP_Text>();
            RefreshLabel();
        }

        void OnEnable() => RefreshLabel();

        void OnValidate() => RefreshLabel();

        void RefreshLabel()
        {
            if (label == null)
                return;

            label.text = $"BUILD {Application.version} - {GetGraphicsApiName(SystemInfo.graphicsDeviceType)}";
        }

        static string GetGraphicsApiName(GraphicsDeviceType graphicsApi) => graphicsApi switch
        {
            GraphicsDeviceType.Direct3D11 => "DX11",
            GraphicsDeviceType.Direct3D12 => "DX12",
            GraphicsDeviceType.Vulkan => "VULKAN",
            GraphicsDeviceType.Metal => "METAL",
            GraphicsDeviceType.OpenGLCore => "OPENGL",
            GraphicsDeviceType.OpenGLES3 => "GLES3",
            GraphicsDeviceType.WebGPU => "WEBGPU",
            _ => graphicsApi.ToString().ToUpperInvariant()
        };
    }
}
