create table if not exists user
(
    `id` varchar(36) not null primary key,
    `third_party_id` varchar(128) not null,
    `third_party_from` int NOT NULL,
    `email` varchar(512) not null,
    `picture` text null,
    `display_name` varchar(128) not null
    )
    charset=utf8mb4;

create table if not exists user_auth_token
(
    `id` varchar(36) not null primary key,
    `access_token` varchar(128) not null,
    `expired_at` datetime(3) not null,
    `third_party_from` int NOT NULL,
    `payload` varchar(128) not null
    )
    charset=utf8mb4;

create table if not exists user_session
(
    `id` varchar(36) not null primary key,
    `created_date` datetime(3) not null,
    `meeting_session_id` varchar(36) not null,
    `connection_id` varchar(128) not null,
    `user_id` varchar(36) not null,
    `username` varchar(128) not null,
    `user_picture` text null,
    `is_muted` tinyint(1) default 0 null,
    `is_sharing_screen` tinyint(1) default 0 null,
    `is_sharing_camera` tinyint(1) default 0 null
    )
    charset=utf8mb4;

create table if not exists meeting
(
    `id` varchar(36) not null primary key,
    `created_date` datetime(3) not null,
    `meeting_number` varchar(256) not null,
    `meeting_type` int NOT NULL
    )
    charset=utf8mb4;

create table if not exists meeting_session
(
    `id` varchar(36) not null primary key,
    `meeting_id` varchar(36) not null,
    `meeting_number` varchar(128) not null,
    `meeting_type` int NOT NULL
    )
    charset=utf8mb4;