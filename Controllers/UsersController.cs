using Microsoft.AspNetCore.Mvc;
using LabControlApi.DTOs;
using LabControlApi.Services.Interfaces;

namespace LabControlApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsersController : ControllerBase
	{
		private readonly IUserService _service;

		public UsersController(IUserService service)
		{
			_service = service;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
		{
			var users = await _service.GetAllAsync();
			return Ok(users);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
		{
			var user = await _service.GetByIdAsync(id);
			if (user == null) return NotFound();
			return Ok(user);
		}

		[HttpPost]
		public async Task<ActionResult<UserResponseDto>> Create([FromBody] CreateUserDto dto)
		{
			var created = await _service.CreateAsync(dto);
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<UserResponseDto>> Update(Guid id, [FromBody] UpdateUserDto dto)
		{
			var updated = await _service.UpdateAsync(id, dto);
			if (updated == null) return NotFound();
			return Ok(updated);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			var deleted = await _service.DeleteAsync(id);
			if (!deleted) return NotFound();
			return NoContent();
		}
	}
}
