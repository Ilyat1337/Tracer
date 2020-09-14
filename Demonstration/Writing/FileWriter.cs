using System.IO;

namespace Demonstration.Writing
{
    class FileWriter : IWriter
    {
        private readonly string fileName;

        public FileWriter(string fileName)
        {
            this.fileName = fileName;
        }

        public void Write(string data)
        {
            File.WriteAllText(fileName, data);
        }
    }
}
