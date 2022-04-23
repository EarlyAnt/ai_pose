using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// 网络工具
/// </summary>
public static class NetHelper
{
    /// <summary>
    /// 从一组IP地址中获取IPv4的地址
    /// </summary>
    /// <param name="addresses">IP地址集合</param>
    /// <returns>返回IPv4地址</returns>
    public static IPAddress GetIPv4Address(IPAddress[] addresses)
    {
        IPAddress ipv4Address = null;
        IPAddress[] sorted = Sort(addresses);
        foreach (IPAddress address in sorted)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                ipv4Address = address;
                break;
            }
        }
        return ipv4Address;
    }
    /// <summary>
    /// 获取本机IP地址
    /// </summary>
    /// <returns>返回本机IPv4地址</returns>
    public static string GetLocalIPv4()
    {
        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        IPAddress ipAddress = NetHelper.GetIPv4Address(ipHostInfo.AddressList);
        return ipAddress.ToString();
    }
    /// <summary>
    /// IP地址排序
    /// </summary>
    /// <param name="addresses">地址列表</param>
    /// <returns>返回排序后的IP地址列表</returns>
    private static IPAddress[] Sort(IPAddress[] addresses)
    {
        Dictionary<string, IPAddress> dicAddresses = new Dictionary<string, IPAddress>();
        foreach (IPAddress address in addresses)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                string[] parts = address.ToString().Split(new char[] { '.' });
                string combine = "";
                foreach (string part in parts)
                {
                    combine += part.PadLeft(3, '0');
                }
                if (!dicAddresses.ContainsKey(combine))
                {
                    dicAddresses.Add(combine, address);
                }
            }
        }
        IEnumerable<KeyValuePair<string, IPAddress>> sorted = dicAddresses.OrderBy(addr => addr.Key);
        IEnumerable<IPAddress> ipAddresses = sorted.Select(x => x.Value);
        return ipAddresses.ToArray();
    }
    /// <summary>
    /// 获取IP地址的各部分组成的字节数组
    /// </summary>
    /// <param name="ipAddress">IPv4地址字符串</param>
    /// <returns>返回IP地址各部分组成的字节数组</returns>
    public static byte[] GetIPv4Bytes(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
        {
            throw new ArgumentException("参数[ipAddress]为空。");
        }
        List<byte> bytes = new List<byte>();
        string[] parts = ipAddress.Split('.');
        if (parts == null || parts.Length < 4)
        {
            throw new ArgumentException("参数[ipAddress]的格式不正确。");
        }
        try
        {
            foreach (string part in parts)
            {
                bytes.Add(byte.Parse(part));
            }
            return bytes.ToArray();
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("转换IP地址发生异常，原因是：{0}", ex.Message), ex);
        }
    }
    /// <summary>
    /// 获取System.Net.IPAddress的实例
    /// </summary>
    /// <param name="ipAddress">IP4v地址地府穿</param>
    /// <returns>返回System.Net.IPAddress的实例</returns>
    public static IPAddress GetIPv4Address(string ipAddress)
    {
        byte[] bytes = GetIPv4Bytes(ipAddress);
        return new IPAddress(bytes);
    }
    /// <summary>
    /// 测试网络畅通与否
    /// </summary>
    /// <param name="ipAddress">IPv4地址字符串</param>
    /// <returns>网络畅通返回True，否则返回False</returns>
    public static bool Ping(string ipAddress)
    {
        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
        System.Net.NetworkInformation.PingOptions options = new System.Net.NetworkInformation.PingOptions();
        options.DontFragment = true;
        byte[] buffer = Encoding.ASCII.GetBytes("Test!");
        int timeout = 1000;
        System.Net.NetworkInformation.PingReply reply = ping.Send(ipAddress, timeout, buffer, options);
        return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
    }
}
