using System.Net;
using System.Net.Sockets;
using LASYS.Application.Interfaces.Services;

namespace LASYS.Infrastructure.Services.System
{
    public sealed class IpAddressProvider : IIpAddressProvider
    {
        public string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            var ip = host.AddressList
                .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

            return ip?.ToString() ?? "127.0.0.1";
        }
    }
}
