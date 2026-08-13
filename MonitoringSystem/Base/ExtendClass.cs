using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringSystem.Base
{
    // 将一个 4 字节的数组（byte[]）重新解释为一个 32 位单精度浮点数（float）
    public static class ExtendClass
    {
        public static float ByteArrsyToFolat(this byte[] value)
        {
            float fValue = 0f;
            uint nRest = ((uint)value[2]) * 256 + ((uint)value[3]) + 65536 * (((uint)value[0]) * 256
                + ((uint)value[1]));
            unsafe
            {
                float* ptemp;
                ptemp = (float*)(&nRest);
                fValue = *ptemp;
            }
            return fValue;
        }
    }
}
