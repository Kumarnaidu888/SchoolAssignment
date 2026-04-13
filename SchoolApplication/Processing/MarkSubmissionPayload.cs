using System.Text.Json.Serialization;

namespace SchoolApplication.Processing;

/// <summary>Serialized into <see cref="Models.MarkProcessingJob.PayloadJson"/>.</summary>
public sealed class MarkSubmissionPayload
{
    public int ExamId { get; set; }
    public int SubmittedByUserId { get; set; }

    [JsonPropertyName("marks")]
    public List<MarkLinePayload> Marks { get; set; } = [];

    public sealed class MarkLinePayload
    {
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public decimal? Score { get; set; }
    }
}
