create table if not exists `meeting_history`
(
    `id` varchar(36) not null primary key,
    `meeting_id` varchar(36) not null,
    `sub_meeting_id` varchar(36) null,
    `user_entity_id` varchar(36) not null,
    `creator_join_time` bigint not null,
    `duration` bigint not null,
    `is_deleted` tinyint(1) not null,
    `created_date` datetime(3) null
) charset=utf8mb4;

CREATE INDEX idx_user_entity_id ON meeting_history (user_entity_id);
CREATE INDEX idx_meeting_id ON meeting_history (meeting_id);
CREATE INDEX idx_sub_meeting_id ON meeting_history (sub_meeting_id);

alter table `meeting` add column `creator_join_time` bigint null;