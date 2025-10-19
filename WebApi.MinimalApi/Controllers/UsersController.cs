using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebApi.MinimalApi.Domain;
using WebApi.MinimalApi.Models;

namespace WebApi.MinimalApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller
{
    private readonly IUserRepository userRepository;
    private readonly IMapper mapper;
    
    public UsersController(IUserRepository userRepository, IMapper mapper)
    {
        this.userRepository = userRepository;
        this.mapper = mapper;
    }

    [HttpGet("{userId:guid}", Name = nameof(GetUserById))]
    [Produces("application/json", "application/xml")]
    public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
    {
        var user = userRepository.FindById(userId);
        return user is null ? NotFound(user) : Ok(mapper.Map<UserDto>(user));
    }

    [HttpPost]
    [Produces("application/json", "application/xml")]
    public IActionResult CreateUser([FromBody] UserCreateDto? user)
    {
        if (user is null)
            return BadRequest();

        if (string.IsNullOrWhiteSpace(user.Login))
        {
            ModelState.AddModelError("login", "Login is necessary");
            return UnprocessableEntity(ModelState);
        }

        if (!user.Login.All(char.IsLetterOrDigit))
        {
            ModelState.AddModelError("login", "Login is invalid");
            return UnprocessableEntity(ModelState);
        }
        
        var userEntity = mapper.Map<UserEntity>(user);

        var createdUserEntity = userRepository.Insert(userEntity);
        return CreatedAtRoute(
            nameof(GetUserById),
            new { userId = createdUserEntity.Id },
            createdUserEntity.Id);
    }
    
    [HttpPut("{userId}")]
    [Produces("application/json", "application/xml")]
    public IActionResult UpdateUser([FromRoute] string userId, [FromBody] UserUpdateDto user)
    {
        if (!Guid.TryParse(userId, out var id))
            return BadRequest();
        if (user == null)
            return BadRequest();
        if (!TryValidateModel(user))
            return UnprocessableEntity(ModelState);
        if (string.IsNullOrEmpty(user.FirstName))
        {
            ModelState.AddModelError("firstName", "FirstName is required");
            return UnprocessableEntity(ModelState);
        }
        if (string.IsNullOrEmpty(user.LastName))
        {
            ModelState.AddModelError("lastName", "LastName is required");
            return UnprocessableEntity(ModelState);
        }
        var entity = new UserEntity(id)
        {
            Login = user.Login,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
        userRepository.UpdateOrInsert(entity, out var isInserted);
        if (isInserted)
            return CreatedAtRoute(nameof(GetUserById), new { userId = entity.Id }, entity.Id);
        return NoContent();
    }
}