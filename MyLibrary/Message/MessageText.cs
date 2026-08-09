using MessagePack;

namespace Shared.Messages
{



    [MessagePackObject]
    public class MessageText
    {
        [Key(0)]
        public int Id;

        [Key(1)]
        public string Text = "";
    }
}