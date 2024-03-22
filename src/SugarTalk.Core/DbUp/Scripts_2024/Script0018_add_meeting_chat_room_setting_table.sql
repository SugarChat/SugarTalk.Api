CREATE TABLE IF NOT EXISTS `meeting_chat_room_setting` (
    `id` INT NOT NULL AUTO_INCREMENT,
    `meeting_id` VARCHAR(36) NOT NULL,
    `user_id` INT NOT NULL,
    `voice_id` varchar(48) null,
    `listening_language` int not null,
    `self_language` int not null,
    `last_modified_date` DATETIME(3) NOT NULL,
    PRIMARY KEY (`id`)
) charset=utf8mb4;