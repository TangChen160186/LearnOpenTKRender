using System.Drawing;
using System.Runtime.InteropServices;
using Dear_ImGui_Sample;
using ImGuiNET;
using LearnOpenTKRender.OpenGL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using PixelFormat = LearnOpenTKRender.OpenGL.PixelFormat;

namespace LearnOpenTKRender
{
    struct EngineUBO
    {
        public Matrix4 Model;
        public Matrix4 View;
        public Matrix4 Projection;
    }
    internal class Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : GameWindow(gameWindowSettings, nativeWindowSettings)
    {
        float[] _vertices =
        [
            // 位置            颜色            纹理坐标
            // Front face
            -0.5f, -0.5f,  0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, // Bottom Left
            0.5f, -0.5f,  0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, // Bottom Right
            0.5f,  0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, // Top Right
            -0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, // Top Left

            // Back face
            -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, // Bottom Left
            0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 1.0f, 1.0f, 0.0f, // Bottom Right
            0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, // Top Right
            -0.5f,  0.5f, -0.5f, 0.5f, 0.5f, 0.5f, 0.0f, 1.0f, // Top Left
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
        private VertexArray _vao;

        private VertexBuffer _vbo;
        private IndexBuffer _ibo;
        private UniformBuffer _ubo;
        private Shader _shader;
        private Texture2D _diffuseTex;


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
                new BufferElement(ShaderDataType.Float3), // Vertex color
                new BufferElement(ShaderDataType.Float2), // Vertex texCoord
            ]));
            _vao.AddIndexBuffer(_ibo);

            _shader = ShaderLoader.Load("Assets/Shaders/VertexShader.glsl", "Assets/Shaders/FragmentShader.glsl");

            GL.Viewport(0, 0, this.FramebufferSize.X, this.FramebufferSize.Y);
            GL.Enable(EnableCap.DepthTest);



            _camera = new Camera(new Vector3(0, 0, 5), this.FramebufferSize.X, this.FramebufferSize.Y);
            
            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
            _controller.SetLightStyle(true, 0.3f);

            _ubo = new UniformBuffer(0, Marshal.SizeOf<EngineUBO>());
            _diffuseTex = TextureLoader.LoadTextureFromPath("Assets/Images/CONTAINER2.png", PixelFormat.R8G8B8A8UNorm);

            _attachmentDepthTexture = Texture2D.CreateDepthBuffer(this.FramebufferSize.X,this.FramebufferSize.Y);
            _attachmentColorTexture = Texture2D.CreateRenderTarget(this.FramebufferSize.X, this.FramebufferSize.Y);
        }

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
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            _controller.Update(this, (float)args.Time, !_cameraEnable);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            _vao.Bind();
            _shader.Use();
            _diffuseTex.BindToUnit(0);
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians((float)_time), out var model);
            var view = _camera.GetViewMatrix();
            var projection = _camera.GetProjectionMatrix();
            EngineUBO ubo = new EngineUBO()
            {
                Model = model,
                View = view,
                Projection = projection
            };
            _ubo.UpdateData(ref ubo);
            // _shader.SetUniform("uModel", uModel);
            // _shader.SetUniform("uView", view);
            // _shader.SetUniform("uProjection", projection);

            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.DrawElements(PrimitiveType.Triangles, _ibo.Count, DrawElementsType.UnsignedInt, 0);





            // Enable Docking
            //ImGui.DockSpaceOverViewport();

            _camera.OnGui();
            ImGui.ShowDemoWindow();
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