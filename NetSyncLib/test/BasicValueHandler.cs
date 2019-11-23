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

        public static void INetObjectReferenceSender(INetObject obj, DataWriter writer)
        {
            NetOrganisator.ServerNetObjectHandler.TryGetId(obj, out ushort id);
            writer.Put(id);
        }

        public static INetObject INetObjectReferenceReceiver(DataReader reader)
        {
            ushort id = reader.GetUShort();
            if (id == 0) return null;
            return NetOrganisator.ClientNetObjectHandler.NetObjects[id];
        }

        public static void StringSender(string obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static string StringReceiver(DataReader reader)
        {
            return reader.GetString();
        }

        public static void StringArraySender(string[] obj, DataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static string[] StringArrayReceiver(DataReader reader)
        {
            return reader.GetStringArray();
        }

        public static void FloatSender(float obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static float FloatReceiver(DataReader reader)
        {
            return reader.GetFloat();
        }

        public static void FloatArraySender(float[] obj, DataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static float[] FloatArrayReceiver(DataReader reader)
        {
            return reader.GetFloatArray();
        }

        public static void DoubleSender(double obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static double DoubleReceiver(DataReader reader)
        {
            return reader.GetDouble();
        }

        public static void DoubleArraySender(double[] obj, DataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static double[] DoubleArrayReceiver(DataReader reader)
        {
            return reader.GetDoubleArray();
        }

        public static void BoolSender(bool obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static bool BoolReceiver(DataReader reader)
        {
            return reader.GetBool();
        }

        public static void BoolArraySender(bool[] obj, DataWriter writer)
        {
            // TODO make bitmask out of bool array
            writer.PutArray(obj);
        }

        public static bool[] BoolArrayReceiver(DataReader reader)
        {
            return reader.GetBoolArray();
        }

        public static void ByteSender(byte obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static byte ByteReceiver(DataReader reader)
        {
            return reader.GetByte();
        }

        public static void ByteArraySender(byte[] obj, DataWriter writer)
        {
            // TODO check if this is working
            writer.Put(obj);
        }

        public static byte[] ByteArrayReceiver(DataReader reader)
        {
            // TODO check if this is working
            return reader.GetBytesWithLength();
        }

        public static void SByteSender(sbyte obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static sbyte SByteReceiver(DataReader reader)
        {
            return reader.GetSByte();
        }

        // TODO SByteArray sender
        public static void ShortSender(short obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static short ShortReceiver(DataReader reader)
        {
            return reader.GetShort();
        }

        public static void ShortArraySender(short[] obj, DataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static short[] ShortArrayReceiver(DataReader reader)
        {
            return reader.GetShortArray();
        }

        public static void UShortSender(ushort obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static ushort UShortReceiver(DataReader reader)
        {
            return reader.GetUShort();
        }

        public static void UShortArraySender(ushort[] obj, DataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static ushort[] UShortArrayReceiver(DataReader reader)
        {
            return reader.GetUShortArray();
        }

        public static void IntSender(int obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static int IntReceiver(DataReader reader)
        {
            return reader.GetInt();
        }

        public static void IntArraySender(int[] obj, DataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static int[] IntArrayReceiver(DataReader reader)
        {
            return reader.GetIntArray();
        }

        public static void UIntSender(uint obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static uint UIntReceiver(DataReader reader)
        {
            return reader.GetUInt();
        }

        public static void UIntArraySender(uint[] obj, DataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static uint[] UIntArrayReceiver(DataReader reader)
        {
            return reader.GetUIntArray();
        }

        public static void LongSender(long obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static long LongReceiver(DataReader reader)
        {
            return reader.GetLong();
        }

        public static void LongArraySender(long[] obj, DataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static long[] LongArrayReceiver(DataReader reader)
        {
            return reader.GetLongArray();
        }

        public static void ULongSender(ulong obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static ulong ULongReceiver(DataReader reader)
        {
            return reader.GetULong();
        }

        public static void ULongArraySender(ulong[] obj, DataWriter writer)
        {
            writer.PutArray(obj);
        }

        public static ulong[] ULongArrayReceiver(DataReader reader)
        {
            return reader.GetULongArray();
        }

        public static void CharSender(char obj, DataWriter writer)
        {
            writer.Put(obj);
        }

        public static char CharReceiver(DataReader reader)
        {
            return reader.GetChar();
        }
    }
}