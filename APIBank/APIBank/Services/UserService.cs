namespace APIBank.Services
{
    public class UserService : IUserService
    {
        private postgresContext _context;
        private IJwtUtils _jwtUtils;
        private readonly IMapper _mapper;

        public UserService(postgresContext context, IJwtUtils jwtUtils,
            IMapper mapper)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _mapper = mapper;
        }

        // helper methods

        private User getUser(int id)
        {
            if (_context.Users == null)
            {
                throw new KeyNotFoundException("Table 'Users' not found");
            }
            var user = _context.Users.Find(id);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }
            return user;
        }

        public LoginResponse Login(LoginRequest model)
        {
            var user = _context.Users.SingleOrDefault(x => x.Username == model.Username);

            // validate
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password_hash))
                throw new AppException("Username or password is incorrect");

            // authentication successful
            var response = _mapper.Map<LoginResponse>(user);
            response.Token = _jwtUtils.GenerateToken(user);
            return response;
        }

        public void Create(CreateUserRequest model)
        {
            if (_context.Users == null)
            {
                throw new KeyNotFoundException("Table 'Users' not found");
            }
            // validate
            if (_context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken");

            // map model to new user object
            var user = _mapper.Map<User>(model);

            // hash password
            user.Password_hash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // save user
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = getUser(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public IEnumerable<User> GetAll()
        {
            if (_context.Users == null)
            {
                throw new KeyNotFoundException("Table 'Users' not found");
            }
            return _context.Users;
        }

        public User GetById(int id)
        {
            return getUser(id);
        }

        public void Update(int id, UpdateRequest model)
        {
            User user = getUser(id);

            // validate
            if (model.Username != user.Username && _context.Users.Any(x => x.Username == model.Username))
                throw new AppException("Username '" + model.Username + "' is already taken");

            // hash password if it was entered
            if (!string.IsNullOrEmpty(model.Password))
                user.Password_hash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // copy model to user and save
            _mapper.Map(model, user);
            _context.Users.Update(user);
            _context.SaveChanges();
        }
    }
}
