using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace LearnOpenTKRender
{
    public class Camera
    {
        public Vector3 Position { get; set; }
        public Vector3 Front { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Right { get; private set; }

        private float _yaw = -90.0f;
        private float _pitch = 0.0f;
        private float _speed = 2.5f;
        private float _sensitivity = 0.1f;

        public Camera(Vector3 position)
        {
            Position = position;
            UpdateCameraVectors();
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + Front, Up);
        }

        public void ProcessKeyboard(KeyboardState keyboard, float deltaTime)
        {
            float velocity = _speed * deltaTime;

            if (keyboard.IsKeyDown(Keys.W))
                Position += Front * velocity;
            if (keyboard.IsKeyDown(Keys.S))
                Position -= Front * velocity;
            if (keyboard.IsKeyDown(Keys.A))
                Position -= Right * velocity;
            if (keyboard.IsKeyDown(Keys.D))
                Position += Right * velocity;
        }

        public void ProcessMouseMovement(float xOffset, float yOffset)
        {
            xOffset *= _sensitivity;
            yOffset *= _sensitivity;

            _yaw += xOffset;
            _pitch += yOffset;

            if (_pitch > 89.0f) _pitch = 89.0f;
            if (_pitch < -89.0f) _pitch = -89.0f;

            UpdateCameraVectors();
        }

        private void UpdateCameraVectors()
        {
            Vector3 front;
            front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
            front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
            Front = Vector3.Normalize(front);

            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }
    }
}