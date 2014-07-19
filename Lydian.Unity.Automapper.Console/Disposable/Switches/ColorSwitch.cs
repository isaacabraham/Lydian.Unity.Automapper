using System;

namespace Lydian.Disposable.Switches
{
    public class ColorSwitch : ISwitchable
    {
        private readonly ConsoleColor color;
        private readonly ConsoleColor originalColor;

        public ColorSwitch(ConsoleColor color)
        {
            this.originalColor = Console.ForegroundColor;
            this.color = color;
        }

        public void On()
        {
            Console.ForegroundColor = color;
        }

        public void Off()
        {
            Console.ForegroundColor = originalColor;
        }
    }
}
