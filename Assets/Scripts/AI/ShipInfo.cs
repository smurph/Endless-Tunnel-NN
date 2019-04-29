using System;
using System.IO;

namespace Assets.Scripts
{
    [Serializable]
    public class ShipInfo
    {
        private static string _fileName = "savedShipInfo.bin";

        public float[][][] Weights { get; set; }
        public int HighScore { get; set; }
        public int Generation { get; set; }

        public void SaveToFile()
        {
            using (Stream stream = File.Open(_fileName, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, this);
            }
        }

        public static ShipInfo LoadFromFile()
        {
            using (Stream stream = File.Open(_fileName, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (ShipInfo)binaryFormatter.Deserialize(stream);
            }
        }
    }
}
