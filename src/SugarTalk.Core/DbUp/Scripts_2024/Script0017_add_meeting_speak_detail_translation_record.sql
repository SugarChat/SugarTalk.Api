create table if not exists `meeting_speak_detail_translation_record`
(
    `id` int primary key auto_increment,
    `meeting_record_id` varchar(36) not null,
    `meeting_speak_detail_id` int not null,
    `language` int not null,
    `status` int not null,
    `original_translation_content` text default null,
    `smart_translation_content` text default null,
    `created_date` datetime(3) not null
)charset=utf8mb4;