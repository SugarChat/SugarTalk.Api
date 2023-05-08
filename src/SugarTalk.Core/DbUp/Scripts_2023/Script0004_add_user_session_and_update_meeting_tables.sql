create table if not exists meeting_user_session
(
    id int auto_increment
    primary key,
    `created_date` datetime(3) not null,
    `meeting_id` varchar(36) not null,
    `user_id` int not null,
    `is_muted` tinyint(1) default 0 null,
    `is_sharing_screen` tinyint(1) default 0 null
    )
    charset=utf8mb4;

create table if not exists meeting_user_session_stream
(
    `id` int auto_increment
    primary key,
    `user_session_id` int not null,
    `room_stream_id` varchar(128) not null
    )
    charset=utf8mb4;

alter table `meeting` add `start_date` int not null;
alter table `meeting` add `end_date` int not null;