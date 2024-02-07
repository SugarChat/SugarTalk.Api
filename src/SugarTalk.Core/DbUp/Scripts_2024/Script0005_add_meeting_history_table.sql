create table if not exists `meeting_history`
(
    `id` varchar(36) not null primary key,
    `meeting_id` varchar(36) not null,
    `user_entity_id` varchar(36) not null,
    `creator_join_time` bigint not null,
    `duration` bigint not null,
    `is_deleted` tinyint(1) not null,
    `created_date` datetime(3) null
)charset=utf8mb4;

alter table `meeting` add  `creator_join_time` bigint null;