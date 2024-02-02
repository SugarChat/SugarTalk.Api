drop table `meeting_period_rule`;

create table if not exists `meeting_repeat_rule`
(
    `id` varchar(36) not null primary key,
    `meeting_id` varchar(36) not null,
    `repeat_type` int not null,
    `repeat_until_date` bigint null
    )