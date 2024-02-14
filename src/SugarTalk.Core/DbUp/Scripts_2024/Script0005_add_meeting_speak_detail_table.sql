create table if not exists `meeting_speak_detail`
(
    `id` varchar(36) not null primary key,
    `meeting_id` varchar(36) not null,
    `meeting_sub_id` varchar(36) null,
    `meeting_record_id` varchar(36) not null,
    `meeting_number` varchar(128) not null,
    `user_id` int not null,
    `speak_start_time` BIGINT not null,
    `speak_end_time` BIGINT null,
    `speak_status` int not null default 10,
    `speak_content` text null,
    `created_date` datetime(3) not null
) charset=utf8mb4;

CREATE INDEX idx_user_id ON meeting_speak_detail (user_id);
CREATE INDEX idx_meeting_id ON meeting_speak_detail (meeting_id);
CREATE INDEX idx_meeting_sub_id ON meeting_speak_detail (meeting_sub_id);
CREATE INDEX idx_meeting_record_id ON meeting_speak_detail (meeting_record_id);