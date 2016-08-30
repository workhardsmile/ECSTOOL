using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ECSTOOL
{
    public class CodeConvert
    {
        public static string ToGB2312(string str, string encodeType)
        {
            try
            {
                Encoding gb2312 = Encoding.GetEncoding("gb2312");//Encoding.Default ,936
                Encoding codeType = Encoding.GetEncoding(encodeType);
                byte[] temp = codeType.GetBytes(str);
                byte[] _temp = Encoding.Convert(codeType, gb2312, temp);
                string result = gb2312.GetString(_temp);
                return result;
            }
            catch
            {
                return str;
            }
        }
        public static string ToUTF8(string str, string encodeType)
        {
            try
            {
                Encoding uft8 = Encoding.GetEncoding(65001);
                Encoding codeType = Encoding.GetEncoding(encodeType);
                byte[] temp = codeType.GetBytes(str);
                byte[] _temp = Encoding.Convert(codeType, uft8, temp);
                string result = uft8.GetString(_temp);
                return result;
            }
            catch
            {
                return str;
            }
        }
    }
}
