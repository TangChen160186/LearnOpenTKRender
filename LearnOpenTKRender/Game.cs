
using Dear_ImGui_Sample;
using ImGuiNET;
using LearnOpenTKRender.OpenGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;
using System.Runtime.InteropServices;
using PixelFormat = LearnOpenTKRender.OpenGL.PixelFormat;

namespace LearnOpenTKRender
{
    struct EngineUBO
    {
        public Matrix4 Model;
        public Matrix4 View;
        public Matrix4 Projection;
        public Vector3 CameraPosition;
        public float Time;
    }

    internal class Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : GameWindow(gameWindowSettings, nativeWindowSettings)
    {
        private float[] _vertices =
        [
            -0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f,
            0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f,
            0.5f, 0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f,
            0.5f, 0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f,

            -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,
            0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f,
            -0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f,

            -0.5f, 0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            -0.5f, 0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f,
            -0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f,
            -0.5f, -0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f,

            0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f,
            0.5f, 0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f,

            -0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f,
            0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 1.0f,
            0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f,
            0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f,
            -0.5f, -0.5f, 0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f,
            -0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f,

            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f,
            0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f,
            0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f,
            -0.5f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,
            -0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f
        ];

        uint[] _index =
        [
            // Front face
            0, 1, 2, 2, 3, 0,
            // Back face
            4, 5, 6, 6, 7, 4,
            // Left face
            7, 3, 0, 0, 4, 7,
            // Right face
            1, 5, 6, 6, 2, 1,
            // Top face
            3, 2, 6, 6, 7, 3,
            // Bottom face
            0, 1, 5, 5, 4, 0
        ];

        private float[] _fullScreenVertices =
        [ 
            // 位置 (NDC坐标)    纹理坐标
            -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, // 左下
            1.0f, -1.0f, 0.0f, 1.0f, 0.0f, // 右下
            1.0f, 1.0f, 0.0f, 1.0f, 1.0f, // 右上
            -1.0f, 1.0f, 0.0f, 0.0f, 1.0f // 左上
        ];

        uint[] _fullScreenIndex =
        [
            0, 1, 2, // 第一个三角形
            2, 3, 0 // 第二个三角形
        ];

        private VertexArray _vao;
        private VertexBuffer _vbo;
        private IndexBuffer _ibo;
        private UniformBuffer _ubo;
        private Shader _shader;
        private Texture2D _diffuseTex;
        private Texture2D _specularTex;
        private Texture2D _emissionTex;

        private VertexBuffer _fullScreenVbo;
        private IndexBuffer _fullScreenIbo;
        private Shader _fullScreenShader;
        private VertexArray _fullScreenVao;
        private Texture2D _attachmentColorTexture;
        private Texture2D _attachmentDepthTexture;
        private Framebuffer _framebuffer;


        private Camera _camera;
        ImGuiController _controller;


        private bool _cameraEnable;


        protected override void OnLoad()
        {
            base.OnLoad();
            _vao = new VertexArray();
            _vbo = new VertexBuffer(_vertices);
            _ibo = new IndexBuffer(_index);

            _vao.AddVertexBuffer(_vbo, new BufferLayout([
                new BufferElement(ShaderDataType.Float3), // Vertex position
                new BufferElement(ShaderDataType.Float3), // Vertex normal
                new BufferElement(ShaderDataType.Float2), // Vertex texCoord
            ]));
            _vao.AddIndexBuffer(_ibo);

            _shader = ShaderLoader.Load("Assets/Shaders/Scene.vert.glsl", "Assets/Shaders/Scene.frag.glsl");

            GL.Viewport(0, 0, this.FramebufferSize.X, this.FramebufferSize.Y);


            _camera = new Camera(new Vector3(0, 0, 5), this.FramebufferSize.X, this.FramebufferSize.Y);

            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
            _controller.SetLightStyle(true, 0.3f);

            _ubo = new UniformBuffer(0, Marshal.SizeOf<EngineUBO>());
            _diffuseTex = TextureLoader.LoadTextureFromPath("Assets/Images/container2_diffuse.png", PixelFormat.R8G8B8A8UNorm);
            _specularTex = TextureLoader.LoadTextureFromPath("Assets/Images/container2_specular.png", PixelFormat.R8G8B8A8UNorm);
            _emissionTex =  TextureLoader.LoadTextureFromPath("Assets/Images/matrix.jpg", PixelFormat.R8G8B8A8UNorm);


            _attachmentDepthTexture = Texture2D.CreateDepthBuffer(this.FramebufferSize.X, this.FramebufferSize.Y);
            _attachmentColorTexture = Texture2D.CreateRenderTarget(this.FramebufferSize.X, this.FramebufferSize.Y);
            _framebuffer = new Framebuffer(this.FramebufferSize.X, this.FramebufferSize.Y);

            _framebuffer.AttachColorTexture(_attachmentColorTexture, 0);
            _framebuffer.AttachDepthTexture(_attachmentDepthTexture);
            _framebuffer.SetDrawBuffers(DrawBuffersEnum.ColorAttachment0);
            _fullScreenVao = new VertexArray();
            _fullScreenVbo = new VertexBuffer(_fullScreenVertices);
            _fullScreenIbo = new IndexBuffer(_fullScreenIndex);
            _fullScreenVao.AddVertexBuffer(_fullScreenVbo, new BufferLayout([
                    new BufferElement(ShaderDataType.Float3), // Vertex position
                    new BufferElement(ShaderDataType.Float2)
                ]) // Vertex texCoord
            );
            _fullScreenVao.AddIndexBuffer(_fullScreenIbo);
            _fullScreenShader = ShaderLoader.Load("Assets/Shaders/FullScreen.vert.glsl",
                "Assets/Shaders/FullScreen.frag.glsl");
            var model = ModelLoader.LoadModel("Assets/Models/german-ksk-operator-rigged/source/Idle.fbx");
            mesh = model.Meshes[3];
        }

