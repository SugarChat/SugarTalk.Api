create table if not exists user_account
(
    id int auto_increment
    primary key,
    issuer int default 0 not null,
    created_on datetime(3) not null,
    modified_on datetime(3) not null,
    uuid varchar(36) not null,
    username varchar(512) charset utf8 not null,
    password varchar(128) not null,
    third_party_user_id varchar(128) null,
    email varchar(512) null,
    picture varchar(512) null,
    active tinyint(1) default 1 not null,
    constraint idx_username_third_party_user_id
    unique (username, third_party_user_id)
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
    `connection_id` varchar(128) null,
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

create table if not exists role
(
    id int auto_increment
    primary key,
    created_on datetime(3) not null,
    modified_on datetime(3) not null,
    uuid varchar(36) not null,
    name varchar(512) charset utf8 not null,
    constraint idx_name
    unique (name)
    )
    charset=utf8mb4;

create table if not exists role_user
(
    id int auto_increment
    primary key,
    created_on datetime(3) not null,
    modified_on datetime(3) not null,
    uuid varchar(36) not null,
    role_id int not null,
    user_id int not null,
    constraint idx_user_id_role_id
    unique (user_id, role_id)
    )
    charset=utf8mb4;