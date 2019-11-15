using LiteNetLib.Utils;
using NetSyncLib;
using NetSyncLib.Helper;
using NetSyncLib.Helper.ValueManager;

namespace NetSyncLibForLiteNetLib
{
    public static class BasicValueHandler
    {
        [NetValueHandler]
        private static void InitBasicHandlers()
        {
            void add<TValueType>(ValueSender<TValueType> sender, ValueReceiver<TValueType> receiver)
            {
                NetPacketHelperTypes.AddValueHandler<TValueType>(new NetPacketHelperTypes.NetValueHandler<TValueType>(sender, receiver));
            }

            add(INetObjectReferenceSender, INetObjectReferenceReceiver);
            add(StringSender, StringReceiver);
            add(StringArraySender, StringArrayReceiver);
            add(FloatSender, FloatReceiver);
            add(FloatArraySender, FloatArrayReceiver);
            add(DoubleSender, DoubleReceiver);
            add(DoubleArraySender, DoubleArrayReceiver);
            add(BoolSender, BoolReceiver);
            add(BoolArraySender, BoolArrayReceiver);
            add(ByteSender, ByteReceiver);
            add(ByteArraySender, ByteArrayReceiver);
            add(SByteSender, SByteReceiver);
            add(ShortSender, ShortReceiver);
            add(ShortArraySender, ShortArrayReceiver);
            add(UShortSender, UShortReceiver);
            add(UShortArraySender, UShortArrayReceiver);
            add(IntSender, IntReceiver);
            add(IntArraySender, IntArrayReceiver);
            add(UIntSender, UIntReceiver);
            add(UIntArraySender, UIntArrayReceiver);
            add(LongSender, LongReceiver);
            add(LongArraySender, LongArrayReceiver);
            add(ULongSender, ULongReceiver);
            add(ULongArraySender, ULongArrayReceiver);
            add(CharSender, CharReceiver);
        }

        public static void INetObjectReferenceSender(INetObject obj, NetDataWriter writer)
        {
            NetOrganisator.ServerNetObjectHandler.TryGetId(obj, out ushort id);
            writer.Put(id);
        }

        public static INetObject INetObjectReferenceReceiver(NetDataReader reader)
        {
            ushort id = reader.GetUShort();
            if (id == 0) return null;
            return NetOrganisator.ClientNetObjectHandler.NetObjects[id];
        }

        public static void StringSender(string obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static string StringReceiver(NetDataReader reader)
        {
            return reader.GetString();
        }

        public static void StringArraySender(string[] obj, NetDataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static string[] StringArrayReceiver(NetDataReader reader)
        {
            return reader.GetStringArray();
        }

        public static void FloatSender(float obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static float FloatReceiver(NetDataReader reader)
        {
            return reader.GetFloat();
        }

        public static void FloatArraySender(float[] obj, NetDataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static float[] FloatArrayReceiver(NetDataReader reader)
        {
            return reader.GetFloatArray();
        }

        public static void DoubleSender(double obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static double DoubleReceiver(NetDataReader reader)
        {
            return reader.GetDouble();
        }

        public static void DoubleArraySender(double[] obj, NetDataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static double[] DoubleArrayReceiver(NetDataReader reader)
        {
            return reader.GetDoubleArray();
        }

        public static void BoolSender(bool obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static bool BoolReceiver(NetDataReader reader)
        {
            return reader.GetBool();
        }

        public static void BoolArraySender(bool[] obj, NetDataWriter writer)
        {
            // TODO make bitmask out of bool array
            writer.PutArray(obj);
        }

        public static bool[] BoolArrayReceiver(NetDataReader reader)
        {
            return reader.GetBoolArray();
        }

        public static void ByteSender(byte obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static byte ByteReceiver(NetDataReader reader)
        {
            return reader.GetByte();
        }

        public static void ByteArraySender(byte[] obj, NetDataWriter writer)
        {
            // TODO check if this is working
            writer.Put(obj);
        }

        public static byte[] ByteArrayReceiver(NetDataReader reader)
        {
            // TODO check if this is working
            return reader.GetBytesWithLength();
        }

        public static void SByteSender(sbyte obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static sbyte SByteReceiver(NetDataReader reader)
        {
            return reader.GetSByte();
        }

        // TODO SByteArray sender
        public static void ShortSender(short obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static short ShortReceiver(NetDataReader reader)
        {
            return reader.GetShort();
        }

        public static void ShortArraySender(short[] obj, NetDataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static short[] ShortArrayReceiver(NetDataReader reader)
        {
            return reader.GetShortArray();
        }

        public static void UShortSender(ushort obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static ushort UShortReceiver(NetDataReader reader)
        {
            return reader.GetUShort();
        }

        public static void UShortArraySender(ushort[] obj, NetDataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static ushort[] UShortArrayReceiver(NetDataReader reader)
        {
            return reader.GetUShortArray();
        }

        public static void IntSender(int obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static int IntReceiver(NetDataReader reader)
        {
            return reader.GetInt();
        }

        public static void IntArraySender(int[] obj, NetDataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static int[] IntArrayReceiver(NetDataReader reader)
        {
            return reader.GetIntArray();
        }

        public static void UIntSender(uint obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static uint UIntReceiver(NetDataReader reader)
        {
            return reader.GetUInt();
        }

        public static void UIntArraySender(uint[] obj, NetDataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static uint[] UIntArrayReceiver(NetDataReader reader)
        {
            return reader.GetUIntArray();
        }

        public static void LongSender(long obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static long LongReceiver(NetDataReader reader)
        {
            return reader.GetLong();
        }

        public static void LongArraySender(long[] obj, NetDataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static long[] LongArrayReceiver(NetDataReader reader)
        {
            return reader.GetLongArray();
        }

        public static void ULongSender(ulong obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static ulong ULongReceiver(NetDataReader reader)
        {
            return reader.GetULong();
        }

        public static void ULongArraySender(ulong[] obj, NetDataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static ulong[] ULongArrayReceiver(NetDataReader reader)
        {
            return reader.GetULongArray();
        }

        public static void CharSender(char obj, NetDataWriter writer)
        {
            writer.Put(obj);
        }

        public static char CharReceiver(NetDataReader reader)
        {
            return reader.GetChar();
        }
    }
}