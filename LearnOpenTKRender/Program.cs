using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTKRender
{
    internal class Program
    {
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
                    ClientSize = new Vector2i(800, 600),
                    Title = "Learn OpenTK Render"
                }
            );
            
            game.Run();
            
        }
    }
}
