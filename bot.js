//#region Requirements
var Discord = require('discord.js');
var validator = require('validator');
var sanitizeHTML = require('sanitize-html');

var config = require('./config.json');
const sandbox = require('./sandbox.js');

const types = sandbox.types;
//#endregion

var momGuild;
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
    
});

bot.login(config.token);
