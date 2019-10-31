//#region Requirements
var Discord = require('discord.js');
var validator = require('validator');
var sanitizeHTML = require('sanitize-html');

const sandbox = require('./sandbox.js');
const types = sandbox.types;
//#endregion

//#region init
var bot = new Discord.Client({
	token: config.token,
	autorun: true
});

bot.on('ready', function() {
	console.log('Connected');
});
//#endregion

bot.on('message', function(message) {

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
