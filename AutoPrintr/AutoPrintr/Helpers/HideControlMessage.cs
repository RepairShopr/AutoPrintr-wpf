namespace AutoPrintr.Helpers
{
    internal class HideControlMessage
    {
        public ControlMessageType Type { get; set; }

        public HideControlMessage(ControlMessageType type)
        {
            Type = type;
        }
    }
}