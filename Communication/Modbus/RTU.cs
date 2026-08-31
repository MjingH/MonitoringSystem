using Communication1;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Communication.Modbus
{
    public class RTU
    {
        //
        public Action<int, List<byte>> ResponseData;
        private static RTU _instance; // 实例
        private static SerialInfo _serialInfo;
        SerialPort serialPort;
        bool isBusing = false;
        int _funcCode;
        int _startAddr;

        private int _currentSlave;
        // 返回数据的字节长度
        private int _wordLen;

        private RTU(SerialInfo serialInfo)
        {
            serialPort = new SerialPort();
            _serialInfo = serialInfo;
        }
        public static RTU GetInstance(SerialInfo serialInfo)
        {
            lock ("rtu")
            {
                if (_instance == null)
                    _instance = new RTU(serialInfo);
                return _instance;
            }
        }

        public bool Connection()
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                }

                serialPort.PortName = _serialInfo.PortName;
                serialPort.BaudRate = _serialInfo.BaudRate;
                serialPort.DataBits = _serialInfo.DataBit;
                serialPort.Parity = _serialInfo.Parity;
                serialPort.StopBits = _serialInfo.StopBits;

                serialPort.ReceivedBytesThreshold = 1;
                serialPort.DataReceived += SerialPort_DataReceived;

                serialPort.Open();
            }
            catch
            {

                return false;
            }
            return true;
        }

        public void Dispose()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
                serialPort = null;
            }
        }

        int receByteCount = 0;
        byte[] byteBuffer = new byte[512];
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte _receiveBytes;
            // BytesToRead方法表示还有多少可以读取
            while (serialPort.BytesToRead > 0)
            {
                // ReadByte() 一次只读取 1 个字节
                _receiveBytes = (Byte)serialPort.ReadByte();
                byteBuffer[receByteCount] = _receiveBytes;
                receByteCount++;
                if (receByteCount >= 512)
                {
                    receByteCount = 0;
                    // 清除缓冲区 
                    serialPort.DiscardInBuffer();
                    return;
                }
            }
            Console.WriteLine(byteBuffer[0] +"|" +  byteBuffer[1] +"|" +  receByteCount + "啊1111111111");
            // 总长度 = 地址(1) + 功能码(1) + 长度(1) + 数据(N) + CRC(2) = N + 5
            if (byteBuffer[1] == _funcCode && receByteCount >= _wordLen + 5)
            {
                // 检查crc

                // 返回数据
                ResponseData?.Invoke(_startAddr, new List<byte>(SubyteArray(byteBuffer, 0, _wordLen + 3)));
            }
            Console.WriteLine(byteBuffer[0] + "|" + byteBuffer[1] + "|" + receByteCount + "啊1111111111");
        }

        // 发送报文
        // 对谁说话（slaveAddr）” + “做什么动作（funcCode）” + “操作哪里（startAddr）” + “操作多少（len）
        public async Task<bool> Send(int slaveAddr, byte funcCode, int startAddr, int len)
        {
            _currentSlave = slaveAddr;
            _funcCode = funcCode;
            _startAddr = startAddr;

            // 01和03返回数据的字节长度不一样需要处理
            if (funcCode == 0x01)
            {
                _wordLen = len / 8 + ((len % 8 > 0) ? 1 : 0);
            }
            if (funcCode == 0x03)
            {
                _wordLen = len*2;
            }



            List<byte> sendBuffer = new List<byte>();
            sendBuffer.Add((byte)slaveAddr);    // 第1字节：从站地址
            sendBuffer.Add((byte)funcCode);     // 第2字节：功能码

            /* 一个寄存器有两个字节，一个字节有八位，
             * 一个字节（八位）能表达0-255的任意整数，
             * 即除以整数256就会获得高八位的二进制
             * 余256就会获得低八位的二进制*/
            sendBuffer.Add((byte)(startAddr / 256)); // 第3字节：起始地址 高8位
            sendBuffer.Add((byte)(startAddr % 256)); // 第4字节：起始地址 低8位
            sendBuffer.Add((byte)(len / 256));  // 第5字节：操作数量 高8位
            sendBuffer.Add((byte)(len % 256));  // 第6字节：操作数量 低8位

            byte[] crc = Crc16(sendBuffer.ToArray(), 6);
            sendBuffer.AddRange(crc);

            try
            {
                while (isBusing) { }
                isBusing = true;
                serialPort?.Write(sendBuffer.ToArray(), 0, 8);
                isBusing = false;
                receByteCount = 0;
                await Task.Delay(1000);
            }
            catch
            {

                return false;
            }
            return true;
        }
        public static byte[] Crc16(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    else
                        crc >>= 1;
                }
            }
            return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
        }

        /*#region CRC校验查找表（预计算，程序启动时仅初始化一次）
        /// <summary>
        /// CRC-16高8位查找表（对应0x00~0xFF所有输入字节）
        /// </summary>
        private static readonly byte[] aucCRCHi = {
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
        0x00, 0xC1, 0x81, 0x40
    };

        /// <summary>
        /// CRC-16低8位查找表（对应0x00~0xFF所有输入字节）
        /// </summary>
        private static readonly byte[] aucCRCLo = {
        0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04,
        0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09, 0x08, 0xC8,
        0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC,
        0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3, 0x11, 0xD1, 0xD0, 0x10,
        0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4,
        0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A, 0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38,
        0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED, 0xEC, 0x2C,
        0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26, 0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0,
        0xA0, 0x60, 0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4,
        0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F, 0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
        0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C,
        0xB4, 0x74, 0x75, 0xB5, 0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
        0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54,
        0x9C, 0x5C, 0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 0x59, 0x99, 0x98, 0x58,
        0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
        0x44, 0x84, 0x85, 0x45, 0x47, 0x87, 0x86, 0x46, 0x42, 0x82, 0x83, 0x43, 0x81, 0x41, 0x40, 0x80
    };
        /// <summary>
        /// 计算Modbus RTU CRC-16校验值（返回16位无符号整数）
        /// </summary>
        /// <param name="data">待校验的数据字节数组</param>
        /// <param name="offset">起始偏移量（从数组的第几个字节开始计算）</param>
        /// <param name="length">需要计算的字节长度</param>
        /// <returns>16位CRC校验值（高8位在前，低8位在后）</returns>
        /// <exception cref="ArgumentNullException">数据数组为空时抛出</exception>
        /// <exception cref="ArgumentOutOfRangeException">偏移量或长度超出数组范围时抛出</exception>
        public static ushort CalculateModbusCRC(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "待校验数据数组不能为空");

            if (offset < 0 || offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "偏移量超出数组范围");

            if (length < 0 || offset + length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "计算长度超出数组范围");

            // Modbus CRC标准初始值：0xFFFF
            byte crcHi = 0xFF;
            byte crcLo = 0xFF;

            for (int i = offset; i < offset + length; i++)
            {
                byte index = (byte)(crcLo ^ data[i]);
                crcLo = (byte)(crcHi ^ aucCRCHi[index]);
                crcHi = aucCRCLo[index];
            }

            return (ushort)((crcHi << 8) | crcLo);
        }

        /// <summary>
        /// 计算Modbus RTU CRC-16校验值（重载：计算整个数组）
        /// </summary>
        /// <param name="data">待校验的数据字节数组</param>
        /// <returns>16位CRC校验值</returns>
        public static ushort CalculateModbusCRC(byte[] data)
        {
            return CalculateModbusCRC(data, 0, data.Length);
        }

        /// <summary>
        /// 计算Modbus RTU CRC-16校验值（返回字节数组，低字节在前，高字节在后）
        /// 完全匹配你代码中调用的 Crc16(sendBuffer.ToArray(), 6) 格式
        /// </summary>
        /// <param name="data">待校验的数据字节数组</param>
        /// <param name="length">需要计算的字节长度</param>
        /// <returns>2字节CRC校验数组 [低字节, 高字节]</returns>
        public static byte[] Crc16(byte[] data, int length)
        {
            ushort crc = CalculateModbusCRC(data, 0, length);
            // Modbus发送要求：先传低字节，再传高字节
            return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
        }

        /// <summary>
        /// 计算Modbus RTU CRC-16校验值（重载：计算整个数组）
        /// </summary>
        /// <param name="data">待校验的数据字节数组</param>
        /// <returns>2字节CRC校验数组 [低字节, 高字节]</returns>
        public static byte[] Crc16(byte[] data)
        {
            return Crc16(data, data.Length);
        }
        #endregion
        */
        // 截取数组
        private Byte[] SubyteArray(byte[] byteArr, int start, int len)
        {
            byte[] Res = new byte[len];
            if (byteArr != null && byteArr.Length > len)
            {
                for (int i = 0; i < len; i++)
                {
                    Res[i] = byteArr[start + i];
                }
            }
            return Res;
        }
    }
}

