CREATE TABLE IF NOT EXISTS `bot_data`.`message_count`
(
    `UserId`       bigint unsigned    NOT NULL,
    `ChannelId`    bigint unsigned    NOT NULL,
    `Date`         date               NOT NULL,
    `MessageCount` mediumint unsigned NOT NULL,
    PRIMARY KEY (`UserId`, `ChannelId`, `Date`)
)
