using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;
using MomentumDiscordBot.Utilities;

namespace MomentumDiscordBot.Commands.Checks
{
    public abstract class RequireUserRoleAttribute : DescriptiveCheckBaseAttribute
    {
        protected Func<Configuration, ulong> RoleIdSelector;

        public override Task<bool> ExecuteChecksAsync(InteractionContext context)
        {
            var config = context.Services.GetRequiredService<Configuration>();
            return Task.FromResult(context.User.RequireRole(RoleIdSelector(config)));
        }
    }
    public abstract class ContextMenuRequireUserRoleAttribute : ContextMenuDescriptiveCheckBaseAttribute
    {
        protected Func<Configuration, ulong> RoleIdSelector;

        public override Task<bool> ExecuteChecksAsync(ContextMenuContext context)
        {
            var config = context.Services.GetRequiredService<Configuration>();
            return Task.FromResult(context.User.RequireRole(RoleIdSelector(config)));
        }
    }
}