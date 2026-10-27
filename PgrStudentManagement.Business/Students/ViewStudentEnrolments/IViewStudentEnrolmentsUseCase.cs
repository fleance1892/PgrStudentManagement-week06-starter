namespace PgrStudentManagement.Business.Students.ViewStudentEnrolments;

public interface IViewStudentEnrolmentsUseCase
{
    Task<StudentEnrolmentsOutput> ExecuteAsync(
        ViewStudentEnrolmentsInput input,
        CancellationToken cancellationToken);
}