namespace PowerLinesAnalysisService.Models;

public class LastResultDate(DateTime? date)
{
    public bool Available { get; set; } = date != null;

    public DateTime? Date { get; set; } = date;
}
