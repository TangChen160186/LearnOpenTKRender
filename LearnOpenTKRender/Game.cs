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
            -0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, // Bottom Left
            0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, // Bottom Right
            0.0f, 0.5f, 0.0f, 0.0f, 0.0f, 1.0f, // Top
        ];

        uint[] _index =
        [
            0, 1, 2 // Triangle
        ];

        private VertexArray _vao;

        private VertexBuffer _vbo;
        private IndexBuffer _ibo;

        private Shader _shader;

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

            GL.Viewport(0, 0, 800, 600);
      
        }

        private double _time = 0;
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(0.3f, 0.2f, 0.5f, 1.0f);

            
            _vao.Bind();
            _shader.Use();
     
            Matrix4.CreateRotationZ(MathHelper.DegreesToRadians((float)_time), out var uModel);
            var view =  Matrix4.LookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.UnitY);
            var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90), this.ClientSize.X * 1.0f / this.ClientSize.Y, 0.1f, 1000f);
            _shader.SetUniform("uModel", uModel);
            _shader.SetUniform("uView", view);
            _shader.SetUniform("uProjection", projection);


            //GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
            GL.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();

            _time += args.Time * 10;
        }


        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }


        protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
        {
            base.OnFramebufferResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }
    }
}