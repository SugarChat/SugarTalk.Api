create table if not exists `meeting_user_setting`
(
    `id` varchar(36) not null primary key,
    `user_id` int not null,
    `target_language_type` int not null,
    `listened_language_type` int not null,
    `last_modified_date` datetime(3) not null
    )
    charset=utf8mb4;