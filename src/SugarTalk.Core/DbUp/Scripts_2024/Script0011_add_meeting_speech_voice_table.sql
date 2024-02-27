create table if not exists `meeting_speech_voice_table`
(
    `id` int primary key auto_increment,
    `meeting_speech_id` varchar(36) not null,
    `voice_id` varchar(48) not null,
    `language_id` varchar(48) not null,
    `translate_text` text not null,
    `voice_url` varchar(64) not null,
    `status` int not null,
    `create_date` datetime
)charset=utf8mb4;