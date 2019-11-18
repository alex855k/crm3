using AutoMapper;
using CRM.Application.Core.ViewModels;
using CRM.Models;
using System;

namespace CRM.Application.Core.AutoMapperMappings
{
    public static class AutoMapperConfiguration
    {
        public static void ConfigureMappings()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<CustomerProfile>();
                cfg.AddProfile<AdministrationProfile>();
                cfg.AddProfile<BudgetProfile>();
                cfg.AddProfile<ProcedureProfile>();
                cfg.AddProfile<OrderProfile>();
                cfg.AddProfile<EmailProfile>();
                cfg.AddProfile<DashboardProfile>();
            });
        }
        public class CustomerProfile : Profile
        {
            public CustomerProfile()
            {
                CreateMap<Customer, CustomerViewModel>()
                .ForMember(dest => dest.SalesPersonId, map => map.MapFrom(src => src.SalesPersonId ?? Guid.Empty.ToString()))
                .ForMember(dest => dest.HiddenCustomerRowColor, map => map.MapFrom(src => src.CustomerRowColor));


                CreateMap<CustomerViewModel, Customer>()
                .ForMember(dest => dest.SalesPersonId, map => map.MapFrom(src => src.SalesPersonId == Guid.Empty.ToString() ? null : src.SalesPersonId))
                .ForMember(dest => dest.Phone, map => map.MapFrom(src => src.Phone.Replace(" ", string.Empty)))
                .ForMember(dest => dest.CustomerStatus, opt => opt.Ignore());

                CreateMap<CustomerContact, CustomerContactsViewModel>()
               .ReverseMap();

                CreateMap<CustomerNotesViewModel, CustomerNote>()
               .ForMember(dest => dest.CustomerNoteDemoId, map => map.MapFrom(src => src.CustomerNotesDemoId == 0 ? null : src.CustomerNotesDemoId))
               .ForMember(dest => dest.CustomerNoteReportId, map => map.MapFrom(src => src.CustomerNotesReportId == 0 ? null : src.CustomerNotesReportId))
               .ForMember(dest => dest.CustomerNoteVisitTypeId, map => map.MapFrom(src => src.CustomerNotesVisitTypeId == 0 ? null : src.CustomerNotesVisitTypeId))
               .ForMember(dest => dest.UserId, map => map.MapFrom(src => src.CreatedBy))
               .ForMember(dest => dest.CreationDate, map => map.MapFrom(src => src.Id != 0 ? src.CreationDate : DateTime.Now))
               .ForMember(dest => dest.UpdateNoteUserId, map => map.MapFrom(src => src.Id != 0 ? src.CreatedBy : null))
               .ForMember(dest => dest.UpdateNoteDate, map => map.MapFrom(src => src.Id != 0 ? src.CreationDate : (DateTime?)null))
               .ReverseMap();

                CreateMap<CustomerAppointment, CustomerAppointmentViewModel>()
               .ReverseMap();
            }
        }

        public class OrderProfile : Profile
        {
            public OrderProfile()
            {
                CreateMap<Order, OrderViewModel>()
                    .ForMember(dest => dest.CreationDate, map => map.MapFrom(src => src.CreationDate))

                    .ForMember(dest => dest.DeliveryHouseNr, map => map.MapFrom(src => src.DeliveryAddressHouseNr))
                    .ForMember(dest => dest.DeliveryPostalCode, map => map.MapFrom(src => src.DeliveryAddressPostalCode))
                    .ForMember(dest => dest.DeliveryStreet, map => map.MapFrom(src => src.DeliveryAddressStreet))
                    .ForMember(dest => dest.DeliveryTown, map => map.MapFrom(src => src.DeliveryAddressTown))

                    .ForMember(dest => dest.BillingHouseNr, map => map.MapFrom(src => src.BillingAddressTown))
                    .ForMember(dest => dest.BillingPostalCode, map => map.MapFrom(src => src.BillingAddressPostalCode))
                    .ForMember(dest => dest.BillingStreet, map => map.MapFrom(src => src.BillingAddressStreet))
                    .ForMember(dest => dest.BillingTown, map => map.MapFrom(src => src.BillingAddressTown))

                    .ForMember(dest => dest.OrderItems, map => map.MapFrom(src => src.OrderItems))
                    .ForMember(dest => dest.Status, opt => opt.Ignore())
                    .ForMember(dest => dest.StatusOptions, opt => opt.Ignore())
                    .ForMember(dest => dest.CustomerCompany, map => map.MapFrom(src => src.Customer.CompanyName))
                    .ReverseMap();
            }
        }

        public class AdministrationProfile : Profile
        {
            public AdministrationProfile()
            {
                CreateMap<UserViewModel, User>()
                .BeforeMap((s, d) => d.IsSuperAdmin = false)
                .ForMember(dest => dest.IsSuperAdmin, opt => opt.Ignore())
                .ReverseMap();

                CreateMap<DashboardListsViewModel, DashboardList>()
                .ForMember(dest => dest.DashboardListColumns, map => map.MapFrom(src => src.DashboardListColumns))
                .ReverseMap();

                CreateMap<DashboardListColumnsViewModel, DashboardListColumn>()
                .ReverseMap();

                CreateMap<CustomerDashboardListViewModel, CustomerDashboardList>()
               .ReverseMap();


            }
        }

        public class BudgetProfile : Profile
        {
            public BudgetProfile()
            {
                CreateMap<BudgetViewModel, Budget>()
                    .ForMember(dest => dest.Id, map => map.MapFrom(src => src.Id))
                    .ForMember(dest => dest.BudgetAmount, map => map.MapFrom(src => src.BudgetAmount))
                    .ForMember(dest => dest.BudgetDate, map => map.MapFrom(src => src.BudgetDate))
                    .ForMember(dest => dest.SalesPersonId, map => map.MapFrom(src => src.SalesPersonId))
                    .ForMember(dest => dest.SalesPerson, opt => opt.Ignore())
                    .ForSourceMember(src => src.SalesPersonList, opt => opt.Ignore())
                    .ForSourceMember(src => src.BudgetList, opt => opt.Ignore())
                .ReverseMap();
            }
        }

        public class ProcedureProfile : Profile
        {
            public ProcedureProfile()
            {
                CreateMap<ProcedureViewModel, Procedure>()
                    .ForMember(dest => dest.Created, map => map.MapFrom(src => DateTime.Now))
                .ReverseMap();

                CreateMap<Procedure, ProcedureViewModel>()
                    .ReverseMap();
            }
        }

        public class EmailProfile : Profile
        {
            public EmailProfile()
            {
                CreateMap<EmailAccount, EmailAccountViewModel>()
                .ReverseMap();


                CreateMap<EmailMessage, EmailMessageViewModel>()
                //.ForMember(dest => dest.Recipients, map => map.MapFrom(src => src.Recipients))
                .ReverseMap();

            }
        }

        public class DashboardProfile : Profile
        {
            public DashboardProfile()
            {
                CreateMap<DashboardList, DashboardListsViewModel>();
                CreateMap<DashboardListColumn, DashboardListColumnsViewModel>()
                       .ForMember(dest => dest.CustomerDashboardListViewModel, opt => opt.MapFrom(src => src.CustomerDashboardLists));
               // CreateMap<CustomerDashboardList, CustomerDashboardListViewModel>()
                CreateMap<Customer, CustomerViewModel>();
            }
        }
        public class CaseProfile : Profile
        {

        }
        public class AssignmentProfile : Profile
        {
            public AssignmentProfile()
            {
                CreateMap<CaseAssignment, CaseAssignmentViewModel>()
                .ReverseMap();
            }

        }
    }
}