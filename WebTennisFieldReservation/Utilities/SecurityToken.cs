using System.Runtime.Serialization;

namespace WebTennisFieldReservation.Utilities
{
    public class SecurityToken
    {
        public Guid UserId { get; }
        public Guid SecurityStamp { get; }
        public DateTimeOffset IssueTime { get; }        

        public SecurityToken(Guid userId, Guid securityStamp, DateTimeOffset issueTime)
        {
            UserId = userId;
            SecurityStamp = securityStamp;
            IssueTime = issueTime;
        }    
        
        public byte[] SerializeToBytes()
        {
            using(MemoryStream ms = new MemoryStream())
            {
                using(BinaryWriter bw = new BinaryWriter(ms))
                {   
                    bw.Write(UserId.ToByteArray());
                    bw.Write(SecurityStamp.ToByteArray());
                    bw.Write(IssueTime.ToString());

                    return ms.ToArray();
                }
            }
        }
        
        public static SecurityToken DeserializeFromBytes(byte[] source)
        {
            using (MemoryStream ms = new MemoryStream(source))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    byte[] userIdAsBytes = br.ReadBytes(16);
                    byte[] secStampAsBytes = br.ReadBytes(16);
                    string issueTimeAsString = br.ReadString();

                    return new SecurityToken(new Guid(userIdAsBytes), new Guid(secStampAsBytes), DateTimeOffset.Parse(issueTimeAsString));
                }
            }
        }
    }
}
