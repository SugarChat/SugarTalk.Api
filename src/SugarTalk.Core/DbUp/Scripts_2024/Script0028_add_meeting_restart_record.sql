create table if not exists `meeting_restart_record`
(
    `id` varchar(36) not null primary key,
    `url` varchar(512),
    `meeting_id` varchar(36) not null,
    `record_id` varchar(36) not null,
    `created_date` datetime(3) not null
)charset=utf8mb4