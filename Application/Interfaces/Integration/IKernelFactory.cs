using Microsoft.SemanticKernel;

namespace Application.Interfaces.Integration
{
    public interface IKernelFactory
    {
        Kernel CreateCustomerKernel();
        Kernel CreateAdminKernel();
    }
}
