using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusLib
{
    public class PlcData
    {
        public static ushort[] fromPlc = new ushort[100];        // word 메모리 80개
        public static ushort[] toPlc = new ushort[100];          // word 메모리 80개
                                                                 // word 메모리 0 ~ 4 까지 BIT 로 사용하게끔 만들었다.
        public static bool[] bFromPlc = new bool[160];
        public static bool[] bToPlc = new bool[160];
    }
}
