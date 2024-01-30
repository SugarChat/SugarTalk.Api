alter table `meeting_speech` drop column `voice_url`;
alter table `meeting_speech` drop column `translated_text`;

ALTER TABLE `meeting_user_session`
    DROP COLUMN `name`,
    DROP COLUMN `picture_url`,
    ADD COLUMN `status` INT NOT NULL DEFAULT 0,
    ADD COLUMN `is_deleted` TINYINT(1) NOT NULL DEFAULT 0,
    ADD COLUMN `first_join_time` BIGINT NULL,
    ADD COLUMN `last_quit_time` BIGINT NULL,
    ADD COLUMN `total_join_count` INT NULL,
    ADD COLUMN `cumulative_time` BIGINT NULL;

drop table `meeting_user_session_stream`;

create table if not exists `meeting_record`
(
    `id` varchar(36) not null primary key,
    `meeting_id` varchar(36) not null,
    `url` varchar(512) not null,
    `created_date` datetime(3) not null
    )
    charset=utf8mb4;