create table if not exists `meeting_speech`
(
    `id` varchar(36) not null primary key,
    `meeting_id` varchar(36) not null,
    `user_id` int not null,
    `original_text` text null,
    `translated_text` text null,
    `created_date` datetime(3) not null,
    `status` int not null
    )
    charset=utf8mb4;