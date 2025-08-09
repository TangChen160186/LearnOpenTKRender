using Dear_ImGui_Sample;
using ImGuiNET;
using LearnOpenTKRender.OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTKRender
{
    internal class Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : GameWindow(gameWindowSettings, nativeWindowSettings)
    {
        float[] _vertices =
        [
            // Front face
            -0.5f, -0.5f,  0.5f, 1.0f, 0.0f, 0.0f, // Bottom Left
            0.5f, -0.5f,  0.5f, 0.0f, 1.0f, 0.0f, // Bottom Right
            0.5f,  0.5f,  0.5f, 0.0f, 0.0f, 1.0f, // Top Right
            -0.5f,  0.5f,  0.5f, 1.0f, 1.0f, 0.0f, // Top Left
    
            // Back face
            -0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 1.0f, // Bottom Left
            0.5f, -0.5f, -0.5f, 0.0f, 1.0f, 1.0f, // Bottom Right
            0.5f,  0.5f, -0.5f, 1.0f, 1.0f, 1.0f, // Top Right
            -0.5f,  0.5f, -0.5f, 0.5f, 0.5f, 0.5f, // Top Left
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

        private Shader _shader;






        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;

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
                new BufferElement(ShaderDataType.Float3),
            ]));
            _vao.AddIndexBuffer(_ibo);

            _shader = ShaderLoader.Load("Assets/Shaders/VertexShader.glsl", "Assets/Shaders/FragmentShader.glsl");

            GL.Viewport(0, 0, this.FramebufferSize.X, this.FramebufferSize.Y);
            GL.Enable(EnableCap.DepthTest);



            _camera = new Camera(new Vector3(0, 0, 5));
    


            _controller = new ImGuiController(ClientSize.X, ClientSize.Y);
        }

        private double _time = 0;
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            _controller.Update(this, (float)args.Time);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ClearColor(0.3f, 0.2f, 0.5f, 1.0f);

            
            _vao.Bind();
            _shader.Use();
     
            Matrix4.CreateRotationY(MathHelper.DegreesToRadians((float)_time), out var uModel);
            var view = _camera.GetViewMatrix();
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), this.ClientSize.X * 1.0f / this.ClientSize.Y, 0.1f, 1000f);
            _shader.SetUniform("uModel", uModel);
            _shader.SetUniform("uView", view);
            _shader.SetUniform("uProjection", projection);


            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.DrawElements(PrimitiveType.Triangles, _ibo.Count, DrawElementsType.UnsignedInt, 0);





            // Enable Docking
            //ImGui.DockSpaceOverViewport();
            ImGui.ShowDemoWindow();

            _controller.Render();

            ImGuiController.CheckGLError("End of frame");

            SwapBuffers();
            _time += args.Time * 10;
        }


        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            if(_cameraEnable)
                _camera.ProcessKeyboard(KeyboardState, (float)args.Time);
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);
            if (_cameraEnable)
            {
                if (_firstMove)
                {
                    _lastPos = new Vector2(e.X, e.Y);
                    _firstMove = false;
                }

                float xOffset = e.X - _lastPos.X;
                float yOffset = _lastPos.Y - e.Y;
                _lastPos = new Vector2(e.X, e.Y);

                _camera.ProcessMouseMovement(xOffset, yOffset);
            }
            
        }

        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            // Tell ImGui of the new size
            _controller.WindowResized(ClientSize.X, ClientSize.Y);
            GL.Viewport(0, 0, e.Width, e.Height);
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
            if (e.Shift)
            {
                CursorState = CursorState == CursorState.Grabbed ? CursorState.Normal : CursorState.Grabbed;
                if (CursorState == CursorState.Grabbed)
                {
                    _cameraEnable = true;
                }
                else
                {
                    _cameraEnable = false;
                }
            }
        }
    }
}