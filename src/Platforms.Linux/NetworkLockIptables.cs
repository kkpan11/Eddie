﻿// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2016 AirVPN (support@airvpn.org) / https://airvpn.org
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Eddie.Core;

namespace Eddie.Platforms.Linux
{
	public class NetworkLockIptables : NetworkLockPlugin
	{
		private IpAddresses m_currentList = new IpAddresses();

		public override string GetCode()
		{
			return "linux_iptables";
		}

		public override string GetName()
		{
			return "Linux iptables";
		}

		public override bool GetSupport()
		{
			if (Platform.Instance.LocateExecutable("iptables") == "")
				return false;
			if (Platform.Instance.LocateExecutable("iptables-save") == "")
				return false;
			if (Platform.Instance.LocateExecutable("iptables-restore") == "")
				return false;
			if (Platform.Instance.LocateExecutable("ip6tables") == "")
				return false;
			if (Platform.Instance.LocateExecutable("ip6tables-save") == "")
				return false;
			if (Platform.Instance.LocateExecutable("ip6tables-restore") == "")
				return false;
			
			return true;
		}

		public string GetBackupPath(string ipVersion)
		{
			if (ipVersion == "4") // For compatibility with Eddie<2.9
				ipVersion = "";
			return Storage.DataPath + Platform.Instance.DirSep + "ip" + ipVersion + "tables.dat";
		}

		public override void Activation()
		{
			base.Activation();

			string rulesBackupSessionV4 = GetBackupPath("4");
            string rulesBackupSessionV6 = GetBackupPath("6");

            if( (Platform.Instance.FileExists(rulesBackupSessionV4)) || (Platform.Instance.FileExists(rulesBackupSessionV6)) )
            {
				Engine.Instance.Logs.Log(LogType.Warning, Messages.NetworkLockLinuxUnexpectedAlreadyActive);
				Deactivation();
			}

			// IPv4 - Backup
			SystemShell.ShellCmdWithException("iptables-save >\"" + SystemShell.EscapePath(rulesBackupSessionV4) + "\"");

			// IPv6 - Backup
			SystemShell.ShellCmdWithException("ip6tables-save >\"" + SystemShell.EscapePath(rulesBackupSessionV6) + "\"");

			// IPv4 - Flush
			SystemShell.ShellCmdWithException("iptables -P INPUT ACCEPT");
			SystemShell.ShellCmdWithException("iptables -P FORWARD ACCEPT");
			SystemShell.ShellCmdWithException("iptables -P OUTPUT ACCEPT");
			SystemShell.ShellCmdWithException("iptables -t nat -F");
			SystemShell.ShellCmdWithException("iptables -t mangle -F");
			SystemShell.ShellCmdWithException("iptables -F");
			SystemShell.ShellCmdWithException("iptables -X");

			// IPv6 - Flush
			SystemShell.ShellCmdWithException("ip6tables -P INPUT ACCEPT");
			SystemShell.ShellCmdWithException("ip6tables -P FORWARD ACCEPT");
			SystemShell.ShellCmdWithException("ip6tables -P OUTPUT ACCEPT");
			SystemShell.ShellCmdWithException("ip6tables -t nat -F");
			SystemShell.ShellCmdWithException("ip6tables -t mangle -F");
			SystemShell.ShellCmdWithException("ip6tables -F");
			SystemShell.ShellCmdWithException("ip6tables -X");

			// IPv4 - Local
			SystemShell.ShellCmdWithException("iptables -A INPUT -i lo -j ACCEPT");
			SystemShell.ShellCmdWithException("iptables -A OUTPUT -o lo -j ACCEPT");

			// IPv6 - Local
			SystemShell.ShellCmdWithException("ip6tables -A INPUT -i lo -j ACCEPT");
			SystemShell.ShellCmdWithException("ip6tables -A OUTPUT -o lo -j ACCEPT");

			// IPv6 - Disable processing of any RH0 packet which could allow a ping-pong of packets
			SystemShell.ShellCmdWithException("ip6tables -A INPUT -m rt --rt-type 0 -j DROP");
			SystemShell.ShellCmdWithException("ip6tables -A OUTPUT -m rt --rt-type 0 -j DROP");
			SystemShell.ShellCmdWithException("ip6tables -A FORWARD -m rt --rt-type 0 -j DROP");

			// Make sure you can communicate with any DHCP server
			SystemShell.ShellCmdWithException("iptables -A OUTPUT -d 255.255.255.255 -j ACCEPT");
			SystemShell.ShellCmdWithException("iptables -A INPUT -s 255.255.255.255 -j ACCEPT");

			if (Engine.Instance.Storage.GetBool("netlock.allow_private"))
			{
				// IPv4 - Private networks
				SystemShell.ShellCmdWithException("iptables -A INPUT -s 192.168.0.0/16 -d 192.168.0.0/16 -j ACCEPT");
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 192.168.0.0/16 -d 192.168.0.0/16 -j ACCEPT");
				SystemShell.ShellCmdWithException("iptables -A INPUT -s 10.0.0.0/8 -d 10.0.0.0/8 -j ACCEPT");
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 10.0.0.0/8 -d 10.0.0.0/8 -j ACCEPT");
				SystemShell.ShellCmdWithException("iptables -A INPUT -s 172.16.0.0/12 -d 172.16.0.0/12 -j ACCEPT");
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 172.16.0.0/12 -d 172.16.0.0/12 -j ACCEPT");

				// IPv4 - Multicast
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 192.168.0.0/16 -d 224.0.0.0/24 -j ACCEPT");
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 192.168.0.0/16 -d 224.0.0.0/24 -j ACCEPT");
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 192.168.0.0/16 -d 224.0.0.0/24 -j ACCEPT");

				// IPv4 - 239.255.255.250  Simple Service Discovery Protocol address
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 192.168.0.0/16 -d 239.255.255.250/32 -j ACCEPT");
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 192.168.0.0/16 -d 239.255.255.250/32 -j ACCEPT");
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 192.168.0.0/16 -d 239.255.255.250/32 -j ACCEPT");

				// IPv4 - 239.255.255.253  Service Location Protocol version 2 address
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 192.168.0.0/16 -d 239.255.255.253/32 -j ACCEPT");
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 192.168.0.0/16 -d 239.255.255.253/32 -j ACCEPT");
				SystemShell.ShellCmdWithException("iptables -A OUTPUT -s 192.168.0.0/16 -d 239.255.255.253/32 -j ACCEPT");

				// IPv6 - Allow Link-Local addresses
				SystemShell.ShellCmdWithException("ip6tables -A INPUT -s fe80::/10 -j ACCEPT");
				SystemShell.ShellCmdWithException("ip6tables -A OUTPUT -s fe80::/10 -j ACCEPT");

				// IPv6 - Allow multicast
				SystemShell.ShellCmdWithException("ip6tables -A INPUT -d ff00::/8 -j ACCEPT");
				SystemShell.ShellCmdWithException("ip6tables -A OUTPUT -d ff00::/8 -j ACCEPT");
			}

			if (Engine.Instance.Storage.GetBool("netlock.allow_ping"))
			{
				// IPv4
				SystemShell.ShellCmdWithException("iptables -A INPUT -p icmp --icmp-type echo-request -j ACCEPT");

				// IPv6
				SystemShell.ShellCmdWithException("ip6tables -A INPUT -p icmpv6 -j ACCEPT");
				SystemShell.ShellCmdWithException("ip6tables -A OUTPUT -p icmpv6 -j ACCEPT");
			}

			// IPv4 - Allow established sessions to receive traffic
			SystemShell.ShellCmdWithException("iptables -A INPUT -m state --state ESTABLISHED,RELATED -j ACCEPT");
			// IPv6 - Allow established sessions to receive traffic
			SystemShell.ShellCmdWithException("ip6tables -A INPUT -m state --state ESTABLISHED,RELATED -j ACCEPT");

			// IPv4 - Allow TUN
			SystemShell.ShellCmdWithException("iptables -A INPUT -i tun+ -j ACCEPT");
			SystemShell.ShellCmdWithException("iptables -A FORWARD -i tun+ -j ACCEPT");
			SystemShell.ShellCmdWithException("iptables -A OUTPUT -o tun+ -j ACCEPT");

			// IPv6 - Allow TUN 
			SystemShell.ShellCmdWithException("ip6tables -A INPUT -i tun+ -j ACCEPT");
			SystemShell.ShellCmdWithException("ip6tables -A FORWARD -i tun+ -j ACCEPT");
			SystemShell.ShellCmdWithException("ip6tables -A OUTPUT -o tun+ -j ACCEPT");

			// IPv4 - Block All
			SystemShell.ShellCmdWithException("iptables -A OUTPUT -j DROP");
			SystemShell.ShellCmdWithException("iptables -A INPUT -j DROP");
			SystemShell.ShellCmdWithException("iptables -A FORWARD -j DROP");

			// IPv6 - Block All
			SystemShell.ShellCmdWithException("ip6tables -A OUTPUT -j DROP");
			SystemShell.ShellCmdWithException("ip6tables -A INPUT -j DROP");
			SystemShell.ShellCmdWithException("ip6tables -A FORWARD -j DROP");

			OnUpdateIps();
			
		}

		public override void Deactivation()
		{
			base.Deactivation();

			// IPv4
			string rulesBackupSessionV4 = GetBackupPath("4");

			if (Platform.Instance.FileExists(rulesBackupSessionV4))
			{
				// Flush
				SystemShell.ShellCmd("iptables -P INPUT ACCEPT");
				SystemShell.ShellCmd("iptables -P FORWARD ACCEPT");
				SystemShell.ShellCmd("iptables -P OUTPUT ACCEPT");				
				SystemShell.ShellCmd("iptables -t nat -F");
				SystemShell.ShellCmd("iptables -t mangle -F");
				SystemShell.ShellCmd("iptables -F");
				SystemShell.ShellCmd("iptables -X");

				// Restore
				SystemShell.ShellCmd("iptables-restore <\"" + SystemShell.EscapePath(rulesBackupSessionV4) + "\""); 

                Platform.Instance.FileDelete(rulesBackupSessionV4);
			}

			// IPv6
			string rulesBackupSessionV6 = GetBackupPath("6");

			if (Platform.Instance.FileExists(rulesBackupSessionV6))
			{
				// Restore
				SystemShell.ShellCmd("ip6tables -P INPUT ACCEPT");
				SystemShell.ShellCmd("ip6tables -P FORWARD ACCEPT");
				SystemShell.ShellCmd("ip6tables -P OUTPUT ACCEPT");
				SystemShell.ShellCmd("ip6tables -t nat -F");
				SystemShell.ShellCmd("ip6tables -t mangle -F");
				SystemShell.ShellCmd("ip6tables -F");
				SystemShell.ShellCmd("ip6tables -X");

				// Backup
				SystemShell.ShellCmd("ip6tables-restore <\"" + SystemShell.EscapePath(rulesBackupSessionV6) + "\""); 

                Platform.Instance.FileDelete(rulesBackupSessionV6);
			}

			// IPS
			m_currentList.Clear();
		}

		public override void OnUpdateIps()
		{
			base.OnUpdateIps();

			IpAddresses ipsFirewalled = GetAllIps(true);

			// Remove IP not present in the new list
			foreach (IpAddress ip in m_currentList.IPs)
			{
				if(ipsFirewalled.Contains(ip) == false)
				{
					// Remove
					string cmd = "";
					if(ip.IsV4)
						cmd = "iptables -D OUTPUT -d " + ip.ToCIDR() + " -j ACCEPT";
					else if(ip.IsV6)
						cmd = "ip6tables -D OUTPUT -d " + ip.ToCIDR() + " -j ACCEPT";
					SystemShell.ShellCmdWithException(cmd);
				}
			}

			// Add IP
			foreach (IpAddress ip in ipsFirewalled.IPs)
			{
				if (m_currentList.Contains(ip) == false)
				{
					// Add
					string cmd = "";
					if(ip.IsV4)
						cmd = "iptables -I OUTPUT 1 -d " + ip.ToCIDR() + " -j ACCEPT";
					else if(ip.IsV6)
						cmd = "ip6tables -I OUTPUT 1 -d " + ip.ToCIDR() + " -j ACCEPT";
					SystemShell.ShellCmdWithException(cmd);
				}
			}

			m_currentList = ipsFirewalled;
		}		
	}
}
