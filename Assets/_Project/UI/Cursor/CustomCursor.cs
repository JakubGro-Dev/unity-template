using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Template.UI
{
    [DisallowMultipleComponent]
    public sealed class CustomCursor : MonoBehaviour
    {
        [SerializeField] RectTransform cursor;

        bool previousCursorVisibility;
        bool previousOverlayRendering;
        bool ownsCursor;

        void OnEnable()
        {
            if (cursor == null)
                return;

            previousCursorVisibility = Cursor.visible;
            previousOverlayRendering = SupportedRenderingFeatures.active.rendersUIOverlay;
            ownsCursor = true;
            RenderPipelineManager.beginContextRendering += RenderOverlayUIAfterPipeline;
            UpdateCursor();
        }

        void OnDisable()
        {
            if (!ownsCursor)
                return;

            RenderPipelineManager.beginContextRendering -= RenderOverlayUIAfterPipeline;
            SupportedRenderingFeatures.active.rendersUIOverlay = previousOverlayRendering;
            Cursor.visible = previousCursorVisibility;
            ownsCursor = false;
        }

        static void RenderOverlayUIAfterPipeline(ScriptableRenderContext context, List<Camera> cameras)
        {
            // As in FalseParadise, inversion must blend over URP's completed output.
            SupportedRenderingFeatures.active.rendersUIOverlay = false;
        }

        void LateUpdate() => UpdateCursor();

        void OnApplicationFocus(bool hasFocus)
        {
            if (ownsCursor)
                UpdateCursor();
        }

        void UpdateCursor()
        {
            if (!ownsCursor || cursor == null)
                return;

            Mouse mouse = Mouse.current;
            bool show = Application.isFocused && mouse != null && Cursor.lockState == CursorLockMode.None;
            Vector2 position = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
            show &= position.x >= 0 && position.y >= 0 && position.x < Screen.width && position.y < Screen.height;
            cursor.gameObject.SetActive(show);
            Cursor.visible = show ? false : previousCursorVisibility;

            if (show)
                cursor.position = position;
        }
    }
}
