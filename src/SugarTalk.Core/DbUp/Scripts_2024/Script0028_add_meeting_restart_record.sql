create table if not exists `meeting_restart_record`
(
    `id` varchar(36) not null primary key,
    `meeting_id` varchar(36) not null,
    `record_id` varchar(36) not null,
    `url` varchar(512),
    `created_date` datetime(3) not null
)charset=utf8mb4