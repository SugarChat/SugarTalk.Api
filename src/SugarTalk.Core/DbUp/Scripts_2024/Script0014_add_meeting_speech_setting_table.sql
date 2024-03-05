create table if not exists `meeting_speech_setting`
(
    `id` int primary key auto_increment,
    `voice_name` varchar(50) not null,
    `created_date` datetime(3) not null
)charset=utf8mb4;

create table if not exists `meeting_voice_tone_chart`
(
    `id` int primary key auto_increment,
    `setting_id` int not null,
    `voice_id` varchar(48) not null,
    `language_id` varchar(48) not null,
    `created_date` datetime(3) not null
)charset=utf8mb4;