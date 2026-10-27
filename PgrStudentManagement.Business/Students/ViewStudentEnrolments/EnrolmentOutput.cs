namespace PgrStudentManagement.Business.Students.ViewStudentEnrolments;

public record EnrolmentOutput(
    string ProgrammeCode,
    DateTime StartDate,
    DateTime? ExpectedEndDate,
    string ModeOfStudy,
    string Status);