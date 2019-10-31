var config = require('./config.json');

// Stores all users currently on message cooldown
var usersOnCd = [];

types = {
	WORD: 0,
	NUMBER: 1,
	LINK: 2
};

exports.types = types;

//#region input validation
function isANumber(argString) {

	return validator.isInt(argString);
}

function isAValidWord(argString) {

	return /[a-zA-Z0-9_]/.test(argString);
}

function isALink(argString) {
	return validator.isURL(argString);
}

exports.validateInput = function(args, argTypes) {
    
	if(args.length != argTypes.length) return false;

	for(var i = 0; i < args.length; i++) {

		if(argTypes[i] == types.WORD)
			if(!isAValidWord(args[i])) return false;

		else if (argTypes[i] == types.NUMBER)
			if(!isANumber(args[i])) return false;

		else if(argTypes[i] == types.LINK)
			if(!isALink(args[i])) return false;
	}
	
	return true;
}
//#endregion

//#region message pass
function addUserCooldown(author) {

	usersOnCd.push(author);

	//avoid possible thread problems
	let authorCopy = author;

	//Wait for an hour, then call this method again to keep an update cycle going
	var interval = setTimeout(() => {    

		 let index = usersOnCd.indexOf(authorCopy);

		 //Delete the user from Cd
		 if(index > -1){
			usersOnCd.splice(index, 1);
		 }

	}, config.msg_cooldown * 1000);
}

exports.messagePassedFilters = function(message) {
     
    // Check for prefix and bot
    if(!message.content.startsWith('!') || message.author.bot) return false;

    // Check for appropriate channels
    if(!config.channels.includes(message.channel.id)) return false;
    
	// Filter @everyone and @here
	if(message.content.includes("@everyone") || message.content.includes("@here")) {
		message.channel.send('Trying to break the bot can result in a ban.');
		return false;
	}
	else if(usersOnCd.includes(message.author)) {
		message.author.send('To prevent spam you can only enter commands every ' + config.msg_cooldown + ' seconds.');
		return false;
	}
	
	let args = message.content.substring(1).split(' ');
	let cmd = args[0];

	// Filter ! and !!! messages
    if(cmd == "" || cmd.replace(/!/g, '') == "") return false;
    
    addUserCooldown(message.author);

	return true;
}

//#endregion