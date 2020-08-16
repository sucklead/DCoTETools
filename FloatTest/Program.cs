using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloatTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string originalsrc = "93.02";

            float f = float.Parse(originalsrc);

            string newsrc = f.ToString();

            Console.WriteLine("originalsrc {0}", originalsrc);
            Console.WriteLine("f {0}", f);
            Console.WriteLine("newsrc {0}", newsrc);

            //byte[] bytes1 = { 0x15, 0x43, 0x01, 0x3D};
            byte[] bytes1 = { 0x3D, 0x0A, 0xBA, 0x42};

            float newfloat1 = BitConverter.ToSingle(bytes1, 0);
            Console.WriteLine("newfloat1 {0}", newfloat1);

            byte[] bytes2 = { 0x3C, 0x0A, 0xBA, 0x42 };

            float newfloat2 = BitConverter.ToSingle(bytes2, 0);
            Console.WriteLine("newfloat2 {0}", newfloat2);

            byte[] newbytes1 = BitConverter.GetBytes(newfloat1);
            Console.WriteLine("newbytes1 {0}", BitConverter.ToString(newbytes1, 0));

            byte[] newbytes2 = BitConverter.GetBytes(newfloat2);
            Console.WriteLine("newbytes2 {0}", BitConverter.ToString(newbytes2, 0));

            Int32 newint32_1 = BitConverter.ToInt32(bytes1, 0);
            Console.WriteLine("newint32_1 {0}", newint32_1);

            Int32 newint32_2 = BitConverter.ToInt32(bytes2, 0);
            Console.WriteLine("newint32_2 {0}", newint32_2);

            byte[] endbytes1 = BitConverter.GetBytes(newint32_1);
            Console.WriteLine("endbytes1 {0}", BitConverter.ToString(endbytes1, 0));

            byte[] endbytes2 = BitConverter.GetBytes(newint32_2);
            Console.WriteLine("endbytes2 {0}", BitConverter.ToString(endbytes2, 0));



            Console.ReadLine();
        }
    }
}
