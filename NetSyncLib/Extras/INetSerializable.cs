namespace LiteNetLib.Utils
{
    public interface INetSerializable
    {
        void Serialize(DataWriter writer);
        void Deserialize(DataReader reader);
    }
}
