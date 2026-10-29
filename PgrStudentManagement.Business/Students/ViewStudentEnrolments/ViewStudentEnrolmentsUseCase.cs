using PgrStudentManagement.Business.Students.Gateway;

namespace PgrStudentManagement.Business.Students.ViewStudentEnrolments;

public class ViewStudentEnrolmentsUseCase : IViewStudentEnrolmentsUseCase
{
    private readonly IEnrolmentGateway _enrolmentGateway;

    public ViewStudentEnrolmentsUseCase(IEnrolmentGateway enrolmentGateway)
    {
        _enrolmentGateway = enrolmentGateway;
    }

    public async Task<StudentEnrolmentsOutput> ExecuteAsync(
        ViewStudentEnrolmentsInput input,
        CancellationToken cancellationToken)
    {
        var enrolments = await _enrolmentGateway.GetForStudentAsync(input.StudentNumber, cancellationToken);

        var output = enrolments
            .OrderBy(enrolment => enrolment.StartDate)
            .Select(enrolment => new EnrolmentOutput(
                enrolment.ProgrammeCode,
                enrolment.StartDate,
                enrolment.ExpectedEndDate,
                enrolment.ModeOfStudy,
                enrolment.Status.ToString()))
            .ToList();

        return new StudentEnrolmentsOutput(input.StudentNumber, output);
    }
}
