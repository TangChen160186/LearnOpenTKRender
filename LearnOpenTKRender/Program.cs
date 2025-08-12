using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTKRender
{
    internal class Program
    {
        private const int Width = 1920;
        private const int Height = 1080;
        static void Main(string[] args)
        {

            using Game game = new Game(
                new GameWindowSettings()
                {
                    UpdateFrequency = 60,
                    Win32SuspendTimerOnDrag = true
                },
                new NativeWindowSettings()
                {
                    ClientSize = new Vector2i(Width, Height),
                    Title = "Learn OpenTK Render"
                }
            );
            
            game.Run();
            
        }
    }
}
