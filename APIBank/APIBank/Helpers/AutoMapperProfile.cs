using APIBank.Models.MyRequestResponses;

namespace APIBank.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User -> LoginResponse
            //CreateMap<User, LoginResponse>();

            // CreateUserRequest -> User
            CreateMap<UserCreateRequest, User>();

            // UpdateRequest -> User
            CreateMap<UserUpdateRequest, User>().ForAllMembers(x => x.Condition((src, dest, prop) =>
                    {
                        // ignore null & empty string properties
                        if (prop == null)
                        {
                            return false;
                        }
                        if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop))
                        {
                            return false;
                        }
                        return true;
                    }
                ));

            // CreateAccountRequest -> Account
            CreateMap<AccountCreateRequest, Account>();
        }
    }
}