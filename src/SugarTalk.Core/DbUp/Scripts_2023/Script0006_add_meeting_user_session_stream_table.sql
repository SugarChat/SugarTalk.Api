create table if not exists meeting_user_session_stream
(
    id int auto_increment
    primary key,
    `meeting_user_session_id` int not null,
    `stream_id` varchar(128) not null,
    `stream_type` int not null
    )
    charset=utf8mb4;
