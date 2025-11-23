using System;
using System.Linq;
using System.Threading.Tasks;
using global::Pcf.GivingToCustomer.Core.Domain;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.gRPC;

namespace Pcf.GivingToCustomer.WebHost.Services;

public class CustomersGrpcService : GivingToCustomersGrpc.GivingToCustomersGrpcBase
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Preference> _preferenceRepository;

    public CustomersGrpcService(
        IRepository<Customer> customerRepository,
        IRepository<Preference> preferenceRepository)
    {
        _customerRepository = customerRepository;
        _preferenceRepository = preferenceRepository;
    }

    public override async Task<CustomersListResponse> GetAllCustomers(Empty request,ServerCallContext context)
    { 
        var customers = await _customerRepository.GetAllAsync();

        var response = new CustomersListResponse();
        response.Customers.AddRange(customers.Select(x => new CustomerShortResponse
        {
            Id = x.Id.ToString(),
            Email = x.Email,
            FirstName = x.FirstName,
            LastName = x.LastName
        }));

        return response;
    }

    public override async Task<CustomerResponse> GetCustomerById(GetCustomerRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var customerId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid customer ID format"));
        }

        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Customer with ID {request.Id} not found"));
        }

        return MapToCustomerResponse(customer);
    }

    public override async Task<CustomerResponse> CreateCustomer(CreateCustomerRequest request, ServerCallContext context)
    {
        var preferencesIds = request.PreferenceIds.Select(Guid.Parse).ToList();
        var preferences = await _preferenceRepository.GetRangeByIdsAsync(preferencesIds);
        var id = Guid.NewGuid();
        var customer = new Customer
        {
            Id = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Preferences = preferences.Select(x => new CustomerPreference()
            {
                CustomerId = id,
                Preference = x,
                PreferenceId = x.Id
            }).ToList()
        };

        await _customerRepository.AddAsync(customer);

        return MapToCustomerResponse(customer);
    }

    public override async Task<CustomerResponse> UpdateCustomer(UpdateCustomerRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var customerId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid customer ID format"));
        }

        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Customer with ID {request.Id} not found"));
        }

        var preferenceIds = request.PreferenceIds.Select(Guid.Parse).ToList();
        var preferences = await _preferenceRepository.GetRangeByIdsAsync(preferenceIds);

        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.Preferences = preferences.Select(x => new CustomerPreference()
        {
            CustomerId = customer.Id,
            Preference = x,
            PreferenceId = x.Id
        }).ToList();

        await _customerRepository.UpdateAsync(customer);

        return MapToCustomerResponse(customer);
    }

    public override async Task<DeleteCustomerResponse> DeleteCustomer(DeleteCustomerRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var customerId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid customer ID format"));
        }

        var customer = await _customerRepository.GetByIdAsync(customerId);
        if (customer == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Customer with ID {request.Id} not found"));
        }

        await _customerRepository.DeleteAsync(customer);

        return new DeleteCustomerResponse { Success = true };
    }

    private static CustomerResponse MapToCustomerResponse(Customer customer)
    {
        var response = new CustomerResponse
        {
            Id = customer.Id.ToString(),
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email
        };

        response.Preferences.AddRange(customer.Preferences.Select(p => new PreferenceResponse
        {
            Id = p.Preference.Id.ToString(),
            Name = p.Preference.Name
        }));

        return response;
    }
}
