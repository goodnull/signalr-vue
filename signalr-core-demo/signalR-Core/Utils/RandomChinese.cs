using System;
using System.Text;

/// 可以随机生成一个长度为2的十六进制字节数组，
/// 使用GetString ()方法对其进行解码就可以得到汉字字符了。
/// 不过对于生成中文汉字验证码来说，因为第15区也就是AF区以前都没有汉字，
/// 只有少量符号，汉字都从第16区B0开始，并且从区位D7开始以后的汉字都是和很难见到的繁杂汉字，
/// 所以这些都要排出掉。所以随机生成的汉字十六进制区位码第1位范围在B、C、D之间，
/// 如果第1位是D的话，第2位区位码就不能是7以后的十六进制数。
/// 在来看看区位码表发现每区的第一个位置和最后一个位置都是空的，没有汉字，
/// 因此随机生成的区位码第3位如果是A的话，第4位就不能是0；第3位如果是F的话，
/// 第4位就不能是F。知道了原理，随机生成中文汉字的程序也就出来了， 

/// 以下就是生成长度为N的随机汉字C#台代码：
public class RandomChinese
{
    public static string GetRandomChinese(int strlength)
    {
        // 获取GB2312编码页（表） 
        Encoding gb = Encoding.GetEncoding("gb2312");

        object[] bytes = CreateRegionCode(strlength);

        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < strlength; i++)
        {
            string temp = gb.GetString((byte[])Convert.ChangeType(bytes[i], typeof(byte[])));
            sb.Append(temp);
        }

        return sb.ToString();
    }

    /** 
    此函数在汉字编码范围内随机创建含两个元素的十六进制字节数组，每个字节数组代表一个汉字，并将 
    四个字节数组存储在object数组中。 
    参数：strlength，代表需要产生的汉字个数 
    **/
    private static object[] CreateRegionCode(int strlength)
    {
        //定义一个字符串数组储存汉字编码的组成元素 
        string[] rBase = new String[16] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" };

        Random rnd = new Random();

        //定义一个object数组用来 
        object[] bytes = new object[strlength];

        /**
         每循环一次产生一个含两个元素的十六进制字节数组，并将其放入bytes数组中 
         每个汉字有四个区位码组成 
         区位码第1位和区位码第2位作为字节数组第一个元素 
         区位码第3位和区位码第4位作为字节数组第二个元素 
        **/
        for (int i = 0; i < strlength; i++)
        {
            //区位码第1位 
            int r1 = rnd.Next(11, 14);
            string str_r1 = rBase[r1].Trim();

            //区位码第2位 
            rnd = new Random(r1 * unchecked((int)DateTime.Now.Ticks) + i); // 更换随机数发生器的 种子避免产生重复值 
            int r2;
            if (r1 == 13)
            {
                r2 = rnd.Next(0, 7);
            }
            else
            {
                r2 = rnd.Next(0, 16);
            }
            string str_r2 = rBase[r2].Trim();

            //区位码第3位 
            rnd = new Random(r2 * unchecked((int)DateTime.Now.Ticks) + i);
            int r3 = rnd.Next(10, 16);
            string str_r3 = rBase[r3].Trim();

            //区位码第4位 
            rnd = new Random(r3 * unchecked((int)DateTime.Now.Ticks) + i);
            int r4;
            if (r3 == 10)
            {
                r4 = rnd.Next(1, 16);
            }
            else if (r3 == 15)
            {
                r4 = rnd.Next(0, 15);
            }
            else
            {
                r4 = rnd.Next(0, 16);
            }
            string str_r4 = rBase[r4].Trim();

            // 定义两个字节变量存储产生的随机汉字区位码 
            byte byte1 = Convert.ToByte(str_r1 + str_r2, 16);
            byte byte2 = Convert.ToByte(str_r3 + str_r4, 16);
            // 将两个字节变量存储在字节数组中 
            byte[] str_r = new byte[] { byte1, byte2 };

            // 将产生的一个汉字的字节数组放入object数组中 
            bytes.SetValue(str_r, i);
        }

        return bytes;
    }
}