CREATE TABLE IF NOT EXISTS `meeting_chat_room_setting` (
    `id` INT NOT NULL AUTO_INCREMENT,
    `meeting_id` VARCHAR(36) NOT NULL,
    `user_id` INT NOT NULL,
    `ea_voice_id` VARCHAR(36) NOT NULL,
    `target_language_type` int not null,
    `original_language_type` int not null,
    `voice_type` int not null,
    `last_modified_date` DATETIME(3) NOT NULL,
    PRIMARY KEY (`id`)
) charset=utf8mb4;