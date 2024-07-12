using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;
using UnityEditor;

namespace Visualization.Internal
{

    internal class VisualizationRenderFeature : ScriptableRendererFeature
    {
        private static Queue<Mesh> meshesToDraw = new Queue<Mesh>();
        private static Queue<Material> meshMaterials = new Queue<Material>();
        private static Queue<MaterialPropertyBlock> meshMaterialProperties = new Queue<MaterialPropertyBlock>();
        private static Queue<Matrix4x4> meshTransforms = new Queue<Matrix4x4>();
        private static Pool<Mesh> meshPool = new();
        private static Pool<MaterialPropertyBlock> propPool = new();

        public static MaterialPropertyBlock GetNewMaterialProperties()
        {
            var block = propPool.GetItem();
            return block;
        }

        public static Mesh GetMesh()
        {
            var mesh = meshPool.GetItem();
            return mesh;
        }

        public static void DrawMesh(Mesh mesh, Matrix4x4 matrix, Material material, MaterialPropertyBlock properties)
        {
            meshesToDraw.Enqueue(mesh);
            meshTransforms.Enqueue(matrix);
            meshMaterials.Enqueue(material);
            meshMaterialProperties.Enqueue(properties);
        }

        DrawVisPass drawObjectsPass;
    
        public override void Create()
        {
            DrawMaterials.Init();
            drawObjectsPass = new DrawVisPass();
            drawObjectsPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }
    
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(drawObjectsPass);
        }

        class DrawVisPass : ScriptableRenderPass
        {

            public DrawVisPass()
            {

            }
        
            private class PassData
            {
            }
    
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
            {
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Visualization Draw", out var passData))
                {
                    // Get the data needed to create the list of objects to draw
                    UniversalRenderingData renderingData = frameContext.Get<UniversalRenderingData>();
                    UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();
                    UniversalLightData lightData = frameContext.Get<UniversalLightData>();
                    SortingCriteria sortFlags = cameraData.defaultOpaqueSortFlags;
                    RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
                    FilteringSettings filterSettings = new FilteringSettings(renderQueueRange, ~0);

                    // Redraw only objects that have their LightMode tag set to UniversalForward 
                    ShaderTagId shadersToOverride = new ShaderTagId("UniversalForward");

                    // Create drawing settings
                    DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(shadersToOverride, renderingData, cameraData, lightData, sortFlags);

                    // Add the override material to the drawing settings
                    // drawSettings.overrideMaterial = useMaterial;

                    // Create the list of objects to draw
                    var rendererListParameters = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);

                    // Convert the list to a list handle that the render graph system can use
                    // passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParameters);
                    // passData.useMaterial = useMaterial;
                    
                    // Set the render target as the color and depth textures of the active camera texture
                    UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
                    // builder.UseRendererList(passData.rendererListHandle);
                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
                }
                
            }

            static void ExecutePass(PassData data, RasterGraphContext context)
            {
                try
                {
                    while (meshesToDraw.Count > 0)
                    {
                        var mesh = meshesToDraw.Dequeue();
                        var matrix = meshTransforms.Dequeue();
                        var material = meshMaterials.Dequeue();
                        var properties = meshMaterialProperties.Dequeue();
                        context.cmd.DrawMesh(mesh, matrix, material, 0, 0, properties);
                    }

                    meshPool.FinishedUsingAllitems();
                    propPool.FinishedUsingAllitems();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

        }
    
    }

}