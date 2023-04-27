namespace WebTennisFieldReservation.Models.Administration
{
    public class TemplateRowModel
    {
        public string WeekDay { get; }
        public int DayPeriod { get; }

        public TemplateRowModel(string weekDay, int dayPeriod)
        {
            WeekDay = weekDay;
            DayPeriod = dayPeriod;
        }
    }
}
