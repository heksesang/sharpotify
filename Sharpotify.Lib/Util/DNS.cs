using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using DnDns;
using DnDns.Enums;
using DnDns.Query;
using DnDns.Records;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Sharpotify.Protocol;

namespace Sharpotify.Util
{
    internal static class DNS
    {
        public static List<HostnamePortPair> LookupSRV(string name)
        {
            List<HostnamePortPair> addresses = new List<HostnamePortPair>();

            DnsQueryRequest request = new DnsQueryRequest();
            IPAddress currentDns = GetActiveDnsServer();
            if (currentDns == null)
                return addresses;
            try
            {
                DnsQueryResponse response = request.Resolve(currentDns.ToString(), name, NsType.SRV, NsClass.INET, ProtocolType.Udp);
                foreach (IDnsRecord record in response.Answers)
                {
                    if (record is SrvRecord)
                    {
                        SrvRecord srvRecord = (SrvRecord)record;
                        addresses.Add(new HostnamePortPair(srvRecord.HostName, srvRecord.Port));
                    }
                }
            }
            catch (Exception)
            {
            }
            return addresses;
        }

        private static IPAddress GetActiveDnsServer()
        {
            var ifs = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var netInterface in ifs)
            {
                // Find the first Interface that is UP that isn't the loopback.
                // OperationalStatus.Up == 1
                // NetworkInterfaceType.Loopback == 0x18
                if (((int)netInterface.OperationalStatus == 1) && ((int)netInterface.NetworkInterfaceType != 0x18))
                {
                    var ip = netInterface.GetIPProperties();
                    if (ip.DnsAddresses.Count > 0)
                    {
                        if (ip.DnsAddresses[0].AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ip.DnsAddresses[0];
                        }
                        else
                        {
                            throw new InvalidOperationException("Unsupported Address Family: " + ip.DnsAddresses[0].AddressFamily.ToString());
                        }
                    }
                }
            }
            return null;
        }
    }
}
