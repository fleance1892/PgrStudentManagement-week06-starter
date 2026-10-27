namespace PgrStudentManagement.Business.Students.ViewStudentEnrolments;

public record StudentEnrolmentsOutput(
    string StudentNumber,
    IReadOnlyList<EnrolmentOutput> Enrolments);