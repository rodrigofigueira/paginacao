using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Paginacao.Data;
using Paginacao.Models;
using Microsoft.EntityFrameworkCore;
using Bogus;
using AutoBogus;

namespace Paginacao.Controllers
{
    [ApiController]
    [Route("v1/users")]
    public class UserController : ControllerBase
    {

        [HttpGet("load")]
        public async Task<IActionResult> LoadAsync([FromServices] AppDbContext context)
        {

            int i = 1;

            var autoFaker = new AutoFaker<User>()
                            .RuleFor(u => u.Id, f => i)
                            .RuleFor(u => u.Name, f => f.Person.FullName)                            
                            .RuleFor(u => u.Birthdate, f => f.Person.DateOfBirth);                            

            for(i = 1; i <= 2000; i++){
                User user = autoFaker.Generate();
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();
            }

            return Ok();

        }

        [HttpGet("skip/{skip:int}/take/{take:int}")]
        public async Task<IActionResult> GetAsync(
            [FromServices] AppDbContext context,
            [FromRoute] int skip = 0,
            [FromRoute] int take = 25
        ){
            var total = await context.Users.CountAsync();

            var users = await context
                        .Users
                        .Skip(skip)
                        .Take(take)
                        .AsNoTracking()
                        .OrderBy(u => u.Id)
                        .ToListAsync();

            return Ok(new{
                total,
                skip,
                take,
                data = users
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromServices] AppDbContext context){
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();
            return NoContent();
        }


    }
}