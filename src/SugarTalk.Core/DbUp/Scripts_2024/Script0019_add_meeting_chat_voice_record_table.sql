create table if not exists `meeting_chat_voice_record`(
    id varchar(36) not null primary key,
    voice_language int not null,
    created_date datetime(3) not null,
    speech_id varchar(36) not null,
    voice_id varchar(36) not null,
    is_system_voice tinyint(1) not null default 0,
    voice_url varchar(512) null,
    is_self tinyint(1) not null default 0,
    generation_status int not null default 0
) charset=utf8mb4;

ALTER TABLE meeting_chat_room_setting ADD `voice_name` varchar(128) null;