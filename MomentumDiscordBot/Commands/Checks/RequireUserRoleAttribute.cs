﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using MomentumDiscordBot.Models;

namespace MomentumDiscordBot.Commands.Checks
{
    public abstract class RequireUserRoleAttribute : DescriptiveCheckBaseAttribute
    {
        protected Func<Configuration, ulong> RoleIdSelector;

        public override Task<bool> ExecuteChecksAsync(InteractionContext context)
        {
            var config = context.Services.GetRequiredService<Configuration>();
            return Task.FromResult(RequireRole(context.User, RoleIdSelector(config)));
        }

        private static bool RequireRole(DiscordUser user, ulong roleId)
        {
            // Check if this user is a Guild User, which is the only context where roles exist
            if (!(user is DiscordMember member))
            {
                return false;
            }

            return member.Roles.Any(role => role.Id == roleId);
        }
    }
}