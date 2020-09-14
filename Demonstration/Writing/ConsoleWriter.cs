using System;

namespace Demonstration.Writing
{
    class ConsoleWriter : IWriter
    {
        public void Write(string data)
        {
            Console.WriteLine(data);
        }
    }
}
