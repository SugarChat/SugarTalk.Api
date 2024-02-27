create table if not exists `meeting_speech_voice_table`
(
    `id` int primary key auto_increment,
    `meeting_speech_id` varchar(36) not null,
    `voice_id` varchar(48) not null,
    `language_id` varchar(48) not null,
    `translate_text` text not null,
    `voice_url` varchar(256) not null,
    `status` int default 10 not null,
    `created_date` datetime(3) not null 
)charset=utf8mb4;