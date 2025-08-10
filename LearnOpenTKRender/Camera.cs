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

        public float Fov
        {
            get => _fov;
            set
            {
                if (Math.Abs(_fov - value) > 0.001f)
                {
                    _fov = value;
                    _projectionMatrixDirty = true;
                }
            }
        }

        public float NearPlane
        {
            get => _nearPlane;
            set
            {
                if (Math.Abs(_nearPlane - value) > 0.001f)
                {
                    _nearPlane = value;
                    _projectionMatrixDirty = true;
                }
            }
        }

        public float FarPlane
        {
            get => _farPlane;
            set
            {
                if (Math.Abs(_farPlane - value) > 0.001f)
                {
                    _farPlane = value;
                    _projectionMatrixDirty = true;
                }
            }
        }

        private float _yaw = -90.0f;
        private float _pitch = 0.0f;
        private float _speed = 2.5f;
        private float _sensitivity = 0.1f;

        private float _aspectRatio;
        private float _fov = 45f;
        private float _nearPlane = 0.1f;
        private float _farPlane = 1000.0f;

        private bool _firstMove = true;
        private Vector2 _lastPos;

        private Matrix4 _viewMatrix;
        private bool _viewMatrixDirty = true;

        private Matrix4 _projection;
        private bool _projectionMatrixDirty = true;

        public Camera(Vector3 position, int width, int height)
        {
            Position = position;
            _aspectRatio = width * 1.0f / height;
            UpdateCameraVectors();
            _projectionMatrixDirty = true;
        }

        public Matrix4 GetViewMatrix()
        {
            if (_viewMatrixDirty)
            {
                _viewMatrix = Matrix4.LookAt(Position, Position + Front, Up);
                _viewMatrixDirty = false;
            }
            return _viewMatrix;
        }

        public Matrix4 GetProjectionMatrix()
        {
            if (_projectionMatrixDirty)
            {
                _projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov), _aspectRatio, _nearPlane, _farPlane);
                _projectionMatrixDirty = false;
            }
            return _projection;
        }

        public void ProcessKeyboard(KeyboardState keyboard, float deltaTime)
        {
            float velocity = _speed * deltaTime;
            Vector3 oldPosition = Position;

            if (keyboard.IsKeyDown(Keys.W))
                Position += Front * velocity;
            if (keyboard.IsKeyDown(Keys.S))
                Position -= Front * velocity;
            if (keyboard.IsKeyDown(Keys.A))
                Position -= Right * velocity;
            if (keyboard.IsKeyDown(Keys.D))
                Position += Right * velocity;

            // 只有位置改变时才标记为脏
            if (Position != oldPosition)
                _viewMatrixDirty = true;
        }

        public void ProcessMouseMovement(MouseState mouseState)
        {
            if (_firstMove)
            {
                _lastPos = new Vector2(mouseState.X, mouseState.Y);
                _firstMove = false;
                return;
            }

            float xOffset = mouseState.X - _lastPos.X;
            float yOffset = _lastPos.Y - mouseState.Y; // Y坐标翻转
            _lastPos = new Vector2(mouseState.X, mouseState.Y);

            // 只有鼠标移动时才处理
            if (Math.Abs(xOffset) > 0.001f || Math.Abs(yOffset) > 0.001f)
            {
                xOffset *= _sensitivity;
                yOffset *= _sensitivity;

                _yaw += xOffset;
                _pitch += yOffset;

                if (_pitch > 89.0f) _pitch = 89.0f;
                if (_pitch < -89.0f) _pitch = -89.0f;

                UpdateCameraVectors();
                _viewMatrixDirty = true;
            }
        }

        public void ResetMouseMovement()
        {
            _firstMove = true;
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

        public void OnWindowResize(int width, int height)
        {
            if (height == 0) height = 1; // 防止除零
            float newAspectRatio = (float)width / height;

            if (Math.Abs(_aspectRatio - newAspectRatio) > 0.001f)
            {
                _aspectRatio = newAspectRatio;
                _projectionMatrixDirty = true;
            }
        }
    }
}
