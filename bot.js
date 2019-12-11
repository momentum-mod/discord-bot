//#region Requirements
var Discord = require('discord.js');
const { Permissions } = require('discord.js');

var validator = require('validator');
var sanitizeHTML = require('sanitize-html');

var config = require('./config.json');
const sandbox = require('./sandbox.js');

const types = sandbox.types;
//#endregion

var momGuild;

const guildID = "639487053725171713";
const perms = ['ADMINISTRATOR', 'KICK_MEMBERS', 'BAN_MEMBERS', 'MANAGE_CHANNELS', 'MANAGE_GUILD', 'MANAGE_MESSAGES', 'MENTION_EVERYONE',
'MUTE_MEMBERS', 'DEAFEN_MEMBERS', 'MOVE_MEMBERS', 'MANAGE_NICKNAMES', 'MANAGE_ROLES', 'MANAGE_WEBHOOKS', 'MANAGE_EMOJIS'];

var faqRole;
const guildID = "639487053725171713";

//#region init
var bot = new Discord.Client({
	token: config.token,
	autorun: true
});

bot.on('ready', function() {
    console.log('Connected');
    
    momGuild = bot.guilds.get(guildID);

    faqRole = momGuild.roles.find('name', 'faq-role');
});
//#endregion

bot.on('guildMemberAdd', member => {

    member.addRole(faqRole);
});

bot.on('message', async function(message) {

    if(message.channel.id == config.faqChannelID) {
        let guildUser = await momGuild.fetchMember(message.author.id);
        message.delete(1);

        if(guildUser.roles.find(r => r.name == "faq-role")) {

            if(message.content == "accept") {
                guildUser.removeRole(faqRole);
            }
        }
    }

    // Filter message for unwanted input
	if(!sandbox.messagePassedFilters(message)) return;
	
	//#region slice args
	var args = message.content.substring(1).split(' ');
	var cmd = args[0];
	
	// remove cmd from args
	args = args.slice(1);
	//#endregion

	// Sanitize input
    args.forEach(arg => arg = sanitizeHTML(arg));
    
    switch(cmd) {

        case 'addRole':
            
            if(sandbox.validateInput(args, [types.WORD])) {
                handleRoleRequest(message.channel, message.author.id, args[0]);
            }
            else message.channel.send("!addRole <role name>");
            break;

        case 'removeRole':

            if(sandbox.validateInput(args, [types.WORD])) {
                handleRoleRequest(message.channel, message.author.id, args[0], true);
            }
            else message.channel.send("!removeRole <role name>");
            break;

        default:
            break;
    }
});

async function handleRoleRequest(channel, userID, roleStr, remove = false) {
    let guildUser = await momGuild.fetchMember(userID);
    let role = await momGuild.roles.find('name', roleStr);

    if(role) {

        // remove role
        if(remove) {
            if(guildUser.roles.find(val => val === role)) guildUser.removeRole(role);
            else channel.send("You do not have this role.");
        }

        // add role
        else {
            let abuse = false;

            // iterate over all perms
            perms.forEach(perm => {

                if(abuse) return;
                if(role.hasPermission(perm)) {
                    abuse = true;
                }
            });

            // check if abuse is present
            if(abuse) {
                channel.send(config.warningMsg);
                return;
            }

            // if not, add role
            guildUser.addRole(role);
        }
    }

    else
        channel.send(roleStr + " is not a valid role.");
}

bot.login(config.token);