        private StaticMesh mesh;

        override protected void OnUnload()
        {
            base.OnUnload();
            _controller.Dispose();
            _vao.Dispose();
            _vbo.Dispose();
            _ibo.Dispose();
            _ubo.Dispose();
            _shader.Dispose();
        }

        private double _time = 0;


        private Vector3 _lightPos = new Vector3(0, 0, 3);
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Enable(EnableCap.DepthTest);
            _controller.Update(this, (float)args.Time, !_cameraEnable);

            _framebuffer.Bind();
            _framebuffer.Clear(new Color4(0.2f, 0.3f, 0.3f, 1.0f));
            //_vao.Bind();
            mesh.Bind();
            _shader.Use();
            _diffuseTex.BindToUnit(0);
            _specularTex.BindToUnit(1);
            _emissionTex.BindToUnit(2);
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians((float)_time), out var model);
            //var model = Matrix4.Identity;
            var view = _camera.GetViewMatrix();
            var projection = _camera.GetProjectionMatrix();
            EngineUBO ubo = new EngineUBO()
            {
                Model = model,
                View = view,
                Projection = projection,
                CameraPosition = _camera.Position,
                Time = (float)_time,
            };
            _ubo.UpdateData(ref ubo);
            _shader.SetUniform("uLightPos", _lightPos);
            _shader.SetUniform("uLightColor", new Vector3(1, 1, 1));
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt, 0);

            _framebuffer.Unbind();
            GL.Disable(EnableCap.DepthTest);
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            _fullScreenVao.Bind();
            _fullScreenShader.Use();
            _attachmentColorTexture.BindToUnit(0);
            GL.DrawElements(PrimitiveType.Triangles, _fullScreenIbo.Count, DrawElementsType.UnsignedInt, 0);


            // Enable Docking
            //ImGui.DockSpaceOverViewport();

            _camera.OnGui();
            ImGui.Begin("Light");
            System.Numerics.Vector3 lightPos = new System.Numerics.Vector3(_lightPos.X, _lightPos.Y, _lightPos.Z);
            if (ImGui.DragFloat3("LightPos", ref lightPos))
            {
                _lightPos = new Vector3(lightPos.X, lightPos.Y, lightPos.Z);
            }
            ImGui.End();

            //ImGui.ShowDemoWindow();
            _controller.Render();
            ImGuiController.CheckGLError("End of frame");

            SwapBuffers();
            _time += args.Time * 10;
        }


        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if (_cameraEnable)
            {
                _camera.ProcessKeyboard(KeyboardState, (float)args.Time);
                _camera.ProcessMouseMovement(MouseState);
            }
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            // Tell ImGui of the new size
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
            GL.Viewport(0, 0, e.Width, e.Height);
            _camera.OnWindowResize(e.Width, e.Height);

            _attachmentDepthTexture.Dispose();
            _attachmentDepthTexture = Texture2D.CreateDepthBuffer(e.Width, e.Height);

            _attachmentColorTexture.Dispose();
            _attachmentColorTexture = Texture2D.CreateRenderTarget(e.Width, e.Height);
            _framebuffer.Dispose();
            _framebuffer = new Framebuffer(e.Width, e.Height);
            _framebuffer.AttachColorTexture(_attachmentColorTexture);
            _framebuffer.AttachDepthTexture(_attachmentDepthTexture);
            _framebuffer.SetDrawBuffers(DrawBuffersEnum.ColorAttachment0);
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            _controller.PressChar((char)e.Unicode);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _controller.MouseScroll(e.Offset);
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Keys.Tab)
            {
                if (CursorState == CursorState.Normal)
                {
                    _cameraEnable = true;
                    _camera.ResetMouseMovement();
                    CursorState = CursorState.Grabbed;
                }
                else
                {
                    CursorState = CursorState.Normal;
                    _cameraEnable = false;
                }
            }
        }
    }
}