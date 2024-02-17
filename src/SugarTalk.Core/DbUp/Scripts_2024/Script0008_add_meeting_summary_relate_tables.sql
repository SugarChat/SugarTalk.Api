create table if not exists `meeting_summary`
(
    `id` int auto_increment,
    `record_id` varchar(36) NOT NULL,
    `meeting_number` varchar(48) NOT NULL,
    `speak_ids` varchar(255) NOT NULL,
    `origin_text` text NOT NULL,
    `summary` text NULL,
    `status` int NOT NULL default 10,
    `target_language` int not null default 0,
    `created_date` datetime NOT NULL,
    PRIMARY KEY (`id`)
) charset=utf8mb4;

ALTER TABLE `meeting_speak_detail` modify `id` int auto_increment not null auto_increment;