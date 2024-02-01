alter table `meeting` 
    drop column `period_type`,
    add column `appointment_type` int not null default 1;

create table if not exists `meeting_period_rule`
(
    `id` varchar(36) not null primary key,
    `meeting_id` varchar(36) not null,
    `period_type` int not null,
    `until_date` datetime(3) null
    )
charset=utf8mb4;

create table if not exists `meeting_sub_meeting`
(
    `id` varchar(36) not null primary key,
    `meeting_id` varchar(36) not null,
    `status` int not null,
    `start_time` bigint not null,
    `end_time` bigint not null
    )
charset=utf8mb4;