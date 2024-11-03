using System.Net;
using System.Net.Sockets;

namespace PereViader.MusicCaster;

public static class UrlService
{
    public static string GetLocalUrl()
    {
        return $"http://{GetLocalIPAddress()}:8080";
    }
    
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            // Get only IPv4 addresses and ignore loopback addresses
            if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
            {
                return ip.ToString();
            }
        }

        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}