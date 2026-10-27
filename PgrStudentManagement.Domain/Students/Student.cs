namespace PgrStudentManagement.Domain.Students;

public class Student
{
    public string StudentNumber { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    public string EmailAddress { get; set; } = string.Empty;

    public string? ResearchProgramme { get; set; }

    public ModeOfStudy ModeOfStudy { get; set; }

    public DateTime? StartDate { get; set; }

    public Status Status { get; set; }

    public string? ThesisTitle { get; set; }
    public DateTime? ExpectedSubmissionDate { get; set; }
    public DateTime? OriginalExpectedSubmissionDate { get; set; }
    public DateTime? ActualSubmissionDate { get; set; }
}