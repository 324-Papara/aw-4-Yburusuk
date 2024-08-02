using AutoMapper;
using Hangfire;
using MediatR;
using Para.Base.Response;
using Para.Bussiness.Cqrs;
using Para.Bussiness.Notification;
using Para.Bussiness.RabbitMQ.Email;
using Para.Data.Domain;
using Para.Data.UnitOfWork;
using Para.Schema;

namespace Para.Bussiness.Command;

public class AccountCommandHandler :
    IRequestHandler<CreateAccountCommand, ApiResponse<AccountResponse>>,
    IRequestHandler<UpdateAccountCommand, ApiResponse>,
    IRequestHandler<DeleteAccountCommand, ApiResponse>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;
    private readonly INotificationService notificationService;
    private readonly EmailProducer emailProducer;

    public AccountCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,INotificationService notificationService, EmailProducer emailProducer)
    {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
        this.notificationService = notificationService;
        this.emailProducer = emailProducer;
    }

    public async Task<ApiResponse<AccountResponse>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var mapped = mapper.Map<AccountRequest, Account>(request.Request);
        mapped.OpenDate = DateTime.Now;
        mapped.Balance = 0;
        mapped.AccountNumber = new Random().Next(1000000, 9999999);
        mapped.IBAN = $"TR{mapped.AccountNumber}97925786{mapped.AccountNumber}01";
        var saved = await unitOfWork.AccountRepository.Insert(mapped);
        await unitOfWork.Complete();

        var customer = await unitOfWork.CustomerRepository.GetById(request.Request.CustomerId);
        
        emailProducer.QueueEmail(
            subject: "Welcome to our Service!",
            email: customer.Email,
            content: $"Hello {customer.FirstName} {customer.LastName}, your account with {request.Request.CurrencyCode} has been created."
        );
        
        var response = mapper.Map<AccountResponse>(saved);
        return new ApiResponse<AccountResponse>(response);
    }
    

    [AutomaticRetryAttribute(Attempts = 3,DelaysInSeconds = new []{10,15,18 },OnAttemptsExceeded = AttemptsExceededAction.Fail)]
     public void SendEmail(string email,string name,string currencyCode)
    {
        notificationService.SendEmail("Yeni hesap acilisi",email,$"Merhaba, {name}, Adiniza ${currencyCode} doviz cinsi hesabiniz acilmistir.");
    }

    public async Task<ApiResponse> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var mapped = mapper.Map<AccountRequest, Account>(request.Request);
        mapped.Id = request.AccountId;
        unitOfWork.AccountRepository.Update(mapped);
        await unitOfWork.Complete();
        return new ApiResponse();
    }

    public async Task<ApiResponse> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.AccountRepository.Delete(request.AccountId);
        await unitOfWork.Complete();
        return new ApiResponse();
    }
}