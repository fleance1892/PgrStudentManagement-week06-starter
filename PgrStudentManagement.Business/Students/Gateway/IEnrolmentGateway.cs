using PgrStudentManagement.Domain.Students;namespace PgrStudentManagement.Business.Students.Gateway;
public interface IEnrolmentGateway{
    Task<IReadOnlyList<Enrolment>> GetForStudentAsync(
            string studentNumber,
            CancellationToken cancellationToken);
}