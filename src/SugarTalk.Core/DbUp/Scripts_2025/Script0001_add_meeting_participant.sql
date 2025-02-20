create table if not exists `meeting_participant`
(
    `id` int auto_increment primary key,
    `meeting_id` varchar(36) not null,
    `third_party_user_id` varchar(128) not null,
    `created_date` datetime(3) not null,
)charset=utf8mb4;