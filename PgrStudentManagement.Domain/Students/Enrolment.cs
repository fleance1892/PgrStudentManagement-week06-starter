namespace PgrStudentManagement.Domain.Students;

public class Enrolment
{
    public int EnrolmentId { get; init; }

    public string StudentNumber { get; init; } = string.Empty;

    public string ProgrammeCode { get; init; } = string.Empty;

    public DateTime StartDate { get; init; }

    public DateTime? ExpectedEndDate { get; init; }

    public string ModeOfStudy { get; init; } = string.Empty;

    public EnrolmentStatus Status { get; init; }
}