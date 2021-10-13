using Memory.Utils;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Memory.Win64
{
    class MemoryHelper64
    {
        Process process;

        public MemoryHelper64()
        {
            process = null;
        }
        public MemoryHelper64(Process TargetProcess)
        {
            SetProcess(TargetProcess);
        }
        public void SetProcess(Process TargetProcess)
        {
            process = TargetProcess;
        }
        public Process GetProcess()
        {
            return process;
        }
        public bool IsRunning()
        {
            if (process == null) return false;
            return !process.HasExited;
        }
        // Returns Base Address of the Attached process. 0 on Error
        public ulong GetBaseAddress()
        {
            if (process == null) return 0;
            return (ulong)process.MainModule.BaseAddress.ToInt64();
        }
        // Reads bunch of offsets to return an address. Returns 0 on Error
        public ulong OffsetCalculator(ulong[] offsets)
        {
            ulong addr = GetBaseAddress();
            if (addr == 0) return 0;
            int size = offsets.GetLength(0);
            for (int i = 0; i < size; i++)
            {
                addr = this.ReadMemory<ulong>(addr + offsets[i]);
                if (addr == 0) return 0;
            }
            return addr;
        }

        public byte[] ReadMemoryBytes(ulong MemoryAddress, int Bytes)
        {
            byte[] data = new byte[Bytes];
            if(!ReadProcessMemory(process.Handle, MemoryAddress, data, data.Length, IntPtr.Zero))
            {
                return null;
            }
            return data;
        }

        public T ReadMemory<T>(ulong MemoryAddress)
        {
            byte[] data = ReadMemoryBytes(MemoryAddress, Marshal.SizeOf(typeof(T)));
            if (data == null) return default;
            T t;
            GCHandle PinnedStruct = GCHandle.Alloc(data, GCHandleType.Pinned);
            try { t = (T)Marshal.PtrToStructure(PinnedStruct.AddrOfPinnedObject(), typeof(T)); }
            catch (Exception ex) { throw ex; }
            finally { PinnedStruct.Free(); }

            return t;
        }

        public string ReadMemoryString(ulong MemoryAddress, int NumOfBytes)
        {
            byte[] data = ReadMemoryBytes(MemoryAddress, NumOfBytes);
            if (data == null) return default;
            return Encoding.ASCII.GetString(data);
        }

        public bool WriteMemory<T>(ulong MemoryAddress, T Value)
        {
            int sz = ObjectType.GetSize<T>();
            byte[] data = ObjectType.GetBytes<T>(Value);
            bool result = WriteProcessMemory(process.Handle, MemoryAddress, data, sz, out IntPtr bw);
            return result && bw != IntPtr.Zero;
        }

        public bool WriteBytes(ulong MemoryAddress, byte[] array)
        {
            int sz = array.Length;
            bool result = WriteProcessMemory(process.Handle, MemoryAddress, array, sz, out IntPtr bw);
            return result && bw != IntPtr.Zero;
        }

        public bool WriteString(ulong MemoryAddress, string value)
        {
            byte[] data = Encoding.ASCII.GetBytes(value);
            int sz = data.Length;
            bool result = WriteProcessMemory(process.Handle, MemoryAddress, data, sz, out IntPtr bw);
            return result && bw != IntPtr.Zero;
        }

        public ulong VirtualAllocate(int size = 1024)
        {
            if (size < 0) return 0;
            int power = 1;
            while (power < size)
                power *= 2;
            size = power;
            ulong target_address = this.GetBaseAddress() - 0x10000;
            ulong allocated_addr = 0;
            ulong limit = this.GetBaseAddress() - (0x400 * 0x400 * 0x400);
            // Looking for space to allocate memory within -1GB Memory space
            for (; target_address > limit; target_address -= 0x10000)
            {
                // 0x3000 : MEM_COMMIT | MEM_RESERVE
                // 0x40: PAGE_EXECUTE_READWRITE
                allocated_addr = VirtualAllocEx(this.process.Handle, (target_address), (uint)size, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
                if (allocated_addr != 0)
                {
                    // Writing NOPs
                    for (int i = 0; i < size; i++)
                        this.WriteMemory<byte>(allocated_addr + (ulong)(i), 0x90);
                    break;
                }
            }
            return allocated_addr;
        }

        public bool VirtualFreeMemory(ulong addr)
        {
            return VirtualFreeEx(this.process.Handle, addr, 0, AllocationType.Release);
        }

        public void Close()
        {
            CloseHandle(process.Handle);
        }

        #region PInvoke
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            ulong lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            ulong lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten
            );
        
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern ulong VirtualAllocEx(
            IntPtr hProcess, 
            ulong lpAddress,
            uint dwSize, 
            AllocationType flAllocationType, 
            MemoryProtection flProtect
            );

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(
            IntPtr hProcess, 
            ulong lpAddress,
            int dwSize, 
            AllocationType dwFreeType
            );

        [DllImport("kernel32.dll")]
        private static extern Int32 CloseHandle(IntPtr hProcess);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        #endregion
    }
}

namespace Memory.Utils
{
    public static class ObjectType
    {
        public static int GetSize<T>()
        {
            return Marshal.SizeOf(typeof(T));
        }

        public static byte[] GetBytes<T>(T Value)
        {
            string typename = typeof(T).ToString();
            Console.WriteLine(typename);
            switch (typename)
            {
                case "System.Single":
                    return BitConverter.GetBytes((float)Convert.ChangeType(Value, typeof(float)));
                case "System.Int16":
                    return BitConverter.GetBytes((short)Convert.ChangeType(Value, typeof(short)));
                case "System.Int32":
                    return BitConverter.GetBytes((int)Convert.ChangeType(Value, typeof(int)));
                case "System.Int64":
                    return BitConverter.GetBytes((long)Convert.ChangeType(Value, typeof(long)));
                case "System.UInt64":
                    return BitConverter.GetBytes((ulong)Convert.ChangeType(Value, typeof(ulong)));
                case "System.Double":
                    return BitConverter.GetBytes((double)Convert.ChangeType(Value, typeof(double)));
                case "System.Byte":
                    return BitConverter.GetBytes((byte)Convert.ChangeType(Value, typeof(byte)));
                case "System.String":
                    return Encoding.Unicode.GetBytes((string)Convert.ChangeType(Value, typeof(string)));
                default:
                    return new byte[0];
            }
        }
    }
}