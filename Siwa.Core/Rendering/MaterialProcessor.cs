using System.Numerics;
using System.Runtime.CompilerServices;
using Arch.Core;
using Silk.NET.OpenGL;
using Siwa.Core.Assets;
using Siwa.Core.Assets.Kinds;
using Siwa.Core.Components;
using Texture = Siwa.Core.Components.Texture;

namespace Siwa.Core.Rendering;

public class MaterialProcessor(RenderPipeline pipeline) : IRendererExtension
{
    private readonly QueryDescription _query = new QueryDescription().WithAll<Renderable, Transform>();
    private readonly AssetPool<UnlitMaterial> _unlitMaterials = AssetPool<UnlitMaterial>.Registry;
    private readonly AssetPool<LitMaterial> _litMaterials = AssetPool<LitMaterial>.Registry;
    private readonly AssetPool<Components.Shader> _shaders = AssetPool<Components.Shader>.Registry;
    private readonly AssetPool<Texture> _textures = AssetPool<Texture>.Registry;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UnlitMaterial(Mesh mesh, Transform transform)
    {
        ref var mat = ref _unlitMaterials.Get(mesh.Material.Handle.ToHandle<UnlitMaterial>());
        ref var shader = ref _shaders.Get(mat.Shader);
        var color = mat.Color;
        var shaderHandle = shader.Handle;
        var translationMatrix = Matrix4x4.CreateScale(transform.Scale) * 
                                Matrix4x4.CreateFromQuaternion(transform.Rotation) * 
                                Matrix4x4.CreateTranslation(transform.Position);
        pipeline.Submit(new RenderCommand
        {
            ShaderHandle = shaderHandle,
            VaoHandle = mesh.Vao.Handle,
            IndexCount = mesh.IndicesCount,
            WorldMatrix = translationMatrix,
            BindMaterialUniforms = gl => {
                gl.Uniform4(gl.GetUniformLocation(shaderHandle, "uColor"), color);
                gl.Uniform3(gl.GetUniformLocation(shaderHandle, "uLightPosition"), new Vector3());
            }
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LitMaterial(Mesh mesh, Transform transform)
    {
        ref var mat = ref _litMaterials.Get(mesh.Material.Handle.ToHandle<LitMaterial>());
        ref var shader = ref _shaders.Get(mat.Shader);
        ref var albedo = ref _textures.Get(mat.AlbedoTexture);
        ref var specular = ref _textures.Get(mat.SpecularTexture);
        var color = mat.Color;
        var lightPos = mat.LightPosition;
        var shaderHandle = shader.Handle;
        var albedoHandle = albedo.Handle;
        var specularHandle = specular.Handle;
        var lightRange = mat.LightRange;
        var lightFalloff = mat.LightFalloff;
        var translationMatrix = Matrix4x4.CreateScale(transform.Scale) * 
                                Matrix4x4.CreateFromQuaternion(transform.Rotation) * 
                                Matrix4x4.CreateTranslation(transform.Position);
        pipeline.Submit(new RenderCommand
        {
            ShaderHandle = shaderHandle,
            VaoHandle = mesh.Vao.Handle,
            IndexCount = mesh.IndicesCount,
            WorldMatrix = translationMatrix,
            BindMaterialUniforms = gl => {
                gl.ActiveTexture(TextureUnit.Texture0);
                gl.BindTexture(TextureTarget.Texture2D,  albedoHandle);
                gl.ActiveTexture(TextureUnit.Texture1);
                gl.BindTexture(TextureTarget.Texture2D,  specularHandle);
                gl.Uniform4(gl.GetUniformLocation(shaderHandle, "uColor"), color);
                gl.Uniform3(gl.GetUniformLocation(shaderHandle, "uLightPosition"), lightPos);
                gl.Uniform1(gl.GetUniformLocation(shaderHandle, "uLightFalloff"), lightRange);
                gl.Uniform1(gl.GetUniformLocation(shaderHandle, "uLightRange"), lightFalloff);
            }
        });
    }
    
    public void CollectCommands(World world)
    {
        world.Query(_query, (Entity entity, ref Renderable renderable, ref Transform transform) =>
        {
            var model = AssetPool<Model>.Registry.Get(renderable.Model);

            foreach (var mesh in model.Meshes)
            {
                switch (mesh.Material.Type)
                {
                    case MaterialType.Unlit:
                    {
                        UnlitMaterial(mesh, transform);
                        break;
                    }
                    case MaterialType.Lit:
                    {
                        LitMaterial(mesh, transform);
                        break;
                    }
                }
            }
        });
    }
}