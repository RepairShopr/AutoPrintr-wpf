namespace AutoPrintr.Helpers
{
    public class EmailSettings
    {
        public string SupportEmailAddress { get; set; }
        public string SupportEmailSubject { get; set; }

        public EmailSettings()
        {
            SupportEmailAddress = "help@repairshopr.com";
            SupportEmailSubject = "Support request from AutoPrintr";
        }
    }
}