using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp3          // 이거 이름은 자신의 프로젝트에 맞게 이름을 수정
{
    public class PlcData
    {
        static private ushort[] toPlc = new ushort[100];
        static private ushort[] fromPlc = new ushort[100];
        
        static public ushort[] ToPlc
        {
            get { return toPlc; }
            set { toPlc = value; }
        }
        static public ushort[] FromPlc
        {
            get { return fromPlc; }
            set { fromPlc = value; }
        }

    }
}
