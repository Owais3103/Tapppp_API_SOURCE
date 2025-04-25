namespace virtual_store.settingclasses
{
    public class Summary
    {
        public PeriodData Daily { get; set; }
        public PeriodData Monthly { get; set; }
        public PeriodData Yearly { get; set; }
    }
    public class PeriodData
    {
        public List<string>? Labels { get; set; }
        public List<int>? Values { get; set; }
        public int Total { get; set; }
        public int previous_online { get; set; }
        public int previous_cod { get; set; }
        public int Online { get; set; }
        public int Cash { get; set; }
    }


}
