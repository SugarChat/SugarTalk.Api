create table  if not exists `meeting_situation_day`
(
    `id` int auto_increment primary key,
    `meeting_id` varchar(36) not null,
    `time_period` varchar(50) not null,
    `use_count` int not null,
    `created_date` datetime(3) not null
)charset=utf8mb4;