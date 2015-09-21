using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace BlockStation.Query
{
    public class MCQuery : ServerQuery
    {
        private bool success = false;
        private ServerInfo info;
        private int challenge = 0;
        private int SID = 0;
        private int ping = 0;

        private Socket sock;

        public bool Success() { return success; }
        public ServerInfo Info() { return info; }

        public void Connect(string host, int port = 25565, double timeout = 2.5)
        {
            try
            {
                sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                if (sock == null) return;

                sock.Connect(host, port);
                sock.ReceiveTimeout = (int)(timeout * 1000);
                sock.SendTimeout = (int)(timeout * 1000);

                this.GetChallenge();
            }
            catch
            {
                success = false;
            }
        }

        internal void GetChallenge()
        {
            SID = new Random().Next() & 0xCF;
            Packet data = this.GrabData(Packets.Challenge(this.SID));
            if (data.Read<byte>() == (byte)0x09 && data.Read<int>() == this.SID)
            {
                int.TryParse(data.Read<String>(), out this.challenge);
                byte[] iabi = BitConverter.GetBytes(this.challenge);
                Array.Reverse(iabi);
                this.challenge = BitConverter.ToInt32(iabi, 0);

                ping = Environment.TickCount;

                this.GetInfo();
            }
            else GetChallenge();
        }

        internal void GetInfo()
        {
            Packet data = this.GrabData(Packets.QueryData(this.SID, this.challenge));

            if (data == null)
            {
                this.GetChallenge();
                return;
            }

            if (data.Read<byte>() == (byte)0x00 && data.Read<int>() == this.SID)
            {
                info = new ServerInfo();

                info.Latency = Environment.TickCount - ping;

                data.Skip(11); // Unknown padding.

                string key, value;

                while (true)
                {
                    key = data.Read<string>();
                    value = data.Read<string>();

                    if (key.Length == 0) break;

                    if (key == "hostname")
                        info.Name = value;

                    else if (key == "gametype")
                        info.GameType = value;

                    else if (key == "game_id")
                        info.GameID = value;

                    else if (key == "version")
                        info.Version = value;

                    else if (key == "plugins")
                        info.Plugins = value;

                    else if (key == "map")
                        info.Map = value;

                    else if (key == "numplayers")
                    {
                        if (!int.TryParse(value, out info.OnlinePlayers)) return;
                    }

                    else if (key == "maxplayers")
                    {
                        if (!int.TryParse(value, out info.MaxPlayers)) return;
                    }

                    else if (key == "hostport")
                        info.HostPort = value;

                    else if (key == "hostip")
                        info.HostIP = value;
                }

                data.Skip(1);

                info.Players = new List<string>();

                while (true)
                {
                    key = data.Read<string>();

                    if (key.Length == 0) break;

                    info.Players.Add(key);
                }

                success = true;
            }
            else GetChallenge();
        }

        internal Packet GrabData(byte[] packet)
        {
            this.Send(packet);

            Packet recv = this.Receive(2048, packet[2]);

            if (recv == null) return null;

            return recv;
        }

        internal Packet Receive(int len, byte check)
        {
            try
            {
                byte[] buffer = new byte[len];
                int recv = this.sock.Receive(buffer, 0, len, SocketFlags.None);

                if (recv < 5 || buffer[0] != check)
                    return null;

                byte[] nbuf = new byte[recv];
                Buffer.BlockCopy(buffer, 0, nbuf, 0, recv);

                return new Packet(nbuf);
            }
            catch
            {
                return null;
            }
        }

        internal void Send(byte[] data)
        {
            try
            {
                sock.Send(data);
            }
            catch
            {

            }
        }

        internal void Send(string data)
        {
            try
            {
                this.Send(Encoding.Unicode.GetBytes(data));
            }
            catch
            {

            }
        }
    }

    public class MCSimpleQuery : ServerQuery
    {
        private bool success = false;
        private ServerInfo info;

        public bool Success() { return success; }
        public ServerInfo Info() { return info; }

        public void Connect(string host, int port = 25565, double timeout = 2.5)
        {
            try
            {
                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (sock == null) return;

                sock.Connect(host, port);
                sock.ReceiveTimeout = (int)(timeout * 1000);
                sock.SendTimeout = (int)(timeout * 1000);

                int start = System.Environment.TickCount;

                sock.Send(new byte[2] { 0xFE, 0x01 });

                byte[] buffer = new byte[512];

                int recv = sock.Receive(buffer, 0, 512, SocketFlags.None);

                if (recv < 4 || buffer[0] != 0xFF) return;

                byte[] nbuf = new byte[recv];
                System.Buffer.BlockCopy(buffer, 0, nbuf, 0, recv);

                string packet = System.Text.Encoding.UTF8.GetString(nbuf).Substring(3);
                string[] bits;

                if (packet[1] != 0xA7 && packet[2] != 0x31)
                {
                    // MC 1.4 and later?
                    bits = packet.Split(new string[1] { "\x00\x00\x00" }, System.StringSplitOptions.None);

                    info = new ServerInfo();

                    info.Latency = System.Environment.TickCount - start;

                    if (!double.TryParse(bits[1].Replace("\x00", ""), out info.Protocol)) return;

                    info.Version = bits[2].Replace("\x00", "");
                    info.Name = bits[3].Replace("\x00", "");

                    if (!int.TryParse(bits[4].Replace("\x00", ""), out info.OnlinePlayers)) return;
                    if (!int.TryParse(bits[5].Replace("\x00", ""), out info.MaxPlayers)) return;

                    success = true;
                    return;
                }
                else
                {
                    // Earlier versions
                    bits = packet.Split(new char[1] { '\xA7' });

                    info = new ServerInfo();

                    info.Latency = System.Environment.TickCount - start;

                    info.Version = "1.3";
                    info.Name = bits[0].Replace("\x00", "");

                    if (bits.Length > 1)
                        if (!int.TryParse(bits[4].Replace("\x00", ""), out info.OnlinePlayers)) return;

                    if (bits.Length > 2)
                        if (!int.TryParse(bits[5].Replace("\x00", ""), out info.MaxPlayers)) return;

                    success = true;
                    return;
                }
            }
            catch
            {
                success = false;
            }
        }
    }

    internal class Packet
    {
        private byte[] buffer;
        private Int32 location;
        private System.Int32 length;

        internal Int32 Location
        {
            get
            {
                return this.location;
            }
        }

        internal Int32 Length
        {
            get
            {
                return this.length;
            }
        }

        internal Packet()
        {
            this.buffer = new byte[65535];
            this.length = 0;
        }

        internal Packet(Int32 size)
        {
            this.buffer = new byte[size];
            this.length = size;
        }

        internal Packet(byte[] data)
        {
            this.buffer = data;
            this.length = data.Length;
        }

        internal byte[] Finalize()
        {
            byte[] final = new byte[this.length];
            Buffer.BlockCopy(this.buffer, 0, final, 0, this.length);
            return final;
        }

        internal void Write(object value, Int32 offset = 0)
        {
            this.location = offset > 0 ? offset : this.location;

            Type T = value.GetType();

            if (T == typeof(byte))
            {
                if (this.CheckBounds(1, T.Name))
                {
                    byte o = Convert.ToByte(value);
                    this.buffer[this.location++] = (byte)o;
                    if (this.location >= this.length)
                        this.length += 1;
                }
            }

            else if (T == typeof(Char))
            {
                if (this.CheckBounds(2, T.Name))
                {
                    Char o = Convert.ToChar(value);
                    this.buffer[this.location++] = (byte)o;
                    this.buffer[this.location++] = (byte)((int)o >> 8);
                    if (this.location >= this.length)
                        this.length += 2;
                }
            }

            else if (T == typeof(Int16) || T == typeof(UInt16))
            {
                if (this.CheckBounds(2, T.Name))
                {
                    Int16 o = Convert.ToInt16(value);
                    this.buffer[this.location++] = (byte)o;
                    this.buffer[this.location++] = (byte)((int)o >> 8);
                    if (this.location >= this.length)
                        this.length += 2;
                }
            }

            else if (T == typeof(Int32) || T == typeof(UInt32) || T == typeof(Single))
            {
                if (this.CheckBounds(4, T.Name))
                {
                    Int32 o = Convert.ToInt32(value);
                    this.buffer[this.location++] = (byte)o;
                    this.buffer[this.location++] = (byte)((int)o >> 8);
                    this.buffer[this.location++] = (byte)((int)o >> 16);
                    this.buffer[this.location++] = (byte)((int)o >> 24);
                    if (this.location >= this.length)
                        this.length += 4;
                }
            }

            else if (T == typeof(Int64) || T == typeof(UInt64) || T == typeof(Double))
            {
                if (this.CheckBounds(8, T.Name))
                {
                    Int64 o = Convert.ToInt64(value);
                    this.buffer[this.location++] = (byte)o;
                    this.buffer[this.location++] = (byte)((int)o >> 8);
                    this.buffer[this.location++] = (byte)((int)o >> 16);
                    this.buffer[this.location++] = (byte)((int)o >> 24);
                    this.buffer[this.location++] = (byte)((int)o >> 32);
                    this.buffer[this.location++] = (byte)((int)o >> 40);
                    this.buffer[this.location++] = (byte)((int)o >> 48);
                    this.buffer[this.location++] = (byte)((int)o >> 56);
                    if (this.location >= this.length)
                        this.length += 8;
                }
            }

            else if (T == typeof(String))
            {
                byte[] string_bytes = System.Text.Encoding.UTF8.GetBytes(Convert.ToString(value));
                if (this.CheckBounds(string_bytes.Length, T.Name))
                {
                    Buffer.BlockCopy(string_bytes, 0, this.buffer, this.location, string_bytes.Length);
                    this.location += string_bytes.Length;
                    if (this.location >= this.length)
                        this.length += string_bytes.Length + 1;
                }
            }

            else throw new NotSupportedException("Unsupported type for writing: " + T.Name);
        }

        internal R Read<R>(Int32 offset = 0)
        {
            this.location = offset > 0 ? offset : this.location;

            Type T = typeof(R);

            if (T == typeof(byte))
            {
                if (this.CheckBounds(1, T.Name))
                {
                    R ret = (R)Convert.ChangeType(this.buffer[this.location], T);

                    this.location++;
                    return ret;
                }
            }

            else if (T == typeof(Char) || T == typeof(Int16) || T == typeof(UInt16))
            {
                if (this.CheckBounds(2, T.Name))
                {
                    R ret = (R)Convert.ChangeType(
                        (
                            (this.buffer[this.location + 1] << 8) +
                            this.buffer[this.location]
                        ), T);

                    this.location += 2;
                    return ret;
                }
            }

            else if (T == typeof(Single))
            {
                R ret = (R)Convert.ChangeType(System.BitConverter.ToSingle(this.buffer, this.location), T);
                this.location += 4;
                return ret;
            }

            else if (T == typeof(Int32) || T == typeof(UInt32))
            {
                if (this.CheckBounds(4, T.Name))
                {
                    R ret = (R)Convert.ChangeType(
                        (
                            (this.buffer[this.location + 3] << 24) +
                            (this.buffer[this.location + 2] << 16) +
                            (this.buffer[this.location + 1] << 8) +
                            this.buffer[this.location]
                        ), T);

                    this.location += 4;
                    return ret;
                }
            }

            else if (T == typeof(Double))
            {
                R ret = (R)Convert.ChangeType(System.BitConverter.ToDouble(this.buffer, this.location), T);
                this.location += 8;
                return ret;
            }

            else if (T == typeof(Int64) || T == typeof(UInt64))
            {
                if (this.CheckBounds(8, T.Name))
                {
                    R ret = (R)Convert.ChangeType(
                        (
                            (this.buffer[this.location + 7] << 56) +
                            (this.buffer[this.location + 6] << 48) +
                            (this.buffer[this.location + 5] << 40) +
                            (this.buffer[this.location + 4] << 32) +
                            (this.buffer[this.location + 3] << 24) +
                            (this.buffer[this.location + 2] << 16) +
                            (this.buffer[this.location + 1] << 8) +
                            this.buffer[this.location]
                        ), T);

                    this.location += 8;
                    return ret;
                }
            }

            else if (T == typeof(String))
            {
                int l = 0;

                for (int i = this.location; i < this.length; i++)
                    if (this.buffer[i] != 0)
                        l++;
                    else break;

                R ret = (R)Convert.ChangeType(System.Text.Encoding.UTF8.GetString(this.buffer, this.location, l), T);

                this.location += l + 1;
                return ret;
            }

            else throw new NotSupportedException("Unsupported type for reading: " + T.Name);

            return default(R);
        }

        internal void Skip(Int32 offset)
        {
            this.location += offset;
        }

        internal bool CheckBounds(Int32 offset, String type_name)
        {
            if (this.location + offset <= this.buffer.Length)
                return true;
            else
                throw new IndexOutOfRangeException("Data exceeded bounds. Type: " + type_name);
        }
    }

    public struct ServerInfo
    {
        public string Name;
        public string GameType;
        public string GameID;
        public string Plugins;
        public string Map;
        public string HostIP;
        public string HostPort;
        public string Software;
        public string Version;
        public int OnlinePlayers;
        public int MaxPlayers;
        public int Latency;
        public double Protocol;

        public System.Collections.Generic.List<string> Players;
    }

    public interface ServerQuery
    {
        void Connect(string host, int port = 25565, double timeout = 2.5);
        bool Success();
        ServerInfo Info();
    }

    internal class Packets
    {
        internal static byte[] Challenge(int SessionID)
        {
            Packet p = new Packet();
            p.Write((byte)0xFE);
            p.Write((byte)0xFD);
            p.Write((byte)0x09);
            p.Write(SessionID);
            return p.Finalize();
        }

        internal static byte[] QueryInfo(int SessionID, int ChallengeToken)
        {
            Packet p = new Packet();
            p.Write((byte)0xFE);
            p.Write((byte)0xFD);
            p.Write((byte)0x00);
            p.Write(SessionID);
            p.Write(ChallengeToken);
            return p.Finalize();
        }

        internal static byte[] QueryData(int SessionID, int ChallengeToken)
        {
            Packet p = new Packet();
            p.Write((byte)0xFE);
            p.Write((byte)0xFD);
            p.Write((byte)0x00);
            p.Write(SessionID);
            p.Write(ChallengeToken);
            p.Write((int)0);
            return p.Finalize();
        }
    }
}
