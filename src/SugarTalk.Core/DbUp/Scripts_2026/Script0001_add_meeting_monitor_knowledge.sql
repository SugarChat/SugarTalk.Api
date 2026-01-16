create table if not exists `meeting_monitor_knowledge`
(
    `id` int auto_increment primary key,
    `prompt` text NULL,
    `created_date` datetime(3) not null
    )charset=utf8mb4;